using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Diagnostics;

namespace DiReCT
{
    #region Program Definitions
    public enum ModuleThread
    {
        AAA = 0,
        DM,
        DS,
        MAN,
        RTQC,
        NumberOfModules
    };

    public enum TimeInterval
    {
        VeryShortTime = 1000,   // 1000 milliseconds, 1 second
        ShortTime = 10000,      // 10000 milliseconds, 10 seconds
        LongTime = 60000,       // 60000 milliseconds, 1 minute
        VeryLongTime = 300000   // 300000 milliseconds, 5 minutes
    };

    public enum EventIndex
    {
        StartWorkEvent = 0,
        NumberOfWorkEvents
    };
    #endregion

    #region Program Shared Data
    class ThreadParameters
    {
        private AutoResetEvent readyToWorkEvent;
        private AutoResetEvent startWorkEvent;

        public ThreadParameters()
        {
            readyToWorkEvent = new AutoResetEvent(false);
            startWorkEvent = new AutoResetEvent(false);
        }

        #region Properties

        public AutoResetEvent ReadyToWorkEvent
        {
            get
            {
                return readyToWorkEvent;
            }
        }

        public AutoResetEvent StartWorkEvent
        {
            get
            {
                return startWorkEvent;
            }
        }

        #endregion
    }
    class ModuleControlData
    {
        private ThreadParameters threadParameters;

        public ModuleControlData()
        {
            threadParameters = new ThreadParameters();
        }

        #region Properties

        public ThreadParameters ThreadParameters
        {
            get
            {
                return threadParameters;
            }
        }

        #endregion
    }
    #endregion

    /// <summary>
    /// The main entry point for the application
    /// </summary>
    class DiReCTMainProgram : Application
    {
        private static Thread[] moduleThreadHandles;
        private static Thread UIThreadHandle;
        private static ModuleControlData[] modulesControlData;
        private static AutoResetEvent[] moduleReadyEvents;
        private static bool IsInitFailed = false; // Whether initialization 
                                                  // processes were completed
                                                  // in time
        private static Timer InitializationTimer;
        
        //ShutdownEvent <- close the app & windows shutdown close app
               
