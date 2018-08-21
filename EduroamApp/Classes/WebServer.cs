using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		private static string responseUrl = "";
		private static HttpListener listener;
		private static frmWaitForAuthenticate waitingDialog;
		private static bool isCanceled;
		private static readonly ManualResetEvent mainThread = new ManualResetEvent(false);
		private static readonly ManualResetEvent cancelThread = new ManualResetEvent(false);
		private static  CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		private static CancellationToken cancellationToken = cancellationTokenSource.Token;

		public static string NonblockingListener(string prefix, string oAuthUri)
		{
			waitingDialog = new frmWaitForAuthenticate();
			Thread dialogThread = new Thread(() => waitingDialog.ShowDialog());


			listener = new HttpListener();
			listener.Prefixes.Add(prefix);

			listener.Start();
			IAsyncResult result = listener.BeginGetContext(ListenerCallback, listener);
			Process.Start(oAuthUri);
			dialogThread.Start();

			WaitHandle[] handles = { result.AsyncWaitHandle, cancelThread };
			int handleResult = WaitHandle.WaitAny(handles);

			if (handleResult == 0)
			{
				mainThread.WaitOne();
				waitingDialog.Invoke((MethodInvoker)delegate { waitingDialog.Close(); });
			}
			else
			{
				cancellationTokenSource.Cancel();
				ListenerCallback(null);
				responseUrl = "CANCEL";
			}

			//result.AsyncWaitHandle;

			//result.AsyncWaitHandle.WaitOne();

			//if (!isCanceled)
			//{

			//MessageBox.Show("Request processed asyncronously.");
			//}

			//result.AsyncWaitHandle.Close();
			listener.Close();
			return responseUrl;
		}

		private static void ListenerCallback(IAsyncResult result)
		{
			if (cancellationToken.IsCancellationRequested) return;

			HttpListener callbackListener = (HttpListener)result.AsyncState;

			// Call EndGetContext to complete the asynchronous operation.
			HttpListenerContext context = callbackListener.EndGetContext(result);
			HttpListenerRequest request = context.Request;

			// gets the URL of the target web site
			responseUrl = request.Url.OriginalString;

			try
			{
				using (HttpListenerResponse response = context.Response)
				{
					// Construct a response.
					string responseString = responseUrl.Contains("access_denied")
						? "<HTML><BODY>You rejected the authorization. Please go back to the Eduroam app. <br />You can now close this tab.</BODY></HTML>"
						: "<HTML><BODY>Feide has been authorized. <br />You can now close this tab.</BODY></HTML>";

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

			mainThread.Set();
		}


		public static void CancelListener()
		{
			isCanceled = true;
			cancelThread.Set();
		}

	}
}
