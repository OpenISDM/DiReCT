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
 *      RTQCModule.cs
 * 
 * Abstract:
 *      
 *      Real-time Quality Control module is a DiReCT component which examines
 *      the observational record meta data and input data during the data
 *      collection. When it detects a defective record, it alerts the Monitor
 *      and Notification module, which is responsible for alerting the user and
 *      handles the defective record in specified ways.
 *
 * Authors:
 * 
 *      Hunter Hsieh, hunter205@iis.sinica.edu.tw  
 *      Jeff Chen, jeff@iis.sinica.edu.tw
 * 
 */

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

using DiReCT.Model.Utilities;
using DiReCT.Model.Observations;
using DiReCT.Model;
using DiReCT.MAN;

namespace DiReCT
{
    class RTQCModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;

        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;

        static DiReCTThreadPool moduleThreadPool;

        const int MAX_NUMBER_OF_THREADS = 10;

        public static void RTQCInit(object objectParameters)
        {
            moduleControlDataBlock
                = (ModuleControlDataBlock)objectParameters;
            threadParameters = moduleControlDataBlock.ThreadParameters;
            //moduleWorkQueue = moduleControlDataBlock.ModuleWorkQueue;

            try
            {
                //Initialize ready/abort event           
                ModuleReadyEvent = threadParameters.ModuleReadyEvent;
                ModuleAbortEvent = threadParameters.ModuleAbortEvent;
                moduleThreadPool = new DiReCTThreadPool(MAX_NUMBER_OF_THREADS);
                ModuleReadyEvent.Set();

                Debug.WriteLine("RTQCInit complete Phase 1 Initialization");

                //Wait for Core StartWorkEvent Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("RTQCInit complete Phase 2 Initialization");

                //
                // Main Thread of RTQC module (begin)
                //
                Debug.WriteLine("RTQC Core: " + Thread.CurrentThread.ManagedThreadId);
                Debug.WriteLine("RTQC module is working...");

                // Check ModuleAbortEvent periodically
                while (!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime))
                {

                }

                Debug.WriteLine("RTQC module is aborting.");
                CleanupExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("RTQC module thread failed.");
                threadParameters.ModuleInitFailedEvent.Set();
                Debug.WriteLine("RTQC ModuleInitFailedEvent Set");
                CleanupExit();
            }
        }

        private static void CleanupExit()
        {
            //
            // Cleanup code
            //
            ModuleStartWorkEvent.Close(); 
            Debug.WriteLine("RTQC module stopped successfully.");
            return;
        }

        /// <summary>
        /// RTQC API to wrap workItem 
        /// </summary>
        /// <param name="asyncCallName"></param>
        /// <param name="callBackFunction"></param>
        /// <param name="inputParameter"></param>
        /// <param name="state"></param>
        public static void RTQCWrapWorkItem(AsyncCallName asyncCallName,
                                          AsyncCallback callBackFunction,
                                          Object inputParameter,
                                          Object state)
        {

            WorkItem workItem = new WorkItem(
                FunctionGroupName.QualityControlFunction,
                asyncCallName,
                inputParameter,
                callBackFunction,
                state);

            moduleThreadPool.AddThreadWork(workItem);
        }

        /// <summary>
        /// determines which methods to process. The function is aimed to be 
        /// used by RTQC Worker threads.
        /// </summary>
        /// <param name="workItem"></param>
        internal static void RTQCWorkerFunctionProcessor(WorkItem workItem)
        {
            switch (workItem.AsyncCallName)
            {
                case AsyncCallName.Validate:

                    Validate(workItem);
                    break;

            }
        }


        /// <summary>
        /// Demo function to determine whether Flood Waterlevel is position or 
        /// negative
        /// </summary>
        /// <param name="workItem"></param>
        private static void Validate(WorkItem workItem)
        {
            //Get the record from input parameters
            dynamic flood = workItem.InputParameters;

            //whether waterlevel is negative or positive
            if(flood.WaterLevel < 0)
            {
                //Notfiy user that the input might be wrong
                //Call Notificaiton
                Notification.Builder mBuilder = new Notification.Builder();
                mBuilder.SetWhen(DateTime.Now);
                mBuilder.SetContentText("This record might be wrong. Please check again!");
                mBuilder.SetNotificationType(NotificationTypes.Toast);
                mBuilder.Build(10, null);
                //
                /* Push a notification */
                NotificationManager.Notify(10);

                workItem.OutputParameters = false;
            }
            else
            {
                workItem.OutputParameters = true;
            }

            //Signal workItem is finished
            workItem.Complete();
        }
    }
}


