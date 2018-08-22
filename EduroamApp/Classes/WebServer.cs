using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace EduroamApp
{
	public class WebServer
	{
		// return value
		private static string responseUrl = "";
		// main thread event
		private static readonly ManualResetEvent mainThread = new ManualResetEvent(false);
		// cancel thread event
		private static ManualResetEvent cancelThread;
		// cancellation token source
		private static  CancellationTokenSource cancelSource;
		// cancellation token
		private static CancellationToken cancelToken;

		/// <summary>
		/// Listens for incoming HTTP requests.
		/// </summary>
		/// <param name="prefix">Localhost address, for example "http://localhost:8080/".</param>
		/// <param name="oAuthUri">URI to open in browser for authentication.</param>
		/// <param name="parentLocation">On-screen location of parent form.</param>
		/// <returns>URL of request after authorization.</returns>
		public static string NonblockingListener(string prefix, string oAuthUri, Point parentLocation)
		{
			// instantiates waiting dialog form
			var waitingDialog = new frmWaitForAuthenticate("", "", parentLocation);

			// creates new thread for opening waiting dialog form
			// necessary to avoid UI blocking when waiting for incoming HTTP request
			var dialogThread = new Thread(() => waitingDialog.ShowDialog());
			// creates cancellation token, used when cancelling BeginGetContext method
			cancelSource = new CancellationTokenSource();
			cancelToken = cancelSource.Token;
			// instantiates wait for cancellation event
			cancelThread = new ManualResetEvent(false);

			// creates a listener
			var listener = new HttpListener();
			// add prefix to listener
			listener.Prefixes.Add(prefix);
			// starts listener
			listener.Start();

			// creates BeginGetContext task for retrieving HTTP request
			IAsyncResult result = listener.BeginGetContext(ListenerCallback, listener);
			// opens authentication URI in default browser
			Process.Start(oAuthUri);
			// starts the waiting dialog thread
			dialogThread.Start();

			// creates WaitHandle array with two tasks: BeginGetContext and wait for cancel
			WaitHandle[] handles = { result.AsyncWaitHandle, cancelThread };
			// waits for both tasks to complete, gets array index of the first one to complete
			int handleResult = WaitHandle.WaitAny(handles);

			// if BeginGetContext completes first
			if (handleResult == 0)
			{
				// freezes main thread so ListenerCallback function can finish
				mainThread.WaitOne();
				// closes waiting dialog
				waitingDialog.Invoke((MethodInvoker)delegate { waitingDialog.Close(); });
			}
			// if cancelled first
			else
			{
				// sets cancellation token to cancel
				cancelSource.Cancel();
				// needs to call ListenerCallback once to cancel it
				ListenerCallback(null);
				// sets response url string
				responseUrl = "CANCEL";
			}

			// closes HTTP listener
			listener.Close();
			// returns response url
			return responseUrl;
		}

		/// <summary>
		/// Callback function for incoming HTTP requests.
		/// </summary>
		/// <param name="result">Result of BeginGetContext task.</param>
		private static void ListenerCallback(IAsyncResult result)
		{
			// cancels and returns if cancellation is requested
			if (cancelToken.IsCancellationRequested) return;

			// sets the callback listener equals to the http listener
			HttpListener callbackListener = (HttpListener)result.AsyncState;

			// calls EndGetContext to complete the asynchronous operation
			HttpListenerContext context = callbackListener.EndGetContext(result);
			HttpListenerRequest request = context.Request;

			// gets the URL of the target web site
			responseUrl = request.Url.OriginalString;

			try
			{
				using (HttpListenerResponse response = context.Response)
				{
					// constructs a response
					string responseString = responseUrl.Contains("access_denied")
						? "<HTML><BODY>You rejected the authorization. Please go back to the Eduroam app. <br />You can now close this tab.</BODY></HTML>"
						: "<HTML><BODY>Feide has been authorized. <br />You can now close this tab.</BODY></HTML>";

					// outputs response to web server
					byte[] buffer = Encoding.UTF8.GetBytes(responseString);
					response.ContentLength64 = buffer.Length;
					Stream output = response.OutputStream;
					output.Write(buffer, 0, buffer.Length);
				}
			}
			catch (HttpListenerException ex)
			{
				MessageBox.Show("Could not write to server. \nException: " + ex.Message);
			}

			// resumes main thread
			mainThread.Set();
		}

		/// <summary>
		/// Gets called when btnCancel on frmWaitForAuthenticate is clicked.
		/// </summary>
		public static void CancelListener()
		{
			// resumes cancel thread
			cancelThread.Set();
		}

	}
}
