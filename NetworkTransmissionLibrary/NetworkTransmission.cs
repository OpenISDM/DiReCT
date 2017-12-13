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

namespace NetworkTransmissionLibrary
{
    public enum ReceiveDataType
    {
        Record,
        File,
        Error
    }

    public class ControlSignalTransmission
    {

        public class NetworkServer :IDisposable
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
            private bool MasterSwitch = true;
            private object ClientsLock = new object();
            private string FilePath;

            public NetworkServer(int Port)
            {
                Listener = new Socket(AddressFamily.InterNetwork, 
                    SocketType.Stream, ProtocolType.Tcp);
                Listener.Bind(new IPEndPoint(IPAddress.Any, Port));
                ListenServiceThread = new Thread(ListenWork) {
                    IsBackground = true };
                ListenServiceThread.Start();
                FilePath = AppDomain.CurrentDomain.BaseDirectory + "\\Media\\";
            }

            public NetworkServer(int Port,string filePath)
            {
                Listener = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                Listener.Bind(new IPEndPoint(IPAddress.Any, Port));
                ListenServiceThread = new Thread(ListenWork);
                ListenServiceThread.Start();
                FilePath = filePath;
            }

            private void ListenWork()
            {
                Listener.Listen(100);
                while(MasterSwitch)
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
                lock(ClientsLock)
                {
                    Socket Client = listener.EndAccept(ar);
                    Clients.Add(Client);
                    CommunicationDictionary.Add(Client, 
                        new CommunicationBase(Client));
                    Thread ClientThread = new Thread(ListenClientRequest) {
                        IsBackground = true };
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
                                        if (SaveFile(Buffer, Request.Item2))
                                            CB.Send(Encoding.Default.GetBytes(
                                                "Success"));
                                        else
                                            CB.Send(Encoding.Default.GetBytes(
                                                "Fail"));
                                    else
                                        CB.Send(Encoding.Default.GetBytes(
                                            "Fail"));
                                }

                                if (Request.Item1 == ReceiveDataType.Record)
                                {
                                    if (Request.Item1 == ReceiveDataType.Record)
                                    {
                                        CB.Send(Encoding.Default.GetBytes("Start"));
                                        Buffer = CB.Receive();
                                        Json = Buffer.DecodingString();
                                        if (Json != string.Empty)
                                            if (SaveRecordToDB(Json))
                                                CB.Send(Encoding.Default.GetBytes(
                                                    "Success"));
                                            else
                                                CB.Send(Encoding.Default.GetBytes(
                                                    "Fail"));
                                        else
                                            CB.Send(Encoding.Default.GetBytes(
                                                "Fail"));
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    
                }
                CommunicationDictionary[ClientSock].Dispose();
                CommunicationDictionary.Remove(ClientSock);
                ClientSock.Dispose();
                Clients.Remove(ClientSock);
                ClientThreadList.Remove(Thread.CurrentThread);
            }

            private bool SaveFile(byte[] Data,string FileName)
            {
                bool IsSave = false;
                try
                {
                    File.WriteAllBytes(FilePath + FileName, Data);
                    IsSave = true;
                }
                catch
                {

                }
                return IsSave;
            }

            private bool SaveRecordToDB(string Json)
            {
                bool IsSave = false;
                try
                {
                    IsSave = true;
                }
                catch
                {

                }
                return IsSave;
            }

            #region IDisposable Support
            private bool disposedValue = false; // 偵測多餘的呼叫

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    Listener.Dispose();
                    ConnectionEvent.Dispose();
                    MasterSwitch = false;
                    foreach (var q in Clients)
                        q.Dispose();
                    foreach (var q in CommunicationDictionary)
                        q.Value.Dispose();

                    foreach (var q in ClientThreadList)
                        q.Join();

                    if (disposing)
                    {
                        FilePath = string.Empty;
                        ClientsLock = null;
                        CommunicationDictionary = null;
                        ClientThreadList = null;
                        Clients = null;
                        // TODO: 處置 Managed 狀態 (Managed 物件)。
                    }

                    // TODO: 釋放 Unmanaged 資源 (Unmanaged 物件) 並覆寫下方的完成項。
                    // TODO: 將大型欄位設為 null。

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

        public class NetworkClient
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
                            foreach(string FilePath in RecordAndFiles.Item2)
                            {
                                IsSend = false;
                                while(IsSend)
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
                                            IsSend = true;
                                    }
                                }
                            }
                            IsSend = false;
                            while(IsSend)
                            {
                                CB.Send(Tools.EncodingString(
                                    Tools.EncodingRequest(
                                    ReceiveDataType.Record,
                                    string.Empty,
                                    string.Empty)));
                                if (Tools.DecodingString(CB.Receive()) ==
                                    "Start")
                                {
                                    if(SendRecord(RecordAndFiles.Item1))
                                        IsSend = true;
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

            private bool SendRecord(dynamic Record)
            {
                bool IsSend = false;

                try
                {
                    
                    if (Tools.DecodingString(CB.Receive())
                        == "Success")
                        IsSend = true;
                }
                catch
                {

                }
                return IsSend;
            }

            private (byte[],string,string) GetFileAndInfo(string FilePath)
            {
                try
                {
                    byte[] FileData = File.ReadAllBytes(FilePath);
                    string FileName = Path.GetFileName(FilePath);
                    string MD5 = Tools.CalculateMD5Value(FileData);
                    return (FileData, FileName, MD5);
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }

            public void EnqueueData((decimal, string[]) Data)
            {
                lock (DataBufferLock)
                    DataBuffer.Enqueue(Data);
            }
        }
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
            catch
            {
                return null;
            }
        }

        public static string DecodingString(this byte[] Data)
        {
            try
            {
                return Encoding.Default.GetString(Data);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string EncodingRequest(ReceiveDataType DataType,
            string FileName, string MD5Hash)
        {
            if (DataType == ReceiveDataType.File)
            {
                string Json = JsonConvert.SerializeObject(
                    new
                    {
                        DataType = (int)ReceiveDataType.File,
                        FileName = FileName,
                        MD5Hash = MD5Hash
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
