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
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace DiReCT
{
    class RTQCModule
    {
        // State control variable
        static bool IsInitialized;
        static bool IsReady;
        static bool IsContinue;

        static ThreadParameters threadParameters;

        public static void RTQCInit(object objectParameters)
        {
            try
            {
                
                threadParameters = (ThreadParameters)objectParameters;

                if (IsInitialized == true)
                {
                    Debug.WriteLine("RTQCInit initial twice.");
                    return;
                }

                // Variables initialization
                IsInitialized = false;
                IsReady = false;
                IsContinue = true;

                //
                // Modules initialization code here...
                //

                //
                // End of Phase 1
                //

                threadParameters.ModuleReadyEvent.Set();
                Debug.WriteLine("RTQCInit complete Phase 1 Initialization");

                DiReCTMainProgram.ModuleStartWorkEvent.WaitOne();

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
                    // To do...
                    // Wait for working events
                    // Switch case for different events, then
                    // 1. Use Task & BlockingCollection
                    // 2. Use BeginInvoke(Delegate, Object[])
                    //
                }
            }
            catch (ThreadAbortException ex) // Catch the exception thrown by 
                                            // Thread.Abort() in main.
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("RTQC module thread is aborting...");
                Thread.ResetAbort(); // Avoid exception rethrowning at the end 
                                     // of the catch block.
                goto CleanupExit;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("RTQC module thread failed.");
                goto CleanupExit;
            }

CleanupExit:
            //
            // Cleanup code
            //
            threadParameters.ModuleInitFailedEvent.Set();
            Debug.WriteLine("RTQC ModuleInitFailedEvent Set");
            return;
        }
    }
}
