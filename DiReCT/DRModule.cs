using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiReCT.Network;
using DiReCT.Logger;
using Newtonsoft.Json;

namespace DiReCT
{
    public class DRModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;
        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;

        static Socket ServerSocket;
        static CommunicationBase ServerCommunication;
        static string ServerIPAddress;
        static int ServerPort;

        static ReceiveEvent receiveEvent;

        public static void DRInit(object objectParameters)
        {
            moduleControlDataBlock
                = (ModuleControlDataBlock)objectParameters;
            threadParameters = moduleControlDataBlock.ThreadParameters;

            try
            {
                // Initialize Ready/Abort Event and threadpool    
                ModuleReadyEvent = threadParameters.ModuleReadyEvent;
                ModuleAbortEvent = threadParameters.ModuleAbortEvent;

                ServerSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream,ProtocolType.Tcp);
                receiveEvent = new ReceiveEvent();
                ModuleReadyEvent.Set();

                Debug.WriteLine("DRInit complete Phase 1 Initialization");

                // Wait for core StartWork Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("DRInit complete Phase 2 Initialization");
                Debug.WriteLine("DR module is working...");

                // Check ModuleAbortEvent periodically
                while (!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime))
                {
                    ServerSocket.Connect(new IPEndPoint(
                        IPAddress.Parse(ServerIPAddress), ServerPort));
                    ServerCommunication = new CommunicationBase(ServerSocket);

                    while(!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime) ||
                        ServerSocket.Connected)
                    {
                        Receive();
                    }

                    Thread.Sleep(60000);
                }

                Debug.WriteLine("DR module is aborting.");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("DR module thread failed.");
                threadParameters.ModuleInitFailedEvent.Set();
                Debug.WriteLine("DR ModuleInitFailedEvent Set");
            }

            CleanupExit();
        }

        public static void Send(string ControlSignal)
        {
            try
            {
                string JsonString = JsonConvert.SerializeObject(
                    new
                    {
                        Type = "ControlSignal",
                        Data = ControlSignal
                    });

                ServerCommunication.Send(Encoding.UTF8.GetBytes(JsonString));
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.Message);
                ServerCommunication.ConnectDone.Set();
                throw ex;
            }
        }

        public static void Send(byte[] Data)
        {
            try
            {
                string DataFlowString = Encoding.UTF8.GetString(Data);

                string JsonString = JsonConvert.SerializeObject(
                    new
                    {
                        Type = "DataFlow",
                        Data = DataFlowString
                    });

                ServerCommunication.Send(Encoding.UTF8.GetBytes(JsonString));
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.Message);
                ServerCommunication.ConnectDone.Set();
                throw ex;
            }
        }

        private static void Receive()
        {
            try
            {
                byte[] Data = ServerCommunication.Receive();

                string JsonString = Encoding.UTF8.GetString(Data);
                dynamic Json = JsonConvert.DeserializeObject(JsonString);

                if (Json["Type"] is "ControlSignal")
                {
                   ReceiveEventArgs.ControlSignalEventArgs ControlSignalReceive
                        = new ReceiveEventArgs.ControlSignalEventArgs {
                            ControlSignal = Json["Data"]
                        };

                    receiveEvent.ControlSignalEventCall(ControlSignalReceive);
                }

                if (Json["Type"] is "DataFlow")
                {
                    byte[] DataFlow = Encoding.UTF8.GetBytes(Json["Data"]);

                    ReceiveEventArgs.DataFlowEventArgs DataFlowReceive
                         = new ReceiveEventArgs.DataFlowEventArgs { 
                             Data = DataFlow
                         };

                    receiveEvent.DataFlowEventCall(DataFlowReceive);
                }
            }
            catch(Exception ex)
            {
                Log.ErrorEvent.Write(ex.Message);
                ServerCommunication.ConnectDone.Set();
                throw ex;
            }
        }

        public ReceiveEvent ReceiveServerEvent { get { return receiveEvent; } }
        public bool Connected { get { return ServerSocket.Connected; } }

        private static void CleanupExit()
        {
            Debug.WriteLine("DM module stopped successfully.");
        }
    }
}
