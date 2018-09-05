using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace EduroamApp
{
	public partial class frmWaitDialog : Form
	{
		// localhost address, for example "http://localhost:8080/"
		private readonly string prefix;
		// URI to open in browser for authentication
		private readonly string oAuthUri;

		// return value
		public string responseUrl = "";
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

		public frmWaitDialog(string inPrefix, string inOAuthUri)
		{
			// sets parameters
			prefix = inPrefix;
			oAuthUri = inOAuthUri;

			InitializeComponent();
		}


		private void frmWaitForAuthenticate_Load(object sender, EventArgs e)
		{
			// starts HTTP listener in new thread so UI stays responsive
			listenerThread = new Thread(NonblockingListener);
			listenerThread.Start();
			// cancellation thread
			cancelThread = new ManualResetEvent(false);
			// creates cancellation token, used when cancelling BeginGetContext method
			cancelTokenSource = new CancellationTokenSource();
			cancelToken = cancelTokenSource.Token;
		}

		private void btnCancel_Click(object sender, EventArgs e)
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
			listener.Prefixes.Add(prefix);
			// starts listener
			listener.Start();

			// creates BeginGetContext task for retrieving HTTP request
			IAsyncResult result = listener.BeginGetContext(ListenerCallback, listener);
			// opens authentication URI in default browser
			Process.Start(oAuthUri);

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
				DialogResult = DialogResult.OK;
			}
			// if cancelled first
			else
			{
				// needs to call ListenerCallback once to cancel it
				ListenerCallback(null);
			}

			// closes HTTP listener
			listener.Close();
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
		/// Makes the form window immovable.
		/// </summary>
		/// <param name="message"></param>
		protected override void WndProc(ref Message message)
		{
			const int WM_SYSCOMMAND = 0x0112;
			const int SC_MOVE = 0xF010;

			switch (message.Msg)
			{
				case WM_SYSCOMMAND:
					int command = message.WParam.ToInt32() & 0xfff0;
					if (command == SC_MOVE)
						return;
					break;
			}

			base.WndProc(ref message);
		}
	}
}
