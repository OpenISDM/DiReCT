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

namespace DiReCT
{
    class DMModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;

        static ManualResetEvent ModuleAbortEvent;
        static AutoResetEvent ModuleReadyEvent, ModuleStartWorkEvent;

        static WorkerThreadPool<WorkItem> moduleThreadPool;
        static WorkItem workItem;

        public static void DMInit(object objectParameters)
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
                Debug.WriteLine("DMInit complete Phase 1 Initialization");

                //
                // Phase 2 initialization code
                //

                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("DMInit complete Phase 2 Initialization");

                //
                // Main Thread of DM module (begin)
                //

                Debug.WriteLine("DM module is working...");

                // Check ModuleAbortEvent periodically
                while (!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime))
                {

                    //
                    // Wait for work event
                    // A switch case for each work event, 
                    //   e.g. DMSaveRecordWorkEvent.
                    //
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
            Debug.WriteLine("DM module stopped successfully.");
            return;
        }
    }
}


