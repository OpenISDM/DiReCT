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
 *      MANModule.cs
 * 
 * Abstract:
 *      
 *      Monitor and Notification (MaN) is a run-time monitoring tool. 
 *      It provides the capabilities for real-time monitoring, 
 *      capture and analysis of events and conditions that indicate the 
 *      potential for occurrences, or actual occurrences, of errors, 
 *      and issuing alerts and notifications to trigger error handling 
 *      or prevention actions. It also provides notification functions 
 *      other modules can call to send reminders and alerts.
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

namespace DiReCT
{
    class MANModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;

        static ManualResetEvent ModuleAbortEvent;
        static AutoResetEvent ModuleReadyEvent, ModuleStartWorkEvent;

        static WorkerThreadPool<WorkItem> moduleThreadPool;
        static WorkItem workItem;

        public static void MANInit(object objectParameters)
        {
            moduleControlDataBlock
                = (ModuleControlDataBlock)objectParameters;
            threadParameters = moduleControlDataBlock.ThreadParameters;
            //moduleWorkQueue = moduleControlDataBlock.ModuleWorkQueue;

            try
            {
                //
                // Modules initialization code here...
                //            

                ModuleReadyEvent.Set();
                Debug.WriteLine("MANInit complete Phase 1 Initialization");

                //
                // Phase 2 initialization code
                //

                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("MANInit complete Phase 2 Initialization");

                //
                // Main Thread of MAN module (begin)
                //

                Debug.WriteLine("MAN module is working...");

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

                Debug.WriteLine("MAN module is aborting.");
                CleanupExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("MAN module thread failed.");
                threadParameters.ModuleInitFailedEvent.Set();
                Debug.WriteLine("MAN ModuleInitFailedEvent Set");
                CleanupExit();
            }
        }

        private static void CleanupExit()
        {
            //
            // Cleanup code
            //
            Debug.WriteLine("MAN module stopped successfully.");
            return;
        }
    }
}


