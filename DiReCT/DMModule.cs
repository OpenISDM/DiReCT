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
 *      DMModule.cs
 * 
 * Abstract:
 *      
 *      Data Manager (DM) provides functions for other modules 
 *      to access the event data, user data and record data 
 *      in the local storage.
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
using DiReCT;
using System.Collections;

namespace DiReCT
{
    class DMModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;

        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;

        static WorkerThreadPool<WorkItem> moduleThreadPool;
        static WorkItem workItem;
        static PriorityWorkQueue<WorkItem> priorityworkQueue;
        //static IDictionary dictionary;

        public static void DMInit(object objectParameters)
        {
            moduleControlDataBlock
                = (ModuleControlDataBlock)objectParameters;
            threadParameters = moduleControlDataBlock.ThreadParameters;
            //moduleWorkQueue = moduleControlDataBlock.ModuleWorkQueue;

            try
            {
                //Initialize Ready/Abort Event      
                ModuleReadyEvent = threadParameters.ModuleReadyEvent;
                ModuleAbortEvent = threadParameters.ModuleAbortEvent;
                ModuleReadyEvent.Set();

                Debug.WriteLine("DMInit complete Phase 1 Initialization");

                //Wait for core StartWork Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("DMInit complete Phase 2 Initialization");

                //
                // Main Thread of DM module (begin)
                //               

                priorityworkQueue = new PriorityWorkQueue<WorkItem>(5);
                Debug.WriteLine("DM module is working...");

                // Check ModuleAbortEvent periodically
                while (!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime))
                {
                    // Wait for work event
                    priorityworkQueue.WakesWorkerEvent.WaitOne();
                    // Wrap work into workitem
                    
                    // Enqueue the workitem to its threadpool
                    
                }

                Debug.WriteLine("DM module is aborting.");
                CleanupExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("DM module thread failed.");
                threadParameters.ModuleInitFailedEvent.Set();
                Debug.WriteLine("DM ModuleInitFailedEvent Set");
                CleanupExit();
            }
        }

        private static void CleanupExit()
        {
            //
            // Cleanup code
            //
            priorityworkQueue.WakesWorkerEvent.Close();
            Debug.WriteLine("DM module stopped successfully.");
            return;
        }
    }
}


