using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace DiReCT
{
    class MANModule
    {
        // State control variable
        static bool IsInitialized = false;
        static bool IsReady = false;
        static bool IsContinue = true;

        public static void MANInit(object objectParameters)
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
                    Debug.WriteLine("MANInit initial twice.");
                    return;
                }

                // Variables initialization
                IsInitialized = false;
                IsReady = false;
                IsContinue = true;

                // Event variables initialization
                initializationEvents[(int)EventIndex.StartWorkEvent]
                    = threadParameters.StartWorkEvent;

                //
                // Modules initialization code here...
                //

                //
                // End of Phase 1
                //
                threadParameters.ReadyToWorkEvent.Set();
                Debug.WriteLine("MANInit complete Phase 1 Initialization");

                indexOfSignalEvent = WaitHandle.WaitAny(initializationEvents);

                if (indexOfSignalEvent != (int)EventIndex.StartWorkEvent)
                    goto Return;

                IsInitialized = true;
                Debug.WriteLine(
                    "MANInit complete Phase 2 Initialization" +
                    "and start working.");

                //
                // Main Thread of MaN module (begin)
                //
                while (IsContinue == true)
                {
                    IsReady = true;

                    // temporary demo code

                    //
                    // Wait for working events
                    // Switch case for different events
                    //
                }
            }
            catch (ThreadAbortException e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine("MAN module thread is aborting...");
                goto Return;
            }
            Return:
            //
            // Cleanup code
            //
            return;
        }
    }
}