        [MTAThread]
        static void Main()
        {
            // Initialize objects of threads
            try
            {
                moduleThreadHandles
                    = new Thread[(int)ModuleThread.NumberOfModules];
                modulesControlData = new ModuleControlData[
                    (int)ModuleThread.NumberOfModules];
                moduleReadyEvents = new AutoResetEvent[
                    (int)ModuleThread.NumberOfModules];
            }
            catch (OutOfMemoryException e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine("Objects of threads initialization failed.");
                goto InitializationFailed;
            }

            // Initialize thread objects and control data of modules
            // Start to execute modules
            try
            {
                // AAA Module
                moduleThreadHandles[(int)ModuleThread.AAA]
                    = new Thread(AAAModule.AAAInit);
                modulesControlData[(int)ModuleThread.AAA]
                    = new ModuleControlData();
                moduleThreadHandles[(int)ModuleThread.AAA]
                        .Start(modulesControlData[(int)ModuleThread.AAA]
                        .ThreadParameters);
                // DM Module
                moduleThreadHandles[(int)ModuleThread.DM]
                    = new Thread(DMModule.DMInit);
                modulesControlData[(int)ModuleThread.DM]
                    = new ModuleControlData();
                moduleThreadHandles[(int)ModuleThread.DM]
                        .Start(modulesControlData[(int)ModuleThread.DM]
                        .ThreadParameters);
                // DS Module
                moduleThreadHandles[(int)ModuleThread.DS]
                    = new Thread(DSModule.DSInit);
                modulesControlData[(int)ModuleThread.DS]
                    = new ModuleControlData();
                moduleThreadHandles[(int)ModuleThread.DS]
                        .Start(modulesControlData[(int)ModuleThread.DS]
                        .ThreadParameters);
                // MAN Module
                moduleThreadHandles[(int)ModuleThread.MAN]
                    = new Thread(MANModule.MANInit);
                modulesControlData[(int)ModuleThread.MAN]
                    = new ModuleControlData();
                moduleThreadHandles[(int)ModuleThread.MAN]
                        .Start(modulesControlData[(int)ModuleThread.MAN]
                        .ThreadParameters);
                // RTQC Module
                moduleThreadHandles[(int)ModuleThread.RTQC]
                    = new Thread(RTQCModule.RTQCInit);
                modulesControlData[(int)ModuleThread.RTQC]
                    = new ModuleControlData();
                moduleThreadHandles[(int)ModuleThread.RTQC]
                        .Start(modulesControlData[(int)ModuleThread.RTQC]
                        .ThreadParameters);
            }
            catch (OutOfMemoryException e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine("Some module thread cannot start.");
                goto InitializationFailed;
            }

            // Initialize the thread object of UI thread
            try
            {
                UIThreadHandle = new Thread(UIMainFunction);
                UIThreadHandle.SetApartmentState(ApartmentState.STA);
            }catch(OutOfMemoryException e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine("UI thread initialization failed.");
                goto InitializationFailed;
            }

            // Set up the array of Ready events
            for (int i = 0; i < (int)ModuleThread.NumberOfModules; i++)
            {
                moduleReadyEvents[i] 
                    = modulesControlData[i].ThreadParameters.ReadyToWorkEvent;
            }            

            while (!IsInitFailed)
            {
                if (WaitHandle.WaitAll(moduleReadyEvents, 
                                       (int)TimeInterval.LongTime, 
                                       true))
                {
                    Debug.WriteLine(
                        "Phase 1 initialization of all modules complete!");
                    break;
                }
                else
                {
                    for (int i = 0; i < moduleThreadHandles.Length; i++)
                    {
                        Thread moduleThreadHandle = moduleThreadHandles[i];
                        if (!moduleThreadHandle.IsAlive || 
                            !moduleReadyEvents[i].WaitOne(0))
                        {
                            string ModuleName 
                                = Enum.GetName(typeof(ModuleThread), i);
                            Debug.WriteLine("Phase 1 initialization of "
                                            + ModuleName + " module fails!");
                            IsInitFailed = true;                            
                        }                        
                    }

                    goto InitializationFailed;
                }
            }

            //InitializationTimer.Dispose();

            // Signal all created threads to start working
            foreach (ModuleControlData moduleControlData in modulesControlData)
            {
                moduleControlData.ThreadParameters.StartWorkEvent.Set();
            }

            //Start to execute UI
            try
            {
                UIThreadHandle.Start();
            }
            catch (OutOfMemoryException e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine("UI thread can not start!");
                goto InitializationFailed;
            }

            //
            // Code of Helper thread here...
            //

            UIThreadHandle.Join();

            Return:
            
            // Signal all created threads to prepare to terminate
            foreach (Thread moduleThreadHandle in moduleThreadHandles)
            {
                if (moduleThreadHandle.ThreadState
                    != System.Threading.ThreadState.Unstarted)
                {
                    moduleThreadHandle.Abort();
                }
            }

            InitializationTimer = new Timer(
                            new TimerCallback(AbortTimeOutEventHandler),
                            moduleThreadHandles,
                            (int)TimeInterval.LongTime,
                            Timeout.Infinite); // Callback is invoked once

            // Wait for all created threads to be terminated
            foreach (Thread moduleThreadHandle in moduleThreadHandles)
            {
                if (moduleThreadHandle.ThreadState 
                    != System.Threading.ThreadState.Unstarted)
                {
                    moduleThreadHandle.Join();
                }
            }
            return;

            InitializationFailed:
                goto Return;
        }
        
        private static void UIMainFunction()
        {
            DiReCTMainProgram DiReCTmainProgram = new DiReCTMainProgram();
            DiReCTmainProgram.StartupUri 
                = new System.Uri("DiReCTCore\\MainWindow.xaml", 
                                 System.UriKind.Relative);
            DiReCTmainProgram.Run();
        }

        private static void AbortTimeOutEventHandler(object state)
        {
            for (int i = 0; i < moduleThreadHandles.Length; i++)
            {
                Thread moduleThreadHandle = moduleThreadHandles[i];
                if (moduleThreadHandle.IsAlive)
                {
                    string ModuleName
                        = Enum.GetName(typeof(ModuleThread), i);
                    Debug.WriteLine(
                        ModuleName + "module thread aborting timed out.");
                }
            }
            return;
        }
    }    
}
