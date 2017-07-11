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

namespace DiReCT
{
    class RTQCModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;

        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;

        static WorkerThreadPool<WorkItem> moduleThreadPool;
        static WorkItem workItem;

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
                ModuleReadyEvent.Set();

                Debug.WriteLine("RTQCInit complete Phase 1 Initialization");

                //Wait for Core StartWorkEvent Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("RTQCInit complete Phase 2 Initialization");

                //
                // Main Thread of RTQC module (begin)
                //

                Debug.WriteLine("RTQC module is working...");

                // Check ModuleAbortEvent periodically
                while (!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime))
                {

                    //
                    // Wait for work event
                    // Wrap work into workitem
                    // Enqueue the workitem to its threadpool
                    //
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

        internal static void RTQCWrapWorkItem(AsyncCallName validate, AsyncCallback asyncCallback, ObservationRecord record, object p)
        {
            throw new NotImplementedException();
        }
    }
}


