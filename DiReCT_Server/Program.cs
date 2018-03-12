using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;
using DiReCT.Network;

namespace DiReCT.Server
{
    static class Constants
    {
        // Time interval constants
        public const int VERY_VERY_SHORT_TIME = 100; // 0.1 seconds
        public const int VERY_SHORT_TIME = 1000; // 1000 milliseconds, 1 second 
        public const int SHORT_TIME = 10000; // 10000 milliseconds, 10 seconds
        public const int LONG_TIME = 60000; // 60000 milliseconds, 1 minute
        public const int VERY_LONG_TIME = 300000; // 300000 milliseconds,
                                                  // 5 minutes
    }

    public enum ModuleThread
    {
        AAA = 0,    // Authentication and Authorization module
        DM,         // Data Manager module
        DS,         // Data Synchronizer module
        MAN,        // Monitor and Notification module
        RTQC,       // Real-time Quality Control module
        NumberOfModules
    };

    public enum TimeInterval
    {
        VeryVeryShortTime = Constants.VERY_VERY_SHORT_TIME,
        VeryShortTime = Constants.VERY_SHORT_TIME,
        ShortTime = Constants.SHORT_TIME,
        LongTime = Constants.LONG_TIME,
        VeryLongTime = Constants.VERY_LONG_TIME
    };

    class ThreadParameters
    {
        // Event raised when module initialization failed
        // Pass by reference to ModuleInitFailedEvents[]
        public AutoResetEvent ModuleInitFailedEvent;

        // Event raised when module thread initialization complete 
        // and ready to work.
        // Pass by reference to ModuleReadyEvents[]
        public AutoResetEvent ModuleReadyEvent;

        // Event raised by main thread for notifying 
        // each modules to start working 
        public ManualResetEvent ModuleStartWorkEvent;

        // Event raised when module needs to abort
        // It is set by MainProgram when the program needs to be terminated
        public ManualResetEvent ModuleAbortEvent;

        // Thread pool that module uses
        public SmartThreadPool moduleThreadPool;

        // Maximum number of threads in thread pool
        const int THREADPOOL_SIZE = 10;

        public ThreadParameters()
        {
            ModuleInitFailedEvent = new AutoResetEvent(false);
            ModuleReadyEvent = new AutoResetEvent(false);
            ModuleStartWorkEvent = DiReCTServerProgram.ModuleStartWorkEvent;
            ModuleAbortEvent = DiReCTServerProgram.ModuleAbortEvent;
            moduleThreadPool = new SmartThreadPool();
        }
    }

    class ModuleControlDataBlock
    {
        public ThreadParameters ThreadParameters;

        public ModuleControlDataBlock()
        {
            ThreadParameters = new ThreadParameters();
        }
    }

    class DiReCTServerProgram
    {
        public static ManualResetEvent ModuleStartWorkEvent
            = new ManualResetEvent(false);
        public static ManualResetEvent ModuleAbortEvent
            = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            
        }


    }
}
