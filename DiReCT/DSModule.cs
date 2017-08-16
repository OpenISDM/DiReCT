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
using System.Threading;
using System.Diagnostics;
using DiReCT.Model;
using Amib.Threading;

namespace DiReCT
{
    class DSModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;
        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;
        static SmartThreadPool moduleThreadPool;
               
        public static void DSInit(object objectParameters)
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
                moduleThreadPool = threadParameters.moduleThreadPool;

                ModuleReadyEvent.Set();

                Debug.WriteLine("DSInit complete Phase 1 Initialization");

                //Wait for startwork signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("DSInit complete Phase 2 Initialization");

                //
                // Main Thread of DS module (begin)
                //
                Debug.WriteLine("DS module is working...");
                Debug.WriteLine("DS Core: " + Thread.CurrentThread.ManagedThreadId);
                // Check ModuleAbortEvent periodically
                while (!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime))
                {
                    //Does nothing
                   
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


