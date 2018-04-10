using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DiReCT.Network
{
    /// <summary>
    /// Provides simple asynchronous send and receive APIs
    /// </summary>
    public class CommunicationBase : IDisposable
    {
        private Socket mSocket;
        private MemoryStream SendStream;
        private MemoryStream ReceiveStream;
        private static ManualResetEvent ReceiveDone =
            new ManualResetEvent(false);
        private static ManualResetEvent SendDone =
            new ManualResetEvent(false);

        public CommunicationBase(Socket socket)
        {
            mSocket = socket;
        }

        /// <summary>
        /// Receive data send by the target.
        /// </summary>
        public byte[] Receive()
        {
            CommunicationObject CommunicationState = 
                 new CommunicationObject();
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
                ReadBytes = SendStream.Read(Buffer, 0,
                    CommunicationObject.BufferSize);

                mSocket.BeginSend(Buffer, 0, ReadBytes, SocketFlags.None,
                    new AsyncCallback(SendCallback), mSocket);

                SendDone.WaitOne();
            }
            while (ReadBytes > 0);

            SendStream.Dispose();

            SendDone.Reset();

            // Send end of file message
            // Notify the end of the data stream
            mSocket.BeginSend(Encoding.ASCII.GetBytes("<EOF>"),
                0, 0x05, SocketFlags.None,
                    new AsyncCallback(SendCallback), mSocket);

            SendDone.WaitOne();
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
                if (bytesRead == 0x05)
                {
                    byte[] Buffer = new byte[5];
                    Array.Copy(CommunicationState.Buffer, Buffer, bytesRead);

                    // Check the end of file message to determine the
                    // end of the data stream
                    if (Encoding.ASCII.GetString(Buffer) == "<EOF>")
                        ReceiveDone.Set();
                    else
                    {
                        ReceiveStream.Write(CommunicationState.Buffer,
                            0, bytesRead);

                        TargetSocket.BeginReceive(CommunicationState.Buffer,0,
                            CommunicationObject.BufferSize, 0,
                            new AsyncCallback(ReceiveCallback),
                            CommunicationState);
                    }
                }
                else
                {
                    ReceiveStream.Write(
                        CommunicationState.Buffer,
                        0,
                        bytesRead);

                    TargetSocket.BeginReceive(CommunicationState.Buffer, 0,
                       CommunicationObject.BufferSize, 0,
                       new AsyncCallback(ReceiveCallback),CommunicationState);
                }
            }
        }

        /// <summary>
        /// Callback when a part of the Data 
        /// has been sent to the targets successfully.
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            Socket handler = null;
            try
            {
                handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);

                SendDone.Set();
            }
            catch (ArgumentException argEx)
            {
                Debug.WriteLine(argEx.Message);
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
    }
}
