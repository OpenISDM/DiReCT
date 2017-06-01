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
 *  DiReCT is distributed in the hope that it will be useful,
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
 *      Model/Utilities.cs
 * 
 * Abstract:
 *      
 *      The utilites of DiReCT contain all models for work processing, including
 *      work wrapping and queueing, worker thread management, and so on. These
 *      models can be used in core part and each module.
 *
 * Authors:
 *  
 *      Hunter Hsieh, hunter205@iis.sinica.edu.tw  
 *      Jeff Chen, jeff@iis.sinica.edu.tw
 * 
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;


namespace DiReCT.Model.Utilities
{
    public enum ThreadState
    {
        NotStarted,
        Executing,
        Completed,
        Aborted
    }

    public enum WorkPriority
    {
        Lowest,
        BelowNormal,
        Normal,
        AboveNormal,
        Highest,
        NumberOfPriorities
    }

    public enum FunctionGroupName
    {
        AuthenticateAuthoriseFunction = 0,
        DataManagementFunction,
        DataSyncFunction,
        MonitorAlertNotificationFunction,
        QualityControlFunction
    };

    public enum AsyncCallName
    {
        // AAA module function
        UserLogin = 0,
        UserLogout,
        
        // DM module function
        SaveRecord,

        // DS module function

        // MAN module function
        
        // RTQC module function
        Validate
    };

    public enum ErrorAndExceptionCode
    {
        NoError = 0,
        ErrorInvalidHandle,
        ErrorNotInitialized,
        ErrorModuleIsNotReady,
        NoException,
        UnknownException,
        InvalidWorkItemException,
        InvalidArgumentException
    };
    class WorkerThreadHandle
    {
        public Guid UID;
        public ThreadState threadState;
        public Thread thread;

        public WorkerThreadHandle()
        {
            UID = new Guid();
            threadState = ThreadState.NotStarted;
        }
    }

    // WorkerThreadPool incomplete...
    public class WorkerThreadPool<T>
    {
        private int minThreads;
        private int maxThreads;
        private int priorityLevels;

        // Time interval to free waiting threads in pool
        private int cleanupTimeout; // milliseconds

        private PriorityWorkQueue<WaitCallback> queue;
        private List<WorkerThreadHandle> pool;

        public WorkerThreadPool(int priorityLevels,
                                int minThreads, 
                                int maxThreads, 
                                int cleanupInterval)
        {
            this.priorityLevels = priorityLevels;
            this.minThreads = minThreads;
            this.maxThreads = maxThreads;
            this.cleanupTimeout = cleanupInterval;
            InitThreadPool();
        }

        private void InitThreadPool()
        {
            // 
            // To do...
            // Initialize the threadpool
            //
            throw new NotImplementedException();
        }
        public bool QueueUserWorkItem(T workItem, int workPriorities)
        {
            //
            // To do...
            // Queue workItem and pass state to worker thread.
            //
            throw new NotImplementedException();
        }
        
    }

    public class WorkItem : IAsyncResult, IDisposable
    {
        // private members used for implementation of IAsyncResult interface
        private Object asyncState;
        private ManualResetEvent asyncCompletedEvent;
        private Boolean completedSynchronously;
        private Boolean isCompleted;

        // Private member used for implementation of IDisposable interface
        private bool isDisposed = false;

        // private members used for processing asynchronous call
        private FunctionGroupName groupName;
        private AsyncCallName functionName;
        private Delegate workFunctionDelegate; // For functions not included in
                                               // WorkFunction enumerator
        private AsyncCallback callBackFunction;
        private Object inputParameters;  // Parameters for work function
        private Object outputParameters; // Parameters for call back function
        private ErrorAndExceptionCode exceptionCode;
        private String exceptionMessage;
        private Exception exception;
        private AutoResetEvent moduleAsyncAPICompletedEvent;
        private IntPtr moduleAsyncAPIResult;// Returning results

        // Constructor using WorkFunction enumerator
        public WorkItem(FunctionGroupName GroupName,
                        AsyncCallName AsyncCallName,
                        Object Parameters,
                        AsyncCallback CallBackFunction,
                        Object AsyncStates)
        {
            // Initialize members
            asyncState = AsyncState;
            asyncCompletedEvent = new ManualResetEvent(false);
            completedSynchronously = false;
            isCompleted = false;
            isDisposed = false;
            callBackFunction = CallBackFunction;
            groupName = GroupName;
            functionName = AsyncCallName;
            inputParameters = Parameters;
            outputParameters = null;
            exceptionCode = ErrorAndExceptionCode.NoException;
            exception = null;
            moduleAsyncAPICompletedEvent = new AutoResetEvent(false);
            moduleAsyncAPIResult = IntPtr.Zero;
        }

