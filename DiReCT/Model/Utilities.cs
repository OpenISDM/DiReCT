using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace DiReCT.Model.Utilities
{
    class WorkItem : IDisposable
    {
        private WorkFunction workFunctionName; // Enum of different functions
        private Delegate workFunctionDelegate; // For functions not included in
                                               // WorkFunction enumerator
        private AsyncCallback callBackFunction;
        private Object inputParameters; // Parameters for work function
        private Object outputParameters; // Parameters for call back function
        private IntPtr moduleAsyncResult; // Returning results
        public int Priority { get; set; } // Lower-priority numeric values mean 
                                          // higher priority (Level 0~7)

        // Private member used for implementation of IDisposable interface
        private bool IsDisposed = false;

        // Constructor using WorkFunction enumerator
        public WorkItem(WorkFunction workFunctionName,
                        AsyncCallback callBackFunction,
                        Object inputParameters,
                        int priority)
        {
            // Initialize members
            this.workFunctionName = workFunctionName;
            this.callBackFunction = callBackFunction;
            this.inputParameters = inputParameters;
            outputParameters = null;
            moduleAsyncResult = IntPtr.Zero;
            this.Priority = priority;
        }

        // Constructor using WorkFunctionDelegate
        public WorkItem(Delegate workFunctionDelegate,
                        AsyncCallback callBackFunction,
                        Object inputParameters,
                        int priority)
        {
            // Initialize members
            this.workFunctionDelegate = workFunctionDelegate;
            this.callBackFunction = callBackFunction;
            this.inputParameters = inputParameters;
            outputParameters = null;
            moduleAsyncResult = IntPtr.Zero;
            this.Priority = priority;
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
                if (disposing)
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

    // An 8-priority-level work-queue which stores items in different
    // priority level collections
    class PriorityWorkQueue
    {
        private BlockingCollection<WorkItem> priority0DataList;
        private BlockingCollection<WorkItem> priority1DataList;
        private BlockingCollection<WorkItem> priority2DataList;
        private BlockingCollection<WorkItem> priority3DataList;
        private BlockingCollection<WorkItem> priority4DataList;
        private BlockingCollection<WorkItem> priority5DataList;
        private BlockingCollection<WorkItem> priority6DataList;
        private BlockingCollection<WorkItem> priority7DataList;
        private BlockingCollection<WorkItem>[] queue;
        private Dictionary<int, BlockingCollection<WorkItem>> priorityMap;

        public PriorityWorkQueue()
        {
            priority0DataList = new BlockingCollection<WorkItem>();
            priority1DataList = new BlockingCollection<WorkItem>();
            priority2DataList = new BlockingCollection<WorkItem>();
            priority3DataList = new BlockingCollection<WorkItem>();
            priority4DataList = new BlockingCollection<WorkItem>();
            priority5DataList = new BlockingCollection<WorkItem>();
            priority6DataList = new BlockingCollection<WorkItem>();
            priority7DataList = new BlockingCollection<WorkItem>();
            queue = new BlockingCollection<WorkItem>[] { priority0DataList,
                                                         priority1DataList,
                                                         priority2DataList,
                                                         priority3DataList,
                                                         priority4DataList,
                                                         priority5DataList,
                                                         priority6DataList,
                                                         priority7DataList};
            priorityMap = new Dictionary<int, BlockingCollection<WorkItem>>();

            int priorityLevels = 8;
            for (int i = 0; i < priorityLevels; i++)
            {
                priorityMap.Add(i, queue[i]);
            }
        }

        public void Enqueue(WorkItem item)
        {
            priorityMap[item.Priority].Add(item);
        }

        public WorkItem Dequeue()
        {
            WorkItem workItem;
            BlockingCollection<WorkItem>.TakeFromAny(queue, out workItem);
            return workItem;
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
