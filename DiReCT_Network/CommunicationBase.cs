using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using DiReCT.Logger;

namespace DiReCT.Network
{
    public class CommunicationBase : IDisposable
    {
        private Socket mSocket;
        private MemoryStream SendStream;
        private MemoryStream ReceiveStream;
        private static ManualResetEvent ReceiveDone =
            new ManualResetEvent(false);
        private static ManualResetEvent SendDone =
            new ManualResetEvent(false);

        private static int signal;

        public CommunicationBase(Socket socket)
        {
            mSocket = socket;
        }

        /// <summary>
        /// Receive data send by the target.
        /// </summary>
        public byte[] Receive()
        {
            CommunicationObject CommunicationState = new CommunicationObject();
            ReceiveStream = new MemoryStream();
            CommunicationState.WorkSocket = mSocket;
            ReceiveDone.Reset();

            mSocket.BeginReceive(CommunicationState.Buffer, 0, 
                CommunicationObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), CommunicationState);
            ReceiveDone.WaitOne();

            CommunicationState.Buffer = new byte[ReceiveStream.Length];
            Array.Copy(ReceiveStream.GetBuffer(),
                CommunicationState.Buffer,
                ReceiveStream.Length);

            ReceiveStream.Dispose();
            return CommunicationState.Buffer;
        }

        /// <summary>
        /// Send data to the target.
        /// </summary>
        /// <param name="Value"></param>
        public void Send(byte[] Value)
        {
            int ReadBytes = 0;
            SendStream = new MemoryStream(Value);
            byte[] Buffer = new byte[CommunicationObject.BufferSize];

            do
            {
                SendDone.Reset();
                signal = 0;
                ReadBytes = SendStream.Read(Buffer, 0, 
                    CommunicationObject.BufferSize);

                Socket handler = mSocket;

                handler.BeginSend(Buffer, 0, ReadBytes, SocketFlags.None,
                    new AsyncCallback(SendCallback), handler);

                SendDone.WaitOne();
            }
            while (ReadBytes > 0);

            SendStream.Dispose();
        }

        /// <summary>
        /// Callback when receive a data chunk from the target successfully.
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            CommunicationObject CommunicationState 
                = (CommunicationObject)ar.AsyncState;
            Socket TargetSocket = CommunicationState.WorkSocket;

            int bytesRead = TargetSocket.EndReceive(ar);
            if (bytesRead > 0)
            {
                ReceiveStream.Write(CommunicationState.Buffer, 0, bytesRead);

                TargetSocket.BeginReceive(CommunicationState.Buffer, 0,
                    CommunicationObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), CommunicationState);
            }
            else
            {
                // Signal if all the file received.
                ReceiveDone.Set();
            }
        }

        /// <summary>
        /// Callback when a part of the Data 
        /// has been sent to the targets successfully.
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

                // Close the socket when all the data has sent to the target.
                if (bytesSent == 0)
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (ArgumentException argEx)
            {
                Log.ErrorEvent.Write(argEx.Message);
            }
            finally
            {
                // Signal when the file chunk has sent 
                // to all the target successfully.
                if (signal >= 1)
                {
                    SendDone.Set();
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

                ReceiveDone.Dispose();
                SendDone.Dispose();

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

    internal class CommunicationObject
    {
        public Socket WorkSocket = null;

        public const int BufferSize = 5242880;

        public byte[] Buffer = new byte[BufferSize];

        public bool Connected = false;
    }
}
