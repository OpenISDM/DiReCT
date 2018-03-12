using DiReCT.Logger;
using DiReCT.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiReCT.Server
{
    public class DRModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;
        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;

        static Thread ListenThread;
        static Socket Listener;
        static Dictionary<Socket, CommunicationBase> CommunicationDictionary;
        static List<Socket> Clients;
        static List<Thread> ClientThreadList;
        static ManualResetEvent ConnectionEvent;

        private bool MasterSwitch = true;
        private object ClientsLock = new object();

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

                Clients = new List<Socket>();
                ClientThreadList = new List<Thread>();
                ConnectionEvent = new ManualResetEvent(false);
                CommunicationDictionary 
                    = new Dictionary<Socket, CommunicationBase>();

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
                    
                }

                Debug.WriteLine("DR module is aborting.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("DR module thread failed.");
                threadParameters.ModuleInitFailedEvent.Set();
                Debug.WriteLine("DR ModuleInitFailedEvent Set");
            }

            CleanupExit();
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

            try
            {
                while (MasterSwitch)
                {
                    Json = CB.Receive().DecodingString();
                }
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.ToString());
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

        private static void CleanupExit()
        {
            Debug.WriteLine("DM module stopped successfully.");
        }
    }
}
