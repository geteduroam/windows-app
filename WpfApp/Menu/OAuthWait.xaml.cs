using System;
using System.Linq;
using System.Text;
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
        // cancellation token
        private static CancellationToken cancelToken;
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
            listenerThread = new Thread(NonblockingListener);
            listenerThread.IsBackground = true;
            listenerThread.Start();
            // cancellation thread
            cancelThread = new ManualResetEvent(false);
            // creates cancellation token, used when cancelling BeginGetContext method
            cancelTokenSource = new CancellationTokenSource();
            cancelToken = cancelTokenSource.Token;
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
        public void NonblockingListener()
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
            // if cancelled first
            else
            {
                // needs to call ListenerCallback once to cancel it
                ListenerCallback(null);
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
        private void ListenerCallback(IAsyncResult result)
        {
            // cancels and returns if cancellation is requested
            if (cancelToken.IsCancellationRequested) return;

            // sets the callback listener equals to the http listener
            HttpListener callbackListener = (HttpListener)result.AsyncState;

            // calls EndGetContext to complete the asynchronous operation
            HttpListenerContext context = callbackListener.EndGetContext(result);
            HttpListenerRequest request = context.Request;

            // gets the URL of the target web site
            responseUrl = request.Url;

            try
            {

                // Parse the result and download the eap config if successfull
                (string authorizationCode, string codeVerifier) = oauth.ParseAndExtractAuthorizationCode(responseUrl);
                bool success = LetsWifi.AuthorizeAccess(profile, authorizationCode, codeVerifier, prefix);

                eapConfig = success ? LetsWifi.RequestAndDownloadEapConfig() : null;
            }
            catch (EduroamAppUserError)
            {
                eapConfig = null;
            }

            try
            {
                using HttpListenerResponse response = context.Response;
                // constructs a response
                string responseString = eapConfig == null
                    ? Properties.Resources.oauth_rejected
                    : Properties.Resources.oauth_accepted;

                // outputs response to web server
                byte[] responseBytes = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = responseBytes.Length;
                response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
            }
            catch (HttpListenerException ex)
            {
                MessageBox.Show("Could not write to server. \nException: " + ex.Message);
            }

            // resumes main thread


            mainThread.Set();
            this.Dispatcher.Invoke(() => {
                mainWindow.OAuthComplete(eapConfig);
            });
        }
    }
}
