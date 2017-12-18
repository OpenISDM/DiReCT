using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace DiReCT_Network
{
    public class CommunicationBase : IDisposable
    {
        private Socket mSocket;
        private MemoryStream SendStream;
        private MemoryStream ReceiveStream;
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);

        private static int signal;

        public CommunicationBase(Socket socket)
        {
            mSocket = socket;
        }

        /// <summary>
        /// Receive the Data send by the server.
        /// </summary>
        public byte[] Receive()
        {
            StateObject state = new StateObject();
            ReceiveStream = new MemoryStream();
            state.WorkSocket = mSocket;
            receiveDone.Reset();

            mSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
            receiveDone.WaitOne();

            state.Buffer = new byte[ReceiveStream.Length];
            Array.Copy(ReceiveStream.GetBuffer(),
                state.Buffer,
                ReceiveStream.Length);

            ReceiveStream.Dispose();
            return state.Buffer;
        }

        /// <summary>
        /// Send Data from server to clients.
        /// </summary>
        /// <param name="Value"></param>
        public void Send(byte[] Value)
        {
            int readBytes = 0;
            SendStream = new MemoryStream(Value);
            byte[] buffer = new byte[StateObject.BufferSize];

            do
            {
                sendDone.Reset();
                signal = 0;
                readBytes = SendStream.Read(buffer, 0, StateObject.BufferSize);

                Socket handler = mSocket;

                handler.BeginSend(buffer, 0, readBytes, SocketFlags.None,
                    new AsyncCallback(SendCallback), handler);

                sendDone.WaitOne();
            }
            while (readBytes > 0);

            SendStream.Dispose();
        }

        /// <summary>
        /// Callback when receive a Data chunk from the server successfully.
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket clientSocket = state.WorkSocket;

            int bytesRead = clientSocket.EndReceive(ar);
            if (bytesRead > 0)
            {
                ReceiveStream.Write(state.Buffer, 0, bytesRead);

                clientSocket.BeginReceive(state.Buffer, 0,
                    StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                // Signal if all the file received.
                receiveDone.Set();
            }
        }

        /// <summary>
        /// Callback when a part of the Data has been sent to the clients successfully.
        /// </summary>
        /// <param name="ar"></param>
        private static void SendCallback(IAsyncResult ar)
        {
            Socket handler = null;
            try
            {
                handler = (Socket)ar.AsyncState;
                signal++;
                int bytesSent = handler.EndSend(ar);

                // Close the socket when all the data has sent to the client.
                if (bytesSent == 0)
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (ArgumentException argEx)
            {
                Debug.WriteLine(argEx.Message);
            }
            catch (SocketException)
            {
                // Close the socket if the client disconnected.
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            finally
            {
                // Signal when the file chunk has sent to all the clients successfully. 
                if (signal >= 1)
                {
                    sendDone.Set();
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                receiveDone.Dispose();
                sendDone.Dispose();

                disposedValue = true;
            }
        }

        ~CommunicationBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    class StateObject
    {
        public Socket WorkSocket = null;

        public const int BufferSize = 5242880;

        public byte[] Buffer = new byte[BufferSize];

        public bool Connected = false;
    }
}
