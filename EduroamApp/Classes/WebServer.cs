using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		private static readonly HttpListener listener = new HttpListener();
		private static readonly frmWaitForAuthenticate waitingDialog = new frmWaitForAuthenticate();

		public static string NonblockingListener(string prefix, string oAuthUri)
		{
			listener.Prefixes.Add(prefix);

			listener.Start();
			IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
			// Applications can do some work here while waiting for the
			// request. If no work can be done until you have processed a request,
			// use a wait handle to prevent this thread from terminating
			// while the asynchronous operation completes.

			Process.Start(oAuthUri);
			waitingDialog.Show();

			result.AsyncWaitHandle.WaitOne();

			waitingDialog.Close();
			//MessageBox.Show("Request processed asyncronously.");
			listener.Close();
			return responseUrl;
		}

		private static void ListenerCallback(IAsyncResult result)
		{
			HttpListener callbackListener = (HttpListener)result.AsyncState;
			// Call EndGetContext to complete the asynchronous operation.
			HttpListenerContext context = callbackListener.EndGetContext(result);
			HttpListenerRequest request = context.Request;
			responseUrl = request.Url.OriginalString;
			// Obtain a response object.
			HttpListenerResponse response = context.Response;
			// Construct a response.
			string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
			// Get a response stream and write the response to it.
			response.ContentLength64 = buffer.Length;
			System.IO.Stream output = response.OutputStream;
			output.Write(buffer, 0, buffer.Length);
			// You must close the output stream.
			output.Close();
		}

		public static void CancelListener()
		{
			waitingDialog.Close();
			listener.Close();
		}

	}
}
