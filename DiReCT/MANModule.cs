using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using DiReCT.Model.Utilities;

namespace DiReCT
{
    class MANModule
    {
        // State control variable
        static bool IsReady;
        static bool IsContinuing;
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;
        static PriorityWorkQueue moduleWorkQueue;

        public static void MANInit(object objectParameters)
        {

            moduleControlDataBlock
                = (ModuleControlDataBlock)objectParameters;
            threadParameters = moduleControlDataBlock.ThreadParameters;
            moduleWorkQueue = moduleControlDataBlock.ModuleWorkQueue;
            try
            {
                // Variables initialization
                IsReady = false;
                IsContinuing = true;

                //
                // Modules initialization code here...
                //

                //
                // End of Phase 1 initialization
                //                

                threadParameters.ModuleReadyEvent.Set();
                Debug.WriteLine("MANInit complete Phase 1 Initialization");

                //
                // Phase 2 initialization code
                //

                DiReCTMainProgram.ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("MANInit complete Phase 2 Initialization" +
                                "and start working.");

                //
                // End of Phase 2 initialization
                //

                //
                // Main Thread of MAN module (begin)
                //
                while (IsContinuing == true)
                {
                    IsReady = true;

                    // Wait for working events
                    moduleWorkQueue.Dequeue();

                    // To do...
                }
            }
            catch (ThreadAbortException ex) // Catch the exception thrown by 
                                            // Thread.Abort() in main.
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("MAN module thread is aborting...");
                Thread.ResetAbort(); // Avoid exception rethrowning at the end 
                                     // of the catch block.
                goto CleanupExit;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("MAN module thread failed.");
                goto CleanupExit;
            }

CleanupExit:
//
// Cleanup code
//
            threadParameters.ModuleInitFailedEvent.Set();
            Debug.WriteLine("MAN ModuleInitFailedEvent Set");
            return;
        }
    }
}
