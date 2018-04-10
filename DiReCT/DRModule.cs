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
 *      DiReCT(Disaster Record Capture Tool)
 * 
 * File Description:
 * File Name:
 * 
 *      DRModule.cs
 * 
 * Abstract:
 *      
 *     Responsible for stable and reliable data transmission with the 
 *     DiReCT server for all downloads and uploads of the record journey.
 *
 * Authors:
 * 
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 * 
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DiReCT.Network;
using Newtonsoft.Json;
using DiReCT.Logger;

namespace DiReCT
{
    public class DRModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;
        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;

        static Thread ReceiveWorkThread;
        static Socket ServerSocket;
        static CommunicationBase ServerCommunication;

        static string ServerIPAddress;
        static int ServerPort;

        static ReceiveEvent receiveEvent;

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

                ServerSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream,ProtocolType.Tcp);
                receiveEvent = new ReceiveEvent();
                ModuleReadyEvent.Set();
                ReceiveWorkThread = new Thread(ReceiveWork);

                Log.GeneralEvent
                    .Write("DRInit complete Phase 1 Initialization");

                // Wait for core StartWork Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Log.GeneralEvent
                    .Write("DRInit complete Phase 2 Initialization");

                ReceiveWorkThread.Start();
                Log.GeneralEvent.Write("DR module is working...");

                // Check ModuleAbortEvent periodically
                SpinWait.SpinUntil(() => !ModuleAbortEvent
                .WaitOne((int)TimeInterval.VeryVeryShortTime));

                Log.GeneralEvent.Write("DR module is aborting.");
            }
            catch(Exception ex)
            {
                Log.ErrorEvent.Write(ex.Message);
                Log.ErrorEvent.Write("DR module thread failed.");
                threadParameters.ModuleInitFailedEvent.Set();
                Log.ErrorEvent.Write("DR ModuleInitFailedEvent Set");
            }

            CleanupExit();
        }

        /// <summary>
        /// Send control signal to server
        /// </summary>
        /// <param name="ControlSignal"></param>
        public static void Send(string ControlSignal)
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

                // Send control signals to server
                ServerCommunication.Send(Encoding.UTF8.GetBytes(JsonString));
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.Message);

                throw ex;
            }
        }

        /// <summary>
        /// Send data (file, record...) to server 
        /// </summary>
        /// <param name="Data"></param>
        public static void Send(byte[] Data)
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

                // Send data to server
                ServerCommunication.Send(Encoding.UTF8.GetBytes(JsonString));
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.Message);

                throw ex;
            }
        }

        /// <summary>
        /// Receive server message
        /// </summary>
        private static void ReceiveWork()
        {
            while (!ModuleAbortEvent
                .WaitOne((int)TimeInterval.VeryVeryShortTime))
            {
                try
                {
                    // Connect to server
                    ServerSocket.Connect(new IPEndPoint(
                        IPAddress.Parse(ServerIPAddress), ServerPort));

                    if (ServerSocket.Connected)
                    {
                        ServerCommunication = 
                            new CommunicationBase(ServerSocket);

                        // Receive server message
                        while (!ModuleAbortEvent
                            .WaitOne((int)TimeInterval.VeryVeryShortTime) ||
                            ServerSocket.Connected)
                        {
                            Receive();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorEvent.Write(ex.Message);
                    Thread.Sleep(60000);
                }
            }
        }

        /// <summary>
        /// Receive server message and parsing content
        /// </summary>
        private static void Receive()
        {
            try
            {
                // Receive server message
                byte[] Data = ServerCommunication.Receive();

                // Convert data to json then parsing content
                string JsonString = Encoding.UTF8.GetString(Data);
                dynamic Json = JsonConvert.DeserializeObject(JsonString);

                if (Json["Type"] == "ControlSignal")
                {
                    ReceiveEventArgs
                        .ControlSignalEventArgs ControlSignalReceive
                         = new ReceiveEventArgs.ControlSignalEventArgs
                         {
                             ControlSignal = Json["Data"]
                         };

                    // Send events to the DS module
                    receiveEvent.ControlSignalEventCall(ControlSignalReceive);
                }

                if (Json["Type"] == "DataFlow")
                {
                    byte[] DataFlow = Encoding.UTF8.GetBytes(Json["Data"]);

                    ReceiveEventArgs.DataFlowEventArgs DataFlowReceive
                         = new ReceiveEventArgs.DataFlowEventArgs
                         {
                             Data = DataFlow
                         };

                    // Send events to the DS module
                    receiveEvent.DataFlowEventCall(DataFlowReceive);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.Message);

                throw ex;
            }
        }

        /// <summary>
        /// Give the DS module to receive events sent by DR
        /// </summary>
        public static ReceiveEvent ReceiveServerEvent
        {
            get
            {
                return receiveEvent;
            }
        }

        /// <summary>
        /// Connection status
        /// </summary>
        public bool Connected
        {
            get
            {
                return ServerSocket.Connected;
            }
        }

        private static void CleanupExit()
        {
            Log.GeneralEvent.Write("DR module stopped successfully.");
        }
    }
}
