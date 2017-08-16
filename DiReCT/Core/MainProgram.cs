/*
 * Copyright (c) 2016 Academia Sinica, Institude of Information Science
 * 
 *  This file is part of DiReCT.
 *
 *  DiReCT is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Foobar is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
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
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;
using AppHost;
using DiReCT.Model;
using Amib.Threading;

namespace DiReCT
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
        VeryVeryShortTime = Constants.VERY_VERY_SHORT_TIME,
        VeryShortTime = Constants.VERY_SHORT_TIME,   
        ShortTime = Constants.SHORT_TIME,      
        LongTime = Constants.LONG_TIME,       
        VeryLongTime = Constants.VERY_LONG_TIME   
    };
    #endregion

    #region Program Shared Data
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
            ModuleStartWorkEvent = DiReCTMainProgram.ModuleStartWorkEvent;
            ModuleAbortEvent = DiReCTMainProgram.ModuleAbortEvent;
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
    #endregion

    // The main entry point of DiReCT application
    public class DiReCTMainProgram
    {
        private static DiReCTCore coreControl;
        private static Thread[] ModuleThreadHandles;
        private static Thread UIThreadHandle;
        private static ModuleControlDataBlock[] ModuleControlDataBlocks;
        private static AutoResetEvent[] ModuleReadyEvents;
        private static AutoResetEvent[] ModuleInitFailedEvents;
        public static ManualResetEvent ModuleStartWorkEvent 
            = new ManualResetEvent(false);
        public static ManualResetEvent ModuleAbortEvent
            = new ManualResetEvent(false);

        private static bool InitHasFailed = false; // Whether initialization 
                                                   // processes were completed
                                                   // in time    
        [MTAThread]
        public static void Main()
        {

            // Subscribe system log off/shutdown or application close events
            SystemEvents.SessionEnding
                += new SessionEndingEventHandler(ShutdownEventHandler);
            AppDomain.CurrentDomain.ProcessExit
                += new EventHandler(ShutdownEventHandler);

            // Initialize objects for main thread
            try
            {
                coreControl = DiReCTCore.getInstance();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Thread resource creation failed.");
                CleanupExit();
            }

            // Initialize objects for threads
            // Initialize thread objects and control data block of each modules
            try
            {
                ModuleThreadHandles
                    = new Thread[(int)ModuleThread.NumberOfModules];
                ModuleControlDataBlocks = new ModuleControlDataBlock[
                    (int)ModuleThread.NumberOfModules];
                ModuleReadyEvents = new AutoResetEvent[
                    (int)ModuleThread.NumberOfModules];
                ModuleInitFailedEvents = new AutoResetEvent[
                    (int)ModuleThread.NumberOfModules];              
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Thread resource creation failed.");
                CleanupExit();
            }

            // AAA Module
            try
            {
                // Create and intialize AAA module
                ModuleThreadHandles[(int)ModuleThread.AAA]
                    = new Thread(AAAModule.AAAInit);
                ModuleControlDataBlocks[(int)ModuleThread.AAA]
                    = new ModuleControlDataBlock();
                ModuleThreadHandles[(int)ModuleThread.AAA]
                        .Start(ModuleControlDataBlocks[(int)ModuleThread.AAA]);
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("AAA module thread creation failed.");
                CleanupExit();
            }

            // DM Module
            try
            {
                // Create and intialize DM module
                ModuleThreadHandles[(int)ModuleThread.DM]
                        = new Thread(DMModule.DMInit);
                ModuleControlDataBlocks[(int)ModuleThread.DM]
                    = new ModuleControlDataBlock();
                ModuleThreadHandles[(int)ModuleThread.DM]
                        .Start(ModuleControlDataBlocks[(int)ModuleThread.DM]);
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("DM module thread creation failed.");
                CleanupExit();
            }

            // DS Module
            try
            {
                // Create and intialize DS module
                ModuleThreadHandles[(int)ModuleThread.DS]
                        = new Thread(DSModule.DSInit);
                ModuleControlDataBlocks[(int)ModuleThread.DS]
                    = new ModuleControlDataBlock();
                ModuleThreadHandles[(int)ModuleThread.DS]
                        .Start(ModuleControlDataBlocks[(int)ModuleThread.DS]);
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("DS module thread creation failed.");
                CleanupExit();
            }

            // MAN Module
            try
            {
                // Create and intialize MAN module
                ModuleThreadHandles[(int)ModuleThread.MAN]
                        = new Thread(MANModule.MANInit);
                ModuleControlDataBlocks[(int)ModuleThread.MAN]
                    = new ModuleControlDataBlock();
                ModuleThreadHandles[(int)ModuleThread.MAN]
                        .Start(ModuleControlDataBlocks[(int)ModuleThread.MAN]);
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("MAN module thread creation failed.");
                CleanupExit();
            }

            // RTQC Module
            try
            {
                // Create and intialize RTQC module
                ModuleThreadHandles[(int)ModuleThread.RTQC]
                        = new Thread(RTQCModule.RTQCInit);
                ModuleControlDataBlocks[(int)ModuleThread.RTQC]
                    = new ModuleControlDataBlock();
                ModuleThreadHandles[(int)ModuleThread.RTQC]
                        .Start(ModuleControlDataBlocks[(int)ModuleThread.RTQC]);
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("RTQC module thread creation failed.");
                CleanupExit();
            }

            // Initialize the thread object of GUI thread
            try
            {
                UIThreadHandle = new Thread(UIMainFunction);
                UIThreadHandle.SetApartmentState(ApartmentState.STA);
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("UI thread creation failed.");
                CleanupExit();
            }

            // Set the array of events for waiting signals raised by 
            // modules using WaitAll and WaitAny below
            for (int i = 0; i < (int)ModuleThread.NumberOfModules; i++)
            {
                ModuleReadyEvents[i]
                    = ModuleControlDataBlocks[i].ThreadParameters
                                                .ModuleReadyEvent;
                
                ModuleInitFailedEvents[i]
                    = ModuleControlDataBlocks[i].ThreadParameters
                                                .ModuleInitFailedEvent;
            }

            while (InitHasFailed == false)
            {
                if (WaitHandle.WaitAll(ModuleReadyEvents,
                                       (int)TimeInterval.LongTime,
                                       true))
                {
                    Debug.WriteLine(
                        "Phase 1 initialization of all modules complete!");
                    break;
                }
                else
                {
                    int WaitReturnValue
                       = WaitHandle.WaitAny(ModuleInitFailedEvents,
                                           (int)TimeInterval.VeryVeryShortTime,  
                                           true);
                    if (WaitReturnValue != WaitHandle.WaitTimeout)
                    {
                        InitHasFailed = true;
                        CleanupExit();
                    }
                }
            }
            Debug.WriteLine("Core Thread ID: " + Thread.CurrentThread.ManagedThreadId);
            // Signal modules to start working
            ModuleStartWorkEvent.Set();
            
            // Start to execute UI
            try
            {
                UIThreadHandle.Start();
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("UI thread can not start!");
                CleanupExit();
            }

            coreControl.Run();
            UIThreadHandle.Join();
            CleanupExit();
            //return;
        }

        /// <summary>
        /// Initialize UI
        /// </summary>
        private static void UIMainFunction()
        {
            App app2 = new App();
            app2.InitializeComponent();
            app2.Run();
            //Application App = new Application();
            //App.StartupUri = new Uri("MainWindow.xaml",
            //                     UriKind.Relative);
            //App.Run();
            Debug.WriteLine("UI Module is working...");
        }

        private static void AbortTimeOutEventHandler(object state)
        {
            for (int i = 0; i < ModuleThreadHandles.Length; i++)
            {
                Thread moduleThreadHandle = ModuleThreadHandles[i];
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

            CleanupExit();
        }

        private static void CleanupExit()
        {
            Debug.WriteLine("Enter CleanupExit.");

            Thread.Sleep(1000);

            // Signal all created threads to prepare to terminate
            ModuleAbortEvent.Set();

            Debug.WriteLine("Cleanup join!!");
            // Wait for all created threads to terminate
            foreach (Thread moduleThreadHandle in ModuleThreadHandles)
            {
                if (moduleThreadHandle.ThreadState
                    != System.Threading.ThreadState.Unstarted)
                {
                    moduleThreadHandle.Join((int)TimeInterval.LongTime);
                }
            }
            Debug.WriteLine("Cleanup complete!!");
        }
    }    
}
