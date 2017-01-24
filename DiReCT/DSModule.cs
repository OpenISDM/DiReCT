using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace DiReCT
{
    class DSModule
    {
        // State control variable
        static bool IsInitialized = false;
        static bool IsReady = false;
        static bool IsContinue = true;

        static ThreadParameters threadParameters;

        public static void DSInit(object objectParameters)
        {
            try
            {

                threadParameters = (ThreadParameters)objectParameters;

                if (IsInitialized == true)
                {
                    Debug.WriteLine("DSInit initial twice.");
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
                Debug.WriteLine("DSInit complete Phase 1 Initialization");

                DiReCTMainProgram.ModuleStartWorkEvent.WaitOne();

                IsInitialized = true;
                Debug.WriteLine("DSInit complete Phase 2 Initialization" +
                                "and start working.");

                //
                // Main Thread of DS module (begin)
                //
                while (IsContinue == true)
                {
                    IsReady = true;

                    //
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
                Debug.WriteLine("DS module thread is aborting...");
                Thread.ResetAbort(); // Avoid exception rethrowning at the end 
                                     // of the catch block.
                goto CleanupExit;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("DS module thread failed.");
                goto CleanupExit;
            }

CleanupExit:
//
// Cleanup code
//
            threadParameters.ModuleInitFailedEvent.Set();
            return;
        }
    }
}
