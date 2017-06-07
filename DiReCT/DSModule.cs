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
 *      DSModule.cs
 * 
 * Abstract:
 *      
 *      Data Synchronizer (DS) downloads event data 
 *      (work descriptions, UI configuration, SOP configuration, map data, 
 *      user data, historical records, quality control data) and 
 *      user data (profile and settings) before data collection. 
 *      It also uploads record data during or after data collection.
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
    class DSModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;

        static ManualResetEvent ModuleAbortEvent;
        static AutoResetEvent ModuleReadyEvent, ModuleStartWorkEvent;

        static WorkerThreadPool<WorkItem> moduleThreadPool;
        static WorkItem workItem;

        public static void DSInit(object objectParameters)
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
                Debug.WriteLine("DSInit complete Phase 1 Initialization");

                //
                // Phase 2 initialization code
                //

                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("DSInit complete Phase 2 Initialization");

                //
                // Main Thread of DS module (begin)
                //

                Debug.WriteLine("DS module is working...");

                // Check ModuleAbortEvent periodically
                while (!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime))
                {

                    //
                    // Wait for work event
                    // A switch case for each work event.
                    //
                }

                Debug.WriteLine("DS module is aborting.");
                CleanupExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("DS module thread failed.");
                threadParameters.ModuleInitFailedEvent.Set();
                Debug.WriteLine("DS ModuleInitFailedEvent Set");
                CleanupExit();
            }
        }

        private static void CleanupExit()
        {
            //
            // Cleanup code
            //
            Debug.WriteLine("DS module stopped successfully.");
            return;
        }
    }
}


