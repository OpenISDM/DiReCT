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
 *      MainProgram.cs
 * 
 * Abstract:
 *      
 *      The main program of DiReCT creates all resources including 
 *      sychronization objects, work queues, worker threads, UI threads,
 *      and turn itself into a UI helper thread.
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
using System.Windows;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;

namespace DiReCT
{
    static class Constants
    {
        // Time interval constants
        public const int VERY_SHORT_TIME = 1000; // 1000 milliseconds, 1 second 
        public const int SHORT_TIME = 10000; // 10000 milliseconds, 10 seconds
        public const int LONG_TIME = 60000; // 60000 milliseconds, 1 minute
        public const int VERY_LONG_TIME = 300000; // 300000 milliseconds,
                                                  // 5 minutes
    }

    #region Program Definitions
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
        VeryShortTime = Constants.VERY_SHORT_TIME,   
        ShortTime = Constants.SHORT_TIME,      
        LongTime = Constants.LONG_TIME,       
        VeryLongTime = Constants.VERY_LONG_TIME   
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
    class ModuleControlDataBlock
    {
        private ThreadParameters threadParameters;

        public ModuleControlDataBlock()
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
    /// The main entry point of DiReCT application
    /// </summary>
    class DiReCTMainProgram : Application
    {
        private static Thread[] moduleThreadHandles;
        private static Thread UIThreadHandle;
        private static ModuleControlDataBlock[] moduleControlDataBlocks;
        private static AutoResetEvent[] moduleReadyEvents;
        private static bool HasInitFailed = false; // Whether initialization 
                                                   // processes were completed
                                                   // in time
        private static Timer InitializationTimer;

        [MTAThread]
        static void Main()
        {
            // Subscribe system log off/shutdown or application close events
            SystemEvents.SessionEnding
                += new SessionEndingEventHandler(ShutdownEventHandler);
            AppDomain.CurrentDomain.ProcessExit
                += new EventHandler(ShutdownEventHandler);

            // Initialize objects for threads
            // Initialize thread objects and control data block of each modules
            try
            {
                moduleThreadHandles
                    = new Thread[(int)ModuleThread.NumberOfModules];
                moduleControlDataBlocks = new ModuleControlDataBlock[
                    (int)ModuleThread.NumberOfModules];
                moduleReadyEvents = new AutoResetEvent[
                    (int)ModuleThread.NumberOfModules];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Objects of threads initialization failed.");
                goto CleanupExit;
            }
            
            // AAA Module
            try
            {
                // Create and intialize AAA module
                moduleThreadHandles[(int)ModuleThread.AAA]
                    = new Thread(AAAModule.AAAInit);
                moduleControlDataBlocks[(int)ModuleThread.AAA]
                    = new ModuleControlDataBlock();
                moduleThreadHandles[(int)ModuleThread.AAA]
                        .Start(moduleControlDataBlocks[(int)ModuleThread.AAA]
                        .ThreadParameters);
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("AAA module thread creation failed.");
                goto CleanupExit;
            }

            // DM Module
            try
            {
                // Create and intialize DM module
                moduleThreadHandles[(int)ModuleThread.DM]
                        = new Thread(DMModule.DMInit);
                moduleControlDataBlocks[(int)ModuleThread.DM]
                    = new ModuleControlDataBlock();
                moduleThreadHandles[(int)ModuleThread.DM]
                        .Start(moduleControlDataBlocks[(int)ModuleThread.DM]
                        .ThreadParameters);
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("DM module thread creation failed.");
                goto CleanupExit;
            }

            // DS Module
            try
            {
                // Create and intialize DS module
                moduleThreadHandles[(int)ModuleThread.DS]
                        = new Thread(DSModule.DSInit);
                moduleControlDataBlocks[(int)ModuleThread.DS]
                    = new ModuleControlDataBlock();
                moduleThreadHandles[(int)ModuleThread.DS]
                        .Start(moduleControlDataBlocks[(int)ModuleThread.DS]
                        .ThreadParameters);
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("DS module thread creation failed.");
                goto CleanupExit;
            }

            // MAN Module
            try
            {
                // Create and intialize MAN module
                moduleThreadHandles[(int)ModuleThread.MAN]
                        = new Thread(MANModule.MANInit);
                moduleControlDataBlocks[(int)ModuleThread.MAN]
                    = new ModuleControlDataBlock();
                moduleThreadHandles[(int)ModuleThread.MAN]
                        .Start(moduleControlDataBlocks[(int)ModuleThread.MAN]
                        .ThreadParameters);
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("MAN module thread creation failed.");
                goto CleanupExit;
            }

            // RTQC Module
            try
            {
                // Create and intialize RTQC module
                moduleThreadHandles[(int)ModuleThread.RTQC]
                        = new Thread(RTQCModule.RTQCInit);
                moduleControlDataBlocks[(int)ModuleThread.RTQC]
                    = new ModuleControlDataBlock();
                moduleThreadHandles[(int)ModuleThread.RTQC]
                        .Start(moduleControlDataBlocks[(int)ModuleThread.RTQC]
                        .ThreadParameters);
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("RTQC module thread creation failed.");
                goto CleanupExit;
            }

            // Initialize the thread object of UI thread
            try
            {
                UIThreadHandle = new Thread(UIMainFunction);
                UIThreadHandle.SetApartmentState(ApartmentState.STA);
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("UI thread creation failed.");
                goto CleanupExit;
            }

            // Initialize the array of Ready events for WaitHandle
            for (int i = 0; i < (int)ModuleThread.NumberOfModules; i++)
            {
                moduleReadyEvents[i]
                    = moduleControlDataBlocks[i].ThreadParameters
                                                .ReadyToWorkEvent;
            }

            while (!HasInitFailed)
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
                        if (!moduleThreadHandle.IsAlive)
                        {
                            string ModuleName
                                = Enum.GetName(typeof(ModuleThread), i);
                            Debug.WriteLine("Phase 1 initialization of "
                                            + ModuleName + " module fails!");
                            HasInitFailed = true;
                        }
                    }

