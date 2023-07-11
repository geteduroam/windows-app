using EduRoam.Connect.Exceptions;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.Connect
{
    public class OAuthHandler
    {
        private readonly IdentityProviderProfile profile;
        private readonly Uri prefix;
        private readonly Uri authUri;
        private readonly OAuth oauth;

        private ManualResetEvent? cancelThread;
        private CancellationTokenSource? cancelTokenSource;
        private ManualResetEvent? mainThread;

        public OAuthHandler(IdentityProviderProfile profile)
        {
            this.profile = profile;
            this.oauth = new OAuth(new Uri(profile?.authorization_endpoint));
            // The url to send the user to
            this.authUri = oauth.CreateAuthUri();
            // The url to listen to for the user to be redirected back to
            this.prefix = oauth.GetRedirectUri();
        }

        public EapConfig? EapConfig { get; private set; }

        public async Task Handle()
        {
            // starts HTTP listener in new thread so UI stays responsive
            var listenerThread = new Thread(NonblockingListener)
            {
                IsBackground = true
            };
            listenerThread.Start();
            // cancellation thread
            this.cancelThread = new ManualResetEvent(false);
            // creates cancellation token, used when cancelling BeginGetContext method
            this.cancelTokenSource = new CancellationTokenSource();
            // wait until oauth done and continue process
            await Task.Run(listenerThread.Join);
        }

        /// <summary>
		/// Listens for incoming HTTP requests.
		/// </summary>
		private void NonblockingListener() //, AsyncCallback listenerCallback, ManualResetEvent mainThread, ManualResetEvent cancelThread)
        {
            // creates a listener
            using var listener = new HttpListener();
            // add prefix to listener
            listener.Prefixes.Add(prefix.ToString());
            // starts listener
            listener.Start();

            // creates BeginGetContext task for retrieving HTTP request
            IAsyncResult result = listener.BeginGetContext(ListenerCallback, listener);
            // opens authentication URI in default browser
            var startInfo = new ProcessStartInfo()
            {
                FileName = authUri.ToString(),
                LoadUserProfile = true,
                UseShellExecute = true
            };
            Process.Start(startInfo);
            //Process.Start(authUri.ToString());

            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;

            //result.AsyncWaitHandle.WaitOne();
            // creates WaitHandle array with two tasks: BeginGetContext and cancel thread
            WaitHandle[] handles = { result.AsyncWaitHandle, cancelThread! };
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
        }

        /// <summary>
		/// Callback function for incoming HTTP requests.
		/// </summary>
		/// <param name="result">Result of BeginGetContext task.</param>
		private async void ListenerCallback(IAsyncResult result)
        {
            // cancels and returns if cancellation is requested
            if (cancelTokenSource == null || cancelTokenSource.Token.IsCancellationRequested) return;

            // sets the callback listener equals to the http listener
            HttpListener callbackListener = (HttpListener)result.AsyncState;

            // calls EndGetContext to complete the asynchronous operation
            HttpListenerContext context = callbackListener.EndGetContext(result);
            HttpListenerRequest request = context.Request;

            // gets the URL of the target web site
            var responseUrl = request.Url;

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
                    byte[] responseString = Encoding.ASCII.GetBytes(authorizationCode == null
                        ? Properties.Resources.oauth_rejected
                        : Properties.Resources.oauth_accepted);

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

                this.EapConfig = success ? await LetsWifi.RequestAndDownloadEapConfig() : null;
            }
            catch (ApiUnreachableException e) // TODO: BAD
            {
                Debug.Print(e.ToString());
                //MessageBox.Show(
                //    "Couldn't connect to the server.\n\n" +
                //    "Make sure that you are connected to the internet, then try again.\n" +
                //    "Exception: " + e.Message,
                //    "ApiUnreachableException", MessageBoxButton.OK, MessageBoxImage.Error);
                this.EapConfig = null;
            }
            catch (ApiParsingException e) // TODO: BAD
            {
                Debug.Print(e.ToString());
                //MessageBox.Show(
                //    "The institution or profile is either not supported or malformed. " +
                //    "Please select a different institution or profile.\n\n" +
                //    "Exception: " + e.Message,
                //    "ApiParsingException", MessageBoxButton.OK, MessageBoxImage.Error);
                this.EapConfig = null;
            } finally
            {
                // resumes main thread
                mainThread?.Set();
            }            
        }
    }
}
