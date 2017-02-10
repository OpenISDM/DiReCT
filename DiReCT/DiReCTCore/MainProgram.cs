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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

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

    public enum WorkFunction
    {
        // AAA module function
        UserLogin = 0,
        UserLogout,

        // DM module function
        SaveRecord,
        GetRecord,

        // DS module function

        // MAN module function
        SetNotification,
        SendNotification,

        // RTQC module function
        Validate,

        NumberOfFunctions
    };
    #endregion

    #region Program Shared Data
    class ThreadParameters
    {
        // Module initialization failed event
        // Pass by reference to ModuleInitFailedEvents[]
        public AutoResetEvent ModuleInitFailedEvent;

        // Module thread initialization complete and ready to work event
        // Pass by reference to ModuleReadyEvents[]
        public AutoResetEvent ModuleReadyEvent;

        public ThreadParameters()
        {
            ModuleInitFailedEvent = new AutoResetEvent(false);
            ModuleReadyEvent = new AutoResetEvent(false);
        }
    }

    class WorkItem : IDisposable, IComparable<WorkItem>
    {        
        private WorkFunction workFunctionName; // Enum of different functions
        private Delegate workFunctionDelegate; // For functions not included in
                                               // WorkFunction enumerator
        private AsyncCallback callBackFunction;
        private Object inputParameters; // Parameters for work function
        private Object outputParameters; // Parameters for call back function
        private IntPtr moduleAsyncResult; // Returning results
        private int priority; // Lower-priority numeric values mean 
                              // higher priority (Level 1~10)

        // Private member used for implementation of IDisposable interface
        private bool IsDisposed = false;

        // Constructor using WorkFunction enumerator
        public WorkItem(WorkFunction WorkFunctionName,
                        AsyncCallback CallBackFunction,
                        Object InputParameters,
                        int Priority)
        {
            // Initialize members
            workFunctionName = WorkFunctionName;
            callBackFunction = CallBackFunction;
            inputParameters = InputParameters;
            outputParameters = null;
            moduleAsyncResult = IntPtr.Zero;
            priority = Priority;
        }

        // Constructor using WorkFunctionDelegate
        public WorkItem(Delegate WorkFunctionDelegate,
                        AsyncCallback CallBackFunction,
                        Object InputParameters,
                        int Priority)
        {
            // Initialize members
            workFunctionDelegate = WorkFunctionDelegate;
            callBackFunction = CallBackFunction;
            inputParameters = InputParameters;
            outputParameters = null;
            moduleAsyncResult = IntPtr.Zero;
            priority = Priority;
        }

        public int CompareTo(WorkItem other)
        {
            if (this.priority < other.priority) return -1; // Higher priority
            else if (this.priority > other.priority) return 1; // Lower priority
            else return 0; // Same priority
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(Boolean disposing)
        {
            if (!IsDisposed)
            {
                if(disposing)
                {
                    //
                    // Dispose managed resources
                    //
                }

                //
                // Dispose unmanaged resources
                //
                if (moduleAsyncResult != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(moduleAsyncResult);
                    moduleAsyncResult = IntPtr.Zero;
                }

                // Note disposing has been done
                IsDisposed = true;
            }
        }
        
        ~WorkItem()
        {
            Dispose(false);
        }
    }

    class PriorityWorkQueue<T> where T : IComparable<T>
    {
        private List<T> dataList;
        public ManualResetEvent workArriveEvent;
        private Mutex mutex;

        public PriorityWorkQueue()
        {
            this.dataList = new List<T>();
            workArriveEvent = new ManualResetEvent(false);
            mutex = new Mutex();
        }

        public void Enqueue(T item)
        {
            mutex.WaitOne();

            dataList.Add(item);
            int childIndex = dataList.Count - 1; // Child index start at end

            while (childIndex > 0)
            {
                int parentIndex = (childIndex - 1) / 2;
                if (dataList[childIndex].CompareTo(dataList[parentIndex]) >= 0)
                    break; // Child item is larger than (or equal) parent

                //Swap child and parent
                T tmp = dataList[childIndex];
                dataList[childIndex] = dataList[parentIndex];
                dataList[parentIndex] = tmp;

                childIndex = parentIndex;
            }

            workArriveEvent.Set();
            mutex.ReleaseMutex();
        }

        public T Dequeue()
        {
            mutex.WaitOne();

            int lastIndex = dataList.Count - 1;
            T frontItem = dataList[0];   // Fetch the front
            dataList[0] = dataList[lastIndex];
            dataList.RemoveAt(lastIndex);
            --lastIndex;

            int parentIndex = 0; // Parent index start at front of queue

            while (true)
            {
                int childIndex = parentIndex * 2 + 1;
                if (childIndex > lastIndex) break;  // No children

                int rightChild = childIndex + 1;

                if (rightChild <= lastIndex && 
                    dataList[rightChild].CompareTo(dataList[childIndex]) < 0)
                    // if there is a rightChild (childIndex + 1), 
                    // and it is smaller than left child, 
                    // use the rightChild instead
                    childIndex = rightChild;

                if (dataList[parentIndex].CompareTo(dataList[childIndex]) <= 0)
                    break; // Parent is smaller than (or equal to) 
                           // smallest child
                
                // Swap parent and child
                T tmp = dataList[parentIndex];
                dataList[parentIndex] = dataList[childIndex];
                dataList[childIndex] = tmp;
                parentIndex = childIndex;
            }

            if(dataList.Count==0)
                workArriveEvent.Reset();

            return frontItem;
        }

        public T Peek()
        {
            T frontItem = dataList[0];
            return frontItem;
        }

        public int Count()
        {
            return dataList.Count;
        }
    }

    class ModuleControlDataBlock
    {
        // Core work-queue of each module
        public PriorityWorkQueue<WorkItem> ModuleWorkQueue;

        public ThreadParameters ThreadParameters;
        public ModuleControlDataBlock()
        {
            ModuleWorkQueue = new PriorityWorkQueue<WorkItem>();
            ThreadParameters = new ThreadParameters();
        }
    }
    #endregion


    // The main entry point of DiReCT application
    class DiReCTMainProgram : Application
    {
        private static Thread[] ModuleThreadHandles;
        private static Thread UIThreadHandle;
        private static ModuleControlDataBlock[] ModuleControlDataBlocks;
        private static AutoResetEvent[] ModuleReadyEvents;
        private static AutoResetEvent[] ModuleInitFailedEvents;
        public static ManualResetEvent ModuleStartWorkEvent 
            = new ManualResetEvent(false);

        private static bool InitHasFailed = false; // Whether initialization 
                                                   // processes were completed
                                                   // in time

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
                goto CleanupExit;
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
                goto CleanupExit;
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
                goto CleanupExit;
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
                goto CleanupExit;
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
                goto CleanupExit;
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
                goto CleanupExit;
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
                goto CleanupExit;
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

            while (!InitHasFailed)
            {
                if (WaitHandle.WaitAll(ModuleReadyEvents,
                                       (int)TimeInterval.VeryLongTime,
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
                        goto CleanupExit;
                    }
                }
            }

            ModuleStartWorkEvent.Set();            

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

            Debug.WriteLine("Enter CleanupExit.");

            // Signal all created threads to prepare to terminate
            foreach (Thread moduleThreadHandle in ModuleThreadHandles)
            {
                if (moduleThreadHandle.ThreadState
                    != System.Threading.ThreadState.Unstarted)
                {
                    moduleThreadHandle.Abort();
                }
            }

            // Wait for all created threads to terminate
            foreach (Thread moduleThreadHandle in ModuleThreadHandles)
            {
                if (moduleThreadHandle.ThreadState
                    != System.Threading.ThreadState.Unstarted)
                {
                    moduleThreadHandle.Join((int)TimeInterval.LongTime);
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


            //
            // Cannot goto CleanupExit
            // Do clean up here...
            //
Cleanup:
            // Signal all created threads to prepare to terminate
            foreach (Thread moduleThreadHandle in ModuleThreadHandles)
            {
                if (moduleThreadHandle.ThreadState
                    != System.Threading.ThreadState.Unstarted)
                {
                    moduleThreadHandle.Abort();
                }
            }

            // Wait for all created threads to terminate
            foreach (Thread moduleThreadHandle in ModuleThreadHandles)
            {
                if (moduleThreadHandle.ThreadState
                    != System.Threading.ThreadState.Unstarted)
                {
                    moduleThreadHandle.Join((int)TimeInterval.LongTime);
                }
            }
            return;

            //File.AppendAllText("Log.txt", DateTime.Now.ToString()+" DiReCT is shutting down..."+Environment.NewLine, Encoding.Unicode);
        }
    }    
}
