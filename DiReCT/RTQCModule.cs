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
using System.Threading;
using System.Diagnostics;
using DiReCT.Model.Utilities;
using DiReCT.Model;
using Amib.Threading;
using System.Collections.Generic;

namespace DiReCT
{
    class RTQCModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;
        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;
        static SmartThreadPool moduleThreadPool;
        
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
                moduleThreadPool = threadParameters.moduleThreadPool;
                
                // Event Handlers Initialization               
                ValidateEventTriggerd += new ValidateEventHanlder(
                                                    RTQCValidateWrapper);

                ModuleReadyEvent.Set();

                Debug.WriteLine("RTQCInit complete Phase 1 Initialization");

                // Wait for Core StartWorkEvent Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("RTQCInit complete Phase 2 Initialization");
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
        private static object Validate(dynamic record)
        {
            // Get the record from input parameters
            //dynamic flood = workItem.InputParameters;

            //// Check whether waterlevel is negative or positive
            //if(flood.waterLevel < 0)
            //{
            //    // Notfiy user that the input might be wrong
            //    Notification.Builder mBuilder = new Notification.Builder();
            //    mBuilder.SetWhen(DateTime.Now);
            //    mBuilder.SetContentText("This record might be wrong." + 
            //                            " Please check again!");
            //    mBuilder.SetNotificationType(NotificationTypes.Toast);
            //    mBuilder.Build(10, null);

            //    /* Push a notification */
            //    NotificationManager.Notify(10);

            //    workItem.OutputParameters = false;
            //}
            //else
            //{
            //    workItem.OutputParameters = true;
            //}
            // workItem.OutputParameters = true;
            // Signal that workItem is finished
            // workItem.Complete();
            KeyValuePair<dynamic,bool> pair = 
                new KeyValuePair<dynamic,bool>(record,true);
            
            return pair;
        }

        // Delegate that specify the parameter of event handler
        public delegate void ValidateEventHanlder(dynamic record);
        // Event Handler for Validate
        public static event ValidateEventHanlder ValidateEventTriggerd;

        /// <summary>
        /// Function to initiate the Validate event
        /// </summary>
        /// <param name="obj"></param>
        public static void OnValidate(dynamic record)
        {
            ValidateEventTriggerd?.BeginInvoke(record, null, null);
        }

        /// <summary>
        /// This function is subscribed to ValidateEventTriggered event and 
        /// will be called when the event is raised
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="callBackFunction"></param>
        public static void RTQCValidateWrapper(dynamic record)
        {
            moduleThreadPool.QueueWorkItem(
                new WorkItemCallback(Validate), record,
                new PostExecuteWorkItemCallback(DMModule.SaveRecordtoDictionary));
        }
    }
}


