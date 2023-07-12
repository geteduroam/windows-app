using EduRoam.Connect.Exceptions;

using System.Diagnostics;
using System.Net;
using System.Text;

namespace EduRoam.Connect
{
    public class OAuthHandler
    {
        private readonly IdentityProviderProfile profile;
        private readonly Uri prefix;
        private readonly Uri authUri;
        private readonly OAuth oauth;

        public OAuthHandler(IdentityProviderProfile profile)
        {
            if (!profile.oauth)
            {
                throw new ArgumentException("Profile is not targeted for OAuth", nameof(profile));
            }
            if (string.IsNullOrWhiteSpace(profile.authorization_endpoint))
            {
                throw new ArgumentException("The profile does not contain a valid authorization endpoint");
            }

            this.profile = profile;
            this.oauth = new OAuth(new Uri(profile.authorization_endpoint));
            // The url to send the user to
            this.authUri = oauth.CreateAuthUri();
            // The url to listen to for the user to be redirected back to
            this.prefix = oauth.GetRedirectUri();
        }

        public EapConfig? EapConfig { get; private set; }

        /// <summary>
        /// Cancellation thread (Optional)
        /// </summary>
        private ManualResetEvent? CancelThread { get; set; }

        /// <summary>
        /// Creates cancellation token, used when cancelling BeginGetContext method. (Optional)
        /// </summary>
        private CancellationTokenSource? CancelTokenSource { get; set; }

        /// <summary>
        /// Main thread (Optional)
        /// </summary>
        private ManualResetEvent? mainThread { get; set; }

        public async Task Handle()
        {
            // starts HTTP listener in new thread so UI stays responsive
            var listenerThread = new Thread(NonblockingListener)
            {
                IsBackground = true
            };
            listenerThread.Start();
            
            // wait until oauth done and continue process
            await Task.Run(listenerThread.Join);
        }

        /// <summary>
		/// Listens for incoming HTTP requests.
		/// </summary>
		private void NonblockingListener()
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
                UseShellExecute = true,
                
            };

            using var process = Process.Start(startInfo);

            var processThread = new ManualResetEvent(false);
            process!.Exited += (object? sender, EventArgs e) => { this.Process_Exited(processThread); };
            process!.EnableRaisingEvents = true;

            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;

            // creates WaitHandle array with two or three tasks: BeginGetContext, Process thread and optionally cancel thread
            var handles = new List<WaitHandle>() { result.AsyncWaitHandle, processThread };
            if (this.CancelThread != null)
            {
                handles.Add(this.CancelThread);
            }
            // waits for any task in the handles list to complete, gets array index of the first one to complete
            int handleResult = WaitHandle.WaitAny(handles.ToArray());

            // if BeginGetContext completes first
            if (handleResult == 0)
            {
                // freezes main thread so ListenerCallback function can finish
                if (this.mainThread != null)
                {
                    mainThread.WaitOne();
                }
            }

            // closes HTTP listener
            listener.Close();
        }

        private void Process_Exited(ManualResetEvent processThread)
        {
            var result = processThread.Set();
            var waitResult = processThread.WaitOne();
            ConsoleExtension.WriteError($"Browser closed before OAuth process was succesfully finished. ({result}, {waitResult})");
        }

        /// <summary>
        /// Callback function for incoming HTTP requests.
        /// </summary>
        /// <param name="result">Result of BeginGetContext task.</param>
        private async void ListenerCallback(IAsyncResult result)
        {
            // cancels and returns if cancellation is requested
            if (this.CancelTokenSource != null && this.CancelTokenSource.Token.IsCancellationRequested) return;

            // sets the callback listener equals to the http listener
            HttpListener callbackListener = (HttpListener)result.AsyncState;

            if (!callbackListener.IsListening)
            {
                return;
            }

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
                if (this.mainThread != null)
                {
                    // resumes main thread
                    this.mainThread?.Set();
                }
            }            
        }
    }
}
