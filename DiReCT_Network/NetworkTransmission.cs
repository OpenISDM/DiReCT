using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;
using DiReCT.Logger;

namespace DiReCT.Network
{
    public enum ReceiveDataType
    {
        Record,
        File,
        Error
    }

    public class NetworkServer : IDisposable
    {
        private Thread ListenServiceThread;
        private Socket Listener;
        private List<Socket> Clients = new List<Socket>();
        private List<Thread> ClientThreadList = new List<Thread>();
        private ManualResetEvent ConnectionEvent =
            new ManualResetEvent(false);
        private Dictionary<Socket, CommunicationBase>
            CommunicationDictionary =
            new Dictionary<Socket, CommunicationBase>();
        public ServerEvent Event = new ServerEvent();
        private bool MasterSwitch = true;
        private object ClientsLock = new object();

        public NetworkServer(int Port)
        {
            Listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(new IPEndPoint(IPAddress.Any, Port));
            ListenServiceThread = new Thread(ListenWork)
            {
                IsBackground = true
            };
        }

        public void Start()
        {
            ListenServiceThread.Start();
        }

        public void Join()
        {
            ListenServiceThread.Join();
            foreach (var q in ClientThreadList)
                q.Join();
        }

        private void ListenWork()
        {
            Listener.Listen(100);
            while (MasterSwitch)
            {
                ConnectionEvent.Reset();
                Listener.BeginAccept(new AsyncCallback(AcceptCallback),
                    Listener);

                ConnectionEvent.WaitOne();
            }
        }

        /// <summary>
        /// Callback when one client successfully connected to the server.
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            lock (ClientsLock)
            {
                Socket Client = listener.EndAccept(ar);
                Clients.Add(Client);
                CommunicationDictionary.Add(Client,
                    new CommunicationBase(Client));
                Thread ClientThread = new Thread(ListenClientRequest)
                {
                    IsBackground = true
                };
                ClientThread.Start(Client);
                ClientThreadList.Add(ClientThread);
            }

            ConnectionEvent.Set();
        }

