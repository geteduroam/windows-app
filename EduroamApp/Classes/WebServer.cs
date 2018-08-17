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
        private static HttpListener listener;
        public static frmWaitForAuthenticate waitingDialog;
        
        private static bool isCanceled = false;

        public delegate void dgEventRaiser();

        //public event dgEventRaiser btnCancel_Click;

        public static string NonblockingListener(string prefix, string oAuthUri)
        {
            waitingDialog = new frmWaitForAuthenticate();
            waitingDialog.BtnCancel.Click += new System.EventHandler((sender, args) => CancelListener());

            listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            
            listener.Start();
            IAsyncResult httpResult = listener.BeginGetContext(ListenerCallback, listener);
            /*IAsyncResult cancelResult = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
            WaitHandle[] results = new WaitHandle[]
            {
                httpResult,
                cancelResult
            };*/
            

            // Applications can do some work here while waiting for the 
            // request. If no work can be done until you have processed a request,
            // use a wait handle to prevent this thread from terminating
            // while the asynchronous operation completes.

            Process.Start(oAuthUri);
            waitingDialog.Show();
            
            //int index = WaitHandle.WaitAny(waitHandles);
            // The time shown below should match the shortest task.
            //Console.WriteLine("Task {0} finished first (time waited={1}).",
            //    index + 1, (DateTime.Now - dt).TotalMilliseconds);
            httpResult.AsyncWaitHandle.WaitOne();
            
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
            // gets the URL of the target web site
            responseUrl = request.Url.OriginalString;

            using (HttpListenerResponse response = context.Response)
            {
                // Construct a response.
                string responseString = "<HTML><BODY>Feide has been authorized. <br />You can now close this tab.</BODY></HTML>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.LongLength;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            /*
            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            // Construct a response.
            string responseString = "<HTML><BODY>Feide has been authorized. <br />You can now close this tab.</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();
            */
            
        }

        // Define an array with two AutoResetEvent WaitHandles.
        static WaitHandle[] waitHandles = new WaitHandle[]
        {
            new AutoResetEvent(false),
            new AutoResetEvent(false)
        };



        public void BeginListeningToCancel()
        {
            

        }

        void HandleCustomEvent(object sender, EventArgs e)
        {
            waitingDialog.Close();
        }

        public static void CancelListener()
        {
            waitingDialog.Close();
            isCanceled = true;
            listener.Close();
        }
        
    }
}
