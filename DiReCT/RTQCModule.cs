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
            try
            {
                // Initialize ready/abort event and threadpool        
                ModuleReadyEvent = threadParameters.ModuleReadyEvent;
                ModuleAbortEvent = threadParameters.ModuleAbortEvent;
                moduleThreadPool = new DiReCTThreadPool(MAX_NUMBER_OF_THREADS);
                ModuleReadyEvent.Set();

                Debug.WriteLine("RTQCInit complete Phase 1 Initialization");

                // Wait for Core StartWorkEvent Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("RTQCInit complete Phase 2 Initialization");

                //
                // Main Thread of RTQC module (begin)
                //
                Debug.WriteLine("RTQC module is working...");

                // Whenever ValidateEvent is raised, RTQCValidateWrapper will
                // be called.
                ValidateEventTriggerd += new ValidateEventHanlder(
                                                    RTQCValidateWrapper);

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
            // Get the record from input parameters
            dynamic flood = workItem.InputParameters;

            // Check whether waterlevel is negative or positive
            if(flood.WaterLevel < 0)
            {
                // Notfiy user that the input might be wrong
                Notification.Builder mBuilder = new Notification.Builder();
                mBuilder.SetWhen(DateTime.Now);
                mBuilder.SetContentText("This record might be wrong." + 
                                        " Please check again!");
                mBuilder.SetNotificationType(NotificationTypes.Toast);
                mBuilder.Build(10, null);
                
                /* Push a notification */
                NotificationManager.Notify(10);
                
                workItem.OutputParameters = false;
            }
            else
            {
                workItem.OutputParameters = true;
            }

            // Signal that workItem is finished
            workItem.Complete();
        }

        // Delegate that specify the parameter of event handler
        public delegate void ValidateEventHanlder(object obj, 
                                               AsyncCallback callBackFunction);
        // Event Handler for Validate
        public static event ValidateEventHanlder ValidateEventTriggerd;

        /// <summary>
        /// Function to initiate the Validate event
        /// </summary>
        /// <param name="obj"></param>
        public static void OnValidate(object obj, 
                                      AsyncCallback callBackFunction)
        {
            ValidateEventTriggerd?.BeginInvoke(obj, callBackFunction, null, null);
        }

        /// <summary>
        /// This function is subscribed to ValidateEventTriggered event and 
        /// will be called when the event is raised
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="callBackFunction"></param>
        public static void RTQCValidateWrapper(object obj,
                                               AsyncCallback callBackFunction)
        {
            WorkItem workItem = new WorkItem(
                FunctionGroupName.QualityControlFunction,
                AsyncCallName.Validate,
                obj,
                callBackFunction,
                null);
     
            moduleThreadPool.AddThreadWork(workItem);
        }
    }
}