        private void ListenClientRequest(object Client)
        {
            Socket ClientSock = (Client as Socket);
            CommunicationBase CB = CommunicationDictionary[ClientSock];
            string Json = string.Empty;
            byte[] Buffer;

            (ReceiveDataType, string, string) Request;
            try
            {
                while (MasterSwitch)
                {
                    Json = CB.Receive().DecodingString();
                    if (Json != string.Empty)
                    {
                        Request = Tools.DecodingRequest(Json);

                        if (Request.Item1 != ReceiveDataType.Error)
                        {
                            if (Request.Item1 == ReceiveDataType.File)
                            {
                                CB.Send(Encoding.Default.GetBytes("Start"));
                                Buffer = CB.Receive();
                                if (Buffer.CalculateMD5Value() ==
                                    Request.Item3)
                                {
                                    ServerReceiveEventArgs SREA =
                                        new ServerReceiveEventArgs
                                        {
                                            ReceiveDataType =
                                                ReceiveDataType.File,
                                            Data = Buffer
                                        };
                                    Event.ReceiveEventCall(SREA);
                                    CB.Send(Encoding.Default.GetBytes(
                                                "Success"));

                                    Event.MessageOutputCall(
                                        new MessageOutputEventArgs
                                        {
                                            Message = "IP: " +
                                            (ClientSock.RemoteEndPoint
                                            as IPEndPoint)
                                            .Address.ToString() +
                                            " Send a File"
                                        });

                                    
                                    Log.GeneralEvent.Write(
                                        "IP: " +
                                            (ClientSock.RemoteEndPoint
                                            as IPEndPoint)
                                            .Address.ToString() +
                                            " Send a File");
                                }
                                else
                                {
                                    CB.Send(Encoding.Default.GetBytes(
                                        "Fail"));

                                    Event.MessageOutputCall(
                                        new MessageOutputEventArgs
                                        {
                                            Message = "IP: " +
                                            (ClientSock.RemoteEndPoint
                                            as IPEndPoint)
                                            .Address.ToString() +
                                            " Send File Fail"
                                        });

                                    Log.GeneralEvent.Write(
                                        "IP: " +
                                            (ClientSock.RemoteEndPoint
                                            as IPEndPoint)
                                            .Address.ToString() +
                                            " Send File Fail");
                                }
                            }

                            if (Request.Item1 == ReceiveDataType.Record)
                            {
                                if (Request.Item1 == ReceiveDataType.Record)
                                {
                                    CB.Send(Encoding.Default.GetBytes("Start"));
                                    Buffer = CB.Receive();
                                    Json = Buffer.DecodingString();
                                    if (Json != string.Empty)
                                    {
                                        ServerReceiveEventArgs SREA =
                                            new ServerReceiveEventArgs
                                            {
                                                ReceiveDataType =
                                                    ReceiveDataType.Record,
                                                Record = Json
                                            };
                                        Event.ReceiveEventCall(SREA);
                                        CB.Send(Encoding.Default.GetBytes(
                                                "Success"));

                                        Event.MessageOutputCall(
                                            new MessageOutputEventArgs
                                            {
                                                Message = "IP: " +
                                                (ClientSock.RemoteEndPoint
                                                as IPEndPoint)
                                                .Address.ToString() +
                                                " Send one Record"
                                            });

                                        Log.GeneralEvent.Write(
                                            "IP: " +
                                                (ClientSock.RemoteEndPoint
                                                as IPEndPoint)
                                                .Address.ToString() +
                                                " Send one Record");
                                    }
                                    else
                                    {
                                        CB.Send(Encoding.Default.GetBytes(
                                            "Fail"));

                                        Event.MessageOutputCall(
                                            new MessageOutputEventArgs
                                            {
                                                Message = "IP: " +
                                                (ClientSock.RemoteEndPoint
                                                as IPEndPoint)
                                                .Address.ToString() +
                                                " Send Record Fail"
                                            });

                                        Log.GeneralEvent.Write( "IP: " +
                                                (ClientSock.RemoteEndPoint
                                                as IPEndPoint)
                                                .Address.ToString() +
                                                " Send Record Fail");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.ToString());
                Event.MessageOutputCall(
                    new MessageOutputEventArgs
                    {
                        Message = "IP: " +
                            (ClientSock.RemoteEndPoint as IPEndPoint)
                            .Address.ToString() + ex.ToString()
                    });
            }

            CommunicationDictionary[ClientSock].Dispose();
            CommunicationDictionary.Remove(ClientSock);
            ClientSock.Dispose();
            Clients.Remove(ClientSock);
            ClientThreadList.Remove(Thread.CurrentThread);

            Event.MessageOutputCall(
                new MessageOutputEventArgs
                {
                    Message = "IP: " +
                    (ClientSock.RemoteEndPoint
                    as IPEndPoint)
                    .Address.ToString() +
                    " Close done"
                });

            Log.GeneralEvent.Write("IP: " + (
                ClientSock.RemoteEndPoint as IPEndPoint)
                .Address.ToString() + " Close done");
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                MasterSwitch = false;
                Listener.Dispose();
                ConnectionEvent.Dispose();
                foreach (var q in Clients)
                    q.Dispose();
                foreach (var q in CommunicationDictionary)
                    q.Value.Dispose();

                foreach (var q in ClientThreadList)
                    q.Join();

                if (disposing)
                {
                    ClientsLock = null;
                    CommunicationDictionary = null;
                    ClientThreadList = null;
                    Clients = null;
                    // TODO: 處置 Managed 狀態 (Managed 物件)。
                }

                Event.MessageOutputCall(
                    new MessageOutputEventArgs
                    {
                        Message = "Network server dispose done"
                    });

                Log.GeneralEvent.Write("Network server dispose done");

                disposedValue = true;
            }
        }

        ~NetworkServer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    public class NetworkClient : IDisposable
    {
        Socket Server;
        Thread ServerThread;
        CommunicationBase CB;
        Queue<(decimal, string[])> DataBuffer;
        object DataBufferLock = new object();
        bool NetworkClientSwitch = true;

        public NetworkClient(string ipAddress, int Port)
        {
            Server = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            DataBuffer = new Queue<(decimal, string[])>();
            Server.Connect(new IPEndPoint(
                IPAddress.Parse(ipAddress), Port));
            CB = new CommunicationBase(Server);
            ServerThread = new Thread(Work) { IsBackground = true };
            ServerThread.Start();
        }

        private void Work()
        {
            int DataCount = int.MinValue;
            (decimal, string[]) RecordAndFiles;
            while (NetworkClientSwitch)
            {
                lock (DataBufferLock)
                    DataCount = DataBuffer.Count();
                if (DataCount is 0 && NetworkClientSwitch)
                    SpinWait.SpinUntil(() =>
                    DataBuffer.Count > 0 && NetworkClientSwitch);
                if (NetworkClientSwitch)
                {
                    try
                    {
                        bool IsSend = false;
                        lock (DataBufferLock)
                            RecordAndFiles = DataBuffer.Dequeue();
                        foreach (string FilePath in RecordAndFiles.Item2)
                        {
                            IsSend = false;
                            while (IsSend)
                            {
                                var FileAndInfo = GetFileAndInfo(FilePath);
                                CB.Send(Tools.EncodingString(
                                    Tools.EncodingRequest(
                                        ReceiveDataType.File,
                                        FileAndInfo.Item2,
                                        FileAndInfo.Item3)));
                                if (Tools.DecodingString(CB.Receive()) ==
                                    "Start")
                                {
                                    CB.Send(FileAndInfo.Item1);
                                    if (Tools.DecodingString(CB.Receive())
                                        == "Success")
                                    {
                                        IsSend = true;
                                        Log.GeneralEvent.Write(
                                                "Send file done");
                                    }
                                    else
                                        Log.GeneralEvent.Write(
                                                "Send file fail");
                                }
                            }
                        }
                        IsSend = false;
                        while (IsSend)
                        {
                            CB.Send(Tools.EncodingString(
                                Tools.EncodingRequest(
                                ReceiveDataType.Record,
                                string.Empty,
                                string.Empty)));
                            if (Tools.DecodingString(CB.Receive()) is "Start")
                            {
                                if (SendRecord(RecordAndFiles.Item1))
                                {
                                    IsSend = true;
                                    Log.GeneralEvent.Write(
                                        "Send record done");
                                }
                                else
                                    Log.GeneralEvent.Write(
                                        "Send record fail");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorEvent.Write(ex.ToString());
                    }
                }
            }
        }

        private bool SendRecord(dynamic Record)
        {
            bool IsSend = false;

            try
            {

                if (Tools.DecodingString(CB.Receive()) is "Success")
                    IsSend = true;
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.ToString());
            }
            return IsSend;
        }

        private (byte[], string, string) GetFileAndInfo(string FilePath)
        {
            try
            {
                byte[] FileData = File.ReadAllBytes(FilePath);
                string FileName = Path.GetFileName(FilePath);
                string MD5 = Tools.CalculateMD5Value(FileData);
                return (FileData, FileName, MD5);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void EnqueueData((decimal, string[]) Data)
        {
            lock (DataBufferLock)
                DataBuffer.Enqueue(Data);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                NetworkClientSwitch = false;
                ServerThread.Join();
                CB.Dispose();
                Server.Dispose();

                if (disposing)
                {
                    Server = null;
                    CB = null;
                    ServerThread = null;
                    DataBufferLock = null;
                }

                Log.GeneralEvent.Write(
                    "Network client dispose done");

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    public static class Tools
    {
        public static string CalculateMD5Value(this byte[] Data)
        {
            byte[] HashBytes;
            using (MD5 MD5Class = new MD5CryptoServiceProvider())
            {
                HashBytes = MD5Class.ComputeHash(Data);
            }
            StringBuilder SB = new StringBuilder();
            foreach (var q in HashBytes)
                SB.Append(q.ToString("x2"));
            return SB.ToString();
        }

        public static byte[] EncodingString(this string Data)
        {
            try
            {
                return Encoding.Default.GetBytes(Data);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static string DecodingString(this byte[] Data)
        {
            try
            {
                return Encoding.Default.GetString(Data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string EncodingRequest(ReceiveDataType DataType,
            string _FileName, string _MD5Hash)
        {
            if (DataType == ReceiveDataType.File)
            {
                string Json = JsonConvert.SerializeObject(
                    new
                    {
                        DataType = (int)ReceiveDataType.File,
                        FileName = _FileName,
                        MD5Hash = _MD5Hash
                    });
                return Json;
            }
            if (DataType == ReceiveDataType.Record)
            {
                string Json = JsonConvert.SerializeObject(
                    new
                    {
                        DataType = (int)ReceiveDataType.File
                    });
                return Json;
            }
            return string.Empty;
        }

        public static (ReceiveDataType, string, string) DecodingRequest(
            string Json)
        {
            Json = Json.Trim();
            if ((Json.StartsWith("{") && Json.EndsWith("}")) ||
                    (Json.StartsWith("[") && Json.EndsWith("]")))
            {
                dynamic Request = JsonConvert.DeserializeObject(Json);
                if (Request["DataType"] == "File")
                    return (ReceiveDataType.File,
                        Request["FileName"],
                        Request["MD5Hash"]);
                if (Request["DataType"] == "Record")
                    return (ReceiveDataType.Record,
                        string.Empty,
                        string.Empty);
            }

            return (ReceiveDataType.Error,
                string.Empty,
                string.Empty);
        }
    }
}
