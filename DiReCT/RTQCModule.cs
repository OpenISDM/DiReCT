/*
 * Copyright (c) 2016 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : This file is subject to the terms and conditions defined
 *      in file 'COPYING.txt', which is part of this source code package.
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
 *      Real-time Quality Control module is a thread which examines the record
 *      meta data and input data during the data collection. It detects defects
 *      in real-time, alerting the user of the errors and overseeing the 
 *      corrections are made.
 *
 * Authors:
 * 
 *      Hunter Hsieh, hunter205@iis.sinica.edu.tw  
 *      Jeff Chen, jeff@iis.sinica.edu.tw
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace DiReCT
{
    class RTQCModule
    {
        // State control variable
        static bool IsInitialized = false;
        static bool IsReady = false;
        static bool IsContinue = true;

        public static void RTQCInit(object objectParameters)
        {
            try
            {
                ThreadParameters threadParameters
                    = (ThreadParameters)objectParameters;

                // Event variables
                WaitHandle[] initializationEvents
                    = new WaitHandle[(int)EventIndex.NumberOfWorkEvents];
                int indexOfSignalEvent;

                if (IsInitialized == true)
                {
                    Debug.WriteLine("RTQCInit initial twice.");
                    return;
                }

                // Variables initialization
                IsInitialized = false;
                IsReady = false;
                IsContinue = true;

                // Event array for WaitHandle
                initializationEvents[(int)EventIndex.StartWorkEvent]
                    = threadParameters.StartWorkEvent;

                //
                // Modules initialization code here...
                //

                //
                // End of Phase 1
                //
                threadParameters.ReadyToWorkEvent.Set();
                Debug.WriteLine("RTQCInit complete Phase 1 Initialization");

                indexOfSignalEvent = WaitHandle.WaitAny(initializationEvents);

                if (indexOfSignalEvent != (int)EventIndex.StartWorkEvent)
                    goto CleanupExit;

                IsInitialized = true;
                Debug.WriteLine("RTQCInit complete Phase 2 Initialization" +
                                "and start working.");

                //
                // Main Thread of RTQC module (begin)
                //
                while (IsContinue == true)
                {
                    IsReady = true;

                    //
                    // Wait for working events
                    // Switch case for different events
                    //
                }
            }
            catch (ThreadAbortException e) // Catch the exception thrown by 
                                           // Thread.Abort() in main.
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine("RTQC module thread is aborting...");
                goto CleanupExit;
            }

CleanupExit:
            //
            // Cleanup code
            //
            return;
        }
    }
}
