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
using System.Threading;
using System.Diagnostics;
using DiReCT.Model;
using Amib.Threading;

namespace DiReCT
{
    class AAAModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;
        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;
        static SmartThreadPool moduleThreadPool;

        public static void AAAInit(object objectParameters)
        {
            moduleControlDataBlock
                = (ModuleControlDataBlock)objectParameters;
            threadParameters = moduleControlDataBlock.ThreadParameters;
            
            try
            {
                //Initialze ready/abort event                           
                ModuleReadyEvent = threadParameters.ModuleReadyEvent;
                ModuleAbortEvent = threadParameters.ModuleAbortEvent;
                moduleThreadPool = threadParameters.moduleThreadPool;
                ModuleReadyEvent.Set();

                Debug.WriteLine("AAAInit complete Phase 1 Initialization");

                //Wait for StartWork Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("AAAInit complete Phase 2 Initialization");
                Debug.WriteLine("AAA module is working...");

                // Check ModuleAbortEvent periodically
                while (!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime))
                {

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


