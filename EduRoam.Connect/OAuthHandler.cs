using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;

using System.Diagnostics;
using System.Net;
using System.Text;

namespace EduRoam.Connect
{
    internal class OAuthHandler
    {
        private readonly IdentityProviderProfile profile;
        private readonly Uri prefix;
        private readonly Uri authUri;
        private readonly OAuth oauth;

        internal OAuthHandler(IdentityProviderProfile profile)
        {
            if (!profile.OAuth)
            {
                throw new ArgumentException("Profile is not targeted for OAuth", nameof(profile));
            }
            if (string.IsNullOrWhiteSpace(profile.AuthorizationEndpoint))
            {
                throw new ArgumentException("The profile does not contain a valid authorization endpoint");
            }

            this.profile = profile;
            this.oauth = new OAuth(new Uri(profile.AuthorizationEndpoint));
            // The url to send the user to
            this.authUri = this.oauth.CreateAuthUri();
            // The url to listen to for the user to be redirected back to
            this.prefix = this.oauth.GetRedirectUri();
        }

        public EapConfig? EapConfig { get; private set; }

        /// <summary>
        /// Cancellation thread (Optional)
        /// </summary>
        public ManualResetEvent? CancelThread { get; set; }

        /// <summary>
        /// Creates cancellation token, used when cancelling BeginGetContext method. (Optional)
        /// </summary>
        private CancellationTokenSource? CancelTokenSource { get; set; }

        /// <summary>
        /// Main thread (Optional)
        /// </summary>
        public ManualResetEvent? MainThread { get; set; }

        public async Task Handle()
        {
            // starts HTTP listener in new thread so UI stays responsive
            var listenerThread = new Thread(this.NonblockingListener)
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
            try
            {
                // creates a listener
                using var listener = new HttpListener();
                // add prefix to listener
                listener.Prefixes.Add(this.prefix.ToString());
                // starts listener
                listener.Start();

                // creates BeginGetContext task for retrieving HTTP request
                var result = listener.BeginGetContext(this.ListenerCallback, listener);

                // opens authentication URI in default browser
                var startInfo = new ProcessStartInfo()
                {
                    FileName = this.authUri.ToString(),
                    LoadUserProfile = true,
                    UseShellExecute = true,
                };

                using var process = Process.Start(startInfo);

                var processThread = new ManualResetEvent(false);

                var context = listener.GetContext();
                var request = context.Request;

                // creates WaitHandle array with two or three tasks: BeginGetContext, Process thread and optionally cancel thread
                var handles = new List<WaitHandle>() { result.AsyncWaitHandle, processThread };
                if (this.CancelThread != null)
                {
                    handles.Add(this.CancelThread);
                }
                // waits for any task in the handles list to complete, gets array index of the first one to complete
                var handleResult = WaitHandle.WaitAny(handles.ToArray());

                // if BeginGetContext completes first
                if (handleResult == 0)
                {
                    // freezes main thread so ListenerCallback function can finish
                    this.MainThread?.WaitOne();
                }

                // closes HTTP listener
                listener.Close();
            }
            catch (HttpListenerException listenerExc)
            {
#if DEBUG
                Debugger.Break();
#endif
                Debug.WriteLine($"Could not open browser window\n{listenerExc.Message}");
            }
        }

        /// <summary>
        /// Callback function for incoming HTTP requests.
        /// </summary>
        /// <param name="result">Result of BeginGetContext task.</param>
        private async void ListenerCallback(IAsyncResult result)
        {
            // cancels and returns if cancellation is requested
            if (this.CancelTokenSource != null && this.CancelTokenSource.Token.IsCancellationRequested)
            {
                return;
            }

            // sets the callback listener equals to the http listener
            var callbackListener = (HttpListener?)result.AsyncState;

            if (callbackListener == null || !callbackListener.IsListening)
            {
                return;
            }

            // calls EndGetContext to complete the asynchronous operation
            var context = callbackListener.EndGetContext(result);
            var request = context.Request;

            // gets the URL of the target web site
            var responseUrl = request.Url;

            // Parse the result and download the eap config if successfull
            string? authorizationCode = null;
            string? codeVerifier;
            try
            {
                (authorizationCode, codeVerifier) = this.oauth.ParseAndExtractAuthorizationCode(responseUrl);
            }
            finally
            {
                try
                {
                    using var response = context.Response;
                    // constructs a response
                    var responseString = Encoding.ASCII.GetBytes(authorizationCode == null
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
                var success = await LetsWifi.Instance.AuthorizeAccess(this.profile, authorizationCode, codeVerifier, this.prefix);

                this.EapConfig = success ? await LetsWifi.Instance.RequestAndDownloadEapConfig() : null;
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
            }
            finally
            {
                if (this.MainThread != null)
                {
                    // resumes main thread
                    this.MainThread?.Set();
                }
            }
        }
    }
}
