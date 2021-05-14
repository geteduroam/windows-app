using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Net;
using System.Threading;
using EduroamConfigure;

namespace WpfApp.Menu
{
    /// <summary>
    /// Interaction logic for OAuthWait.xaml
    /// </summary>
    public partial class OAuthWait : Page
    {
        private MainWindow mainWindow;
        // localhost address, for example "http://localhost:8080/"
        private readonly Uri prefix;
        // URI to open in browser for authentication
        private readonly Uri authUri;
        private readonly OAuth oauth;
        private readonly IdentityProviderProfile profile;

        // return value
        public Uri responseUrl { get; private set; }
        // main thread event
        private static ManualResetEvent mainThread;
        // cancel thread event
        private static ManualResetEvent cancelThread;
        //
        private Thread listenerThread;
        // cancellation token source
        private static CancellationTokenSource cancelTokenSource;
        public EapConfig eapConfig { get; set; }

        public OAuthWait(MainWindow mainWindow, IdentityProviderProfile profile) 
        {
            this.mainWindow = mainWindow;
            this.profile = profile;
            this.oauth = new OAuth(new Uri(profile?.authorization_endpoint));
            // The url to send the user to
            authUri = oauth.CreateAuthUri();
            // The url to listen to for the user to be redirected back to
            prefix = oauth.GetRedirectUri();
            InitializeComponent();
            Load();
        }


        private async void Load()
        {
            // starts HTTP listener in new thread so UI stays responsive
            listenerThread = new Thread(NonblockingListener)
            {
                IsBackground = true
            };
            listenerThread.Start();
            // cancellation thread
            cancelThread = new ManualResetEvent(false);
            // creates cancellation token, used when cancelling BeginGetContext method
            cancelTokenSource = new CancellationTokenSource();
            // wait until oauth done and continue process
            await Task.Run(listenerThread.Join);
            //mainWindow.OAuthComplete(eapConfig);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // sets cancellation token to cancel
            cancelTokenSource.Cancel();
            // resumes cancel thread
            cancelThread.Set();
        }

        /// <summary>
        /// Tells the http server to turn off
        /// </summary>
        public static void CancelThread()
        {
            // sets cancellation token to cancel
            cancelTokenSource.Cancel();
            // resumes cancel thread
            cancelThread.Set();
        }

        /// <summary>
        /// Listens for incoming HTTP requests.
        /// </summary>
        private void NonblockingListener()
        {
            // creates a listener
            var listener = new HttpListener();
            // add prefix to listener
            listener.Prefixes.Add(prefix.ToString());
            // starts listener
            listener.Start();

            // creates BeginGetContext task for retrieving HTTP request
            IAsyncResult result = listener.BeginGetContext(ListenerCallback, listener);
            // opens authentication URI in default browser
            Process.Start(authUri.ToString());

            //result.AsyncWaitHandle.WaitOne();
            // creates WaitHandle array with two tasks: BeginGetContext and cancel thread 
            WaitHandle[] handles = { result.AsyncWaitHandle, cancelThread };
            // waits for both tasks to complete, gets array index of the first one to complete
            int handleResult = WaitHandle.WaitAny(handles);

            // if BeginGetContext completes first
            if (handleResult == 0)
            {
                // freezes main thread so ListenerCallback function can finish
                mainThread = new ManualResetEvent(false);
                mainThread.WaitOne();
                //DialogResult = DialogResult.OK;
            }

            // closes HTTP listener
            listener.Close();
            // download eapconfig using response from oauth process

            //mainWindow.Activate();
           
        }

        /// <summary>
        /// Callback function for incoming HTTP requests.
        /// </summary>
        /// <param name="result">Result of BeginGetContext task.</param>
        private async void ListenerCallback(IAsyncResult result)
        {
            // cancels and returns if cancellation is requested
            if (cancelTokenSource.Token.IsCancellationRequested) return;

            // sets the callback listener equals to the http listener
            HttpListener callbackListener = (HttpListener)result.AsyncState;

            // calls EndGetContext to complete the asynchronous operation
            HttpListenerContext context = callbackListener.EndGetContext(result);
            HttpListenerRequest request = context.Request;

            // gets the URL of the target web site
            responseUrl = request.Url;

            // Parse the result and download the eap config if successfull
            string authorizationCode = null;
            string codeVerifier;
            try
            {
                (authorizationCode, codeVerifier) = oauth.ParseAndExtractAuthorizationCode(responseUrl);
            }
            finally
            {
                try
                {
                    using HttpListenerResponse response = context.Response;
                    // constructs a response
                    byte[] responseString = authorizationCode == null
                        ? Properties.Resources.oauth_rejected
                        : Properties.Resources.oauth_accepted;

                    // outputs response to web server
                    response.ContentLength64 = responseString.Length;
                    response.OutputStream.Write(responseString, 0, responseString.Length);
                    response.Close();
                }
                catch (HttpListenerException e)
                {
                    Debug.Print("{0} occurred replying {1} to the webbrowser", e.GetType(), authorizationCode == null ? "REJECT" : "OK");
                }
            }

            try
            {
                bool success = await LetsWifi.AuthorizeAccess(profile, authorizationCode, codeVerifier, prefix);

                eapConfig = success ? await LetsWifi.RequestAndDownloadEapConfig() : null;
            }
            catch (ApiUnreachableException e) // TODO: BAD
            {
                Debug.Print(e.ToString());
                MessageBox.Show(
                    "Couldn't connect to the server.\n\n" +
                    "Make sure that you are connected to the internet, then try again.\n" +
                    "Exception: " + e.Message,
                    "ApiUnreachableException", MessageBoxButton.OK, MessageBoxImage.Error);
                eapConfig = null;
            }
            catch (ApiParsingException e) // TODO: BAD
            {
                Debug.Print(e.ToString());
                MessageBox.Show(
                    "The institution or profile is either not supported or malformed. " +
                    "Please select a different institution or profile.\n\n" +
                    "Exception: " + e.Message,
                    "ApiParsingException", MessageBoxButton.OK, MessageBoxImage.Error);
                eapConfig = null;
            }

            // resumes main thread
            mainThread.Set();
            Dispatcher.Invoke(() => {
                mainWindow.OAuthComplete(eapConfig);
            });
        }
    }
}