        public void Complete()
        {
            System.Diagnostics.Debug.Assert(!isCompleted, "iNuCWorkItem re-complete");
            System.Diagnostics.Debug.Assert(!isDisposed, "iNuCWorkItem alread disposed");
            isCompleted = true;
            asyncCompletedEvent.Set();
            if (callBackFunction != null)
            {
                callBackFunction(this);
            }
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(Boolean disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    asyncCompletedEvent.Close();
                    moduleAsyncAPICompletedEvent.Close();
                    asyncCompletedEvent = null;
                    moduleAsyncAPICompletedEvent = null;
                }

                // Dispose unmanaged resources
                if (moduleAsyncAPIResult != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(moduleAsyncAPIResult);
                    moduleAsyncAPIResult = IntPtr.Zero;
                }

                // Note disposing has been done
                isDisposed = true;
            }
        }

        ~WorkItem()
        {
            Dispose(false);
        }

        #region Properties of IAsyncResult

        public Object AsyncState
        {
            get
            {
                return asyncState;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return (WaitHandle)asyncCompletedEvent;
            }
        }

        public Boolean CompletedSynchronously
        {
            get
            {
                return completedSynchronously;
            }
        }

        public Boolean IsCompleted
        {
            get
            {
                return isCompleted;
            }
        }

        #endregion

        #region Properties

        public FunctionGroupName GroupName
        {
            get
            {
                return groupName;
            }
        }

        public AsyncCallName AsyncCallName
        {
            get
            {
                return functionName;
            }
        }

        public Object InputParameters
        {
            get
            {
                return inputParameters;
            }
        }

        public Object OutputParameters
        {
            get
            {
                return outputParameters;
            }

            set
            {
                outputParameters = value;
            }
        }

        public ErrorAndExceptionCode ExceptionCode
        {
            get
            {
                return exceptionCode;
            }

            set
            {
                exceptionCode = value;
            }
        }

        public String ExceptionMessage
        {
            get
            {
                return exceptionMessage;
            }

            set
            {
                exceptionMessage = value;
            }
        }

        public Exception Exception
        {
            get
            {
                return exception;
            }

            set
            {
                exception = value;
            }
        }

        public AutoResetEvent ModuleAsyncAPICompletedEvent
        {
            get
            {
                return moduleAsyncAPICompletedEvent;
            }
        }

        public IntPtr ModuleAsyncAPIResult
        {
            get
            {
                return moduleAsyncAPIResult;
            }

            set
            {
                moduleAsyncAPIResult = value;
            }
        }

        #endregion
    }

    // PriorityWorkQueue incomplete...
    // An multi-level priority work-queue which stores items in different
    // priority level collections
    public class PriorityWorkQueue<T>
    {
        private Queue<T>[] queue;

        // Priority level start from 0
        public PriorityWorkQueue(int levels)
        {
            queue = new Queue<T>[levels];

            for ( int i=0;i< levels; i++)
            {
                queue[i] = new Queue<T>();
            }
        }

        /// <summary>
        /// Add an item to work queue.
        /// </summary>
        /// <param name="item">The item to be added from the collection.</param>
        /// <param name="priority">The priority level of the item.</param>
        /// <returns>true if item could be added; otherwise false.</returns>
        public bool Enqueue(T item,
                            int priority,
                            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Takes an item from work queue in an order of prority order.
        /// </summary>
        /// <param name="item">
        /// The item to be removed from the collection.</param>
        /// <returns>The priority of the workItem; -1 if the operation has been 
        /// cancelled or an item could not be removed.</returns>
        public int Dequeue(out T workItem)
        {
            throw new NotImplementedException();
        }
    }


    // A priority queue which stores items in a binary heap
    // based on their priority values
    class GeneralPriorityQueue<T> where T : IComparable<T>
    {
        private List<T> dataList;
        public ManualResetEvent workArriveEvent;
        private Mutex mutex;

        public GeneralPriorityQueue()
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

            if (dataList.Count == 0)
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
}
