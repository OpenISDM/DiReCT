/*
 * Copyright (c) 2016 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and 
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 * 
 *      DiReCT_Server(Disaster Record Capture Tool Server)
 * 
 * File Description:
 * File Name:
 * 
 *      DRModule.cs
 * 
 * Abstract:
 *      
 *     Responsible for stable and reliable data transmission with the 
 *     DiReCT client for all downloads and uploads of the record journey.
 *
 * Authors:
 * 
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 * 
 */

using DiReCT.Logger;
using DiReCT.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent,
            SocketCloseSignal;
        static AutoResetEvent ModuleReadyEvent;

        // Listen client connection request
        static Thread ListenThread;
        static Socket Listener;
        static ManualResetEvent ConnectionEvent;
        static int ListenPort;

        // Client connection configuration
        static Dictionary<Socket, CommunicationBase> CommunicationDictionary;
        static List<Socket> Clients;
        static List<Thread> ClientThreadList;

        static bool MasterSwitch = true;
        static object ClientsLock = new object();

        /// <summary>
        /// DR module initialization
        /// </summary>
        /// <param name="objectParameters"></param>
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

                // Set listener socket configuration
                Listener = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                Listener.Bind(new IPEndPoint(IPAddress.Any, ListenPort));

                ListenThread = new Thread(ListenWork);

                ModuleReadyEvent.Set();

                Log.GeneralEvent
                    .Write("DRInit complete Phase 1 Initialization");

                // Wait for core StartWork Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Log.GeneralEvent
                    .Write("DRInit complete Phase 2 Initialization");

                ListenThread.Start();
                Log.GeneralEvent.Write("DR module is working...");

                // Check ModuleAbortEvent periodically
                SpinWait.SpinUntil(() => !ModuleAbortEvent
                .WaitOne((int)TimeInterval.VeryVeryShortTime));

                Log.GeneralEvent.Write("DR module is aborting.");
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.Message);
                Log.ErrorEvent.Write("DR module thread failed.");
                threadParameters.ModuleInitFailedEvent.Set();
                Log.ErrorEvent.Write("DR ModuleInitFailedEvent Set");
            }

            CleanupExit();
        }

        /// <summary>
        /// Send control signal to DiReCT
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="ControlSignal"></param>
        public static void Send(Socket ClientSocket, string ControlSignal)
        {
            try
            {
                // Package control signals into json format
                string JsonString = JsonConvert.SerializeObject(
                    new
                    {
                        Type = "ControlSignal",
                        Data = ControlSignal
                    });

                // Send control signals to DiReCT
                CommunicationDictionary[ClientSocket]
                    .Send(Encoding.UTF8.GetBytes(JsonString));
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.Message);
            }
        }

        /// <summary>
        /// Send data (file, record...) to DiReCT
        /// </summary>
        /// <param name="ClientSocket"></param>
        /// <param name="Data"></param>
        public static void Send(Socket ClientSocket, byte[] Data)
        {
            try
            {
                // Convert byte array to string
                string DataFlowString = Encoding.UTF8.GetString(Data);

                // Package data string into json format
                string JsonString = JsonConvert.SerializeObject(
                    new
                    {
                        Type = "DataFlow",
                        Data = DataFlowString
                    });

                // Send data to DiReCT
                CommunicationDictionary[ClientSocket]
                    .Send(Encoding.UTF8.GetBytes(JsonString));
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.Message);
            }
        }

        /// <summary>
        /// Listen client connection request
        /// </summary>
        private static void ListenWork()
        {
            Listener.Listen(100);
            while (!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime))
            {
                try
                {
                    ConnectionEvent.Reset();
                    Listener.BeginAccept(new AsyncCallback(AcceptCallback),
                        Listener);
                    ConnectionEvent.WaitOne();
                }
                catch(Exception ex)
                {
                    Log.ErrorEvent.Write(ex.Message);
                }
            }
        }

        /// <summary>
        /// Callback when one client successfully connected to the server.
        /// </summary>
        /// <param name="ar"></param>
        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;

            // Create and record connection configuration
            lock (ClientsLock)
            {
                Socket Client = listener.EndAccept(ar);
                Clients.Add(Client);
                CommunicationDictionary.Add(Client,
                    new CommunicationBase(Client));

                Thread ClientThread = new Thread(ReceiveClientRequest)
                {
                    IsBackground = true
                };

                ClientThread.Start(Client);
                ClientThreadList.Add(ClientThread);
            }

            ConnectionEvent.Set();
        }

        /// <summary>
        /// Receive client message
        /// </summary>
        /// <param name="Client"></param>
        private static void ReceiveClientRequest(object Client)
        {
            // Get the client connection configuration
            Socket ClientSock = (Client as Socket);
            CommunicationBase ClientCommunication
                = CommunicationDictionary[ClientSock];
            string IPAddress = (ClientSock.RemoteEndPoint as IPEndPoint)
                .Address.ToString();

            // Receive DiReCT message
            while (MasterSwitch && ClientSock.Connected)
            {
                try
                {
                    // Receive DiReCT message
                    byte[] Data = ClientCommunication.Receive();

                    // Convert data to json then parsing content
                    string JsonString = Encoding.UTF8.GetString(Data);
                    dynamic Json = JsonConvert.DeserializeObject(JsonString);

                    if (Json["Type"] is "ControlSignal")
                    {
                        ReceiveEventArgs
                            .ControlSignalEventArgs ControlSignalReceive
                             = new ReceiveEventArgs.ControlSignalEventArgs
                             {
                                 ControlSignal = Json["Data"],
                                 Socket = ClientSock
                             };

                        Task.Run(() =>
                        {
                            // do something
                        });
                    }

                    if (Json["Type"] is "DataFlow")
                    {
                        byte[] DataFlow =Encoding.UTF8.GetBytes(Json["Data"]);

                        ReceiveEventArgs.DataFlowEventArgs DataFlowReceive
                             = new ReceiveEventArgs.DataFlowEventArgs
                             {
                                 Data = DataFlow,
                                 Socket = ClientSock
                             };

                        Task.Run(() =>
                        {
                            // do something
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorEvent.Write(ex.Message);
                }
            }

            // Disconnected or server service is ending
            // Release connection resources
            if (ModuleAbortEvent.WaitOne())
                SocketCloseSignal.WaitOne();
            CommunicationDictionary[ClientSock].Dispose();
            CommunicationDictionary.Remove(ClientSock);
            ClientSock.Dispose();
            Clients.Remove(ClientSock);
            ClientThreadList.Remove(Thread.CurrentThread);

            Log.GeneralEvent.Write("IP: " + IPAddress + " Close done");
        }

        private static void CleanupExit()
        {
            Listener.Dispose();

            foreach (var ClientCommunication in CommunicationDictionary)
            {
                try
                {
                    ClientCommunication.Key.Shutdown(SocketShutdown.Both);
                    ClientCommunication.Key.Close();
                }
                catch(Exception ex)
                {
                    Log.ErrorEvent.Write(ex.Message);
                }
            }

            SocketCloseSignal.Set();
            ConnectionEvent.Dispose();

            Debug.WriteLine("DR module stopped successfully.");
        }
    }
}