                    goto CleanupExit;
                }
            }

            // Signal all created threads to start working
            foreach (ModuleControlDataBlock moduleControlData
                     in moduleControlDataBlocks)
            {
                moduleControlData.ThreadParameters.StartWorkEvent.Set();
            }

            //Start to execute UI
            try
            {
                UIThreadHandle.Start();
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("UI thread can not start!");
                goto CleanupExit;
            }

            //
            // Code of Helper thread here...
            //

            UIThreadHandle.Join();

CleanupExit:

            // Signal all created threads to prepare to terminate
            foreach (Thread moduleThreadHandle in moduleThreadHandles)
            {
                if (moduleThreadHandle.ThreadState
                    != System.Threading.ThreadState.Unstarted)
                {
                    moduleThreadHandle.Abort();
                }
            }

            InitializationTimer 
                = new Timer(new TimerCallback(AbortTimeOutEventHandler),
                            moduleThreadHandles,
                            (int)TimeInterval.LongTime,
                            Timeout.Infinite); // Callback is invoked once

            // Wait for all created threads to terminate
            foreach (Thread moduleThreadHandle in moduleThreadHandles)
            {
                if (moduleThreadHandle.ThreadState
                    != System.Threading.ThreadState.Unstarted)
                {
                    moduleThreadHandle.Join();
                }
            }
            return;
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

        private static void ShutdownEventHandler(object sender, EventArgs e)
        {
            Debug.WriteLine("Windows is shutting down...");
            Debug.WriteLine("Cleaning up...");

            
            //
            // Cannot goto CleanupExit
            // Do clean up here...
            //
            
            //File.AppendAllText("Log.txt", DateTime.Now.ToString()+" DiReCT is shutting down..."+Environment.NewLine, Encoding.Unicode);
        }
    }    
}
