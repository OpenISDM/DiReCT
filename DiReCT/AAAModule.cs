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
 *      AAAModule.cs
 * 
 * Abstract:
 *      
 *      Authentication and Authorization (AaA) module authenticates 
 *      the user during login. It checks the user for authorization 
 *      to download event data and upload record data to and from DiReCT.
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
    class AAAModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;
        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;
        static WorkerThreadPool<WorkItem> moduleThreadPool;

        public static void AAAInit(object objectParameters)
        {
            moduleControlDataBlock
                = (ModuleControlDataBlock)objectParameters;
            threadParameters = moduleControlDataBlock.ThreadParameters;
            //moduleWorkQueue = moduleControlDataBlock.ModuleWorkQueue;

            try
            {
                //Initialze ready/abort event                           
                ModuleReadyEvent = threadParameters.ModuleReadyEvent;
                ModuleAbortEvent = threadParameters.ModuleAbortEvent;
                ModuleReadyEvent.Set();

                Debug.WriteLine("AAAInit complete Phase 1 Initialization");

                //Wait for StartWork Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("AAAInit complete Phase 2 Initialization");

                //
                // Main Thread of AAA module (begin)
                //

                Debug.WriteLine("AAA module is working...");

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

                Debug.WriteLine("AAA module is aborting.");
                CleanupExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("AAA module thread failed.");
                threadParameters.ModuleInitFailedEvent.Set();
                Debug.WriteLine("AAA ModuleInitFailedEvent Set");
                CleanupExit();
            }
        }

        private static void CleanupExit()
        {
            //
            // Cleanup code
            //
            Debug.WriteLine("AAA module stopped successfully.");
            return;
        }
    }
}


