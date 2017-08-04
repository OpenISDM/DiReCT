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
 *      Model/ThreadPool.cs
 * 
 * Abstract:
 *      
 *      This file contains classes for managing threadpools and worker threads.
 *
 * Authors:
 * 
 *      Hunter Hsieh, hunter205@iis.sinica.edu.tw  
 *      Joe Huang, huangjoe9@gmail.com
 * 
 */
using DiReCT.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiReCT.Model
{
    public class ThreadItem
    {
        public Thread _Thread { get; set; }
        public AutoResetEvent PromoteNewLeaderEvent { get; set; }
        public bool IsRunning { get; set; }

        public ThreadItem(ParameterizedThreadStart DoWork)
        {
            PromoteNewLeaderEvent = new AutoResetEvent(false);
            IsRunning = true;
            _Thread = new Thread(DoWork) { IsBackground = true };
        }
    }

    /// <summary>
    /// Controller for threadpool
    /// </summary>
    internal class ThreadpoolController : IDisposable
    {
        private Thread ControllerThread;
        private int CurrentThreadQuantity = 0;
        private int maxThread = 0;
        private int minThread = 0;
        private int threadIdleTime = 0;
        private List<ThreadItem> ThreadItemList = new List<ThreadItem>();
        private Queue<ThreadItem> IdleThreadQueue = new Queue<ThreadItem>();
        private DateTime becomeLeaderTimePeriod = DateTime.Now;
        public bool IsRunning = true;
        private object CurrentThreadQuantityLock = new object();
        public object ThreadVariableChangeLock = new object();
        private bool disposed = false;
        private ParameterizedThreadStart threadWork;

        /// <summary>
        /// Constructor of ThreadpoolController
        /// </summary>
        /// <param name="SizeMax">
        ///     Maximum threadpool size
        /// </param>
        /// <param name="SizeMin">
        ///     Minimum threadpool size
        /// </param>
        /// <param name="ThreadIdleTime">
        ///     The number of seconds a thread must be idle before decreasing
        ///     threadpool size to SizeMin
        /// </param>
        public ThreadpoolController(int SizeMax,
                                    int SizeMin,
                                    int ThreadIdleTime,
                                    ParameterizedThreadStart ThreadWork)
        {
            maxThread = SizeMax;
            minThread = SizeMin;
            threadIdleTime = ThreadIdleTime;
            ControllerThread = new Thread(Run) { IsBackground = true };
            ControllerThread.Start();
            becomeLeaderTimePeriod = DateTime.Now;
            threadWork = ThreadWork;
        }

        public void Init()
        {
            for (int i = 0; i < minThread; i++)
            {
                // All thread join idleThreadQueue
                ThreadItem threadItem = CreateThread();
                IdleThreadQueue.Enqueue(threadItem);
                Debug.WriteLine("Thread[{0}] Create Done",
                    threadItem._Thread.ManagedThreadId);
            }
        }

        // Controller Thread work
        private void Run()
        {
            while (IsRunning)
            {
                // Check leader thread idle in every 1 second
                SpinWait.SpinUntil(() => false, 1000);
                // Release the extra thread
                if (becomeLeaderTimePeriod.AddSeconds(threadIdleTime)
                        < DateTime.Now)
                    while (IdleThreadQueue.Count + 1 > minThread)
                    {
                        lock (CurrentThreadQuantityLock)
                            if (CurrentThreadQuantity <= minThread)
                                break;
                            else
                            {
                                // From idle thread queue dequeue
                                // a thread and close
                                ThreadItem threadItem;
                                lock (ThreadVariableChangeLock)
                                    threadItem = IdleThreadQueue.Dequeue();
                                threadItem.IsRunning = false;
                                threadItem.PromoteNewLeaderEvent.Set();
                            }
                    }
            }
        }

        // Close all threads
        private void CloseAllThread()
        {
            IsRunning = false;
            ControllerThread.Join();
            foreach (var q in ThreadItemList)
                q.PromoteNewLeaderEvent.Set();

            // Wait for all threads to close
            SpinWait.SpinUntil(() => (CurrentThreadQuantity == 0));
        }

        public ThreadItem GetThreadItem()
        {
            ThreadItem threadItem;
            lock (ThreadVariableChangeLock)
                if (IdleThreadQueue.Count != 0)
                {
                    threadItem = IdleThreadQueue.Dequeue();
                    Debug.WriteLine("Assign Thread[{0}] as a new leader",
                                    threadItem._Thread.ManagedThreadId);
                }
                else
                {
                    threadItem = CreateThread();

                    // This is for dubug only
                    DebugGetLeaderThread(threadItem);
                }

            return threadItem;
        }

        private ThreadItem CreateThread()
        {
            ThreadItem threadItem;
            lock (CurrentThreadQuantityLock)
                if (CurrentThreadQuantity < maxThread)
                {
                    threadItem = new ThreadItem(threadWork);
                    ThreadItemList.Add(threadItem);
                    threadItem._Thread.Start(threadItem);
                    CurrentThreadQuantity++;
                }
                else
                {
                    threadItem = null;
                }
            return threadItem;

        }

        // Decrease thread-number counter
        public void ReduceCurrentThreadQuantity()
        {
            lock (CurrentThreadQuantityLock)
                CurrentThreadQuantity--;
        }

        // Removes the specified Threaditem from ThreadItemList
        public void RemoveThread(ThreadItem threaditem)
        {
            ThreadItemList.Remove(threaditem);
        }

        // Record time of the thread to become leader 
        public DateTime BecomeLeaderTimePeriod
        {
            set
            {
                becomeLeaderTimePeriod = value;
            }
        }

        public ThreadItem EnqueueIdleThreadQueue
        {
            set
            {
                IdleThreadQueue.Enqueue(value);
            }
        }

        public void Dispose()
        {
            CloseAllThread();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                ControllerThread = null;
                IdleThreadQueue.Clear();
                IdleThreadQueue = null;
                ThreadItemList = null;
                CurrentThreadQuantityLock = null;
                ThreadVariableChangeLock = null;
            }

            disposed = true;
        }

        ~ThreadpoolController()
        {
            CloseAllThread();
            Dispose(false);
        }

        [Conditional("DEBUG")]
        private void DebugGetLeaderThread(ThreadItem threadItem)
        {
            if (threadItem != null)
                Debug.WriteLine("Thread[{0}] is Leader Create Done",
                                threadItem._Thread.ManagedThreadId);
            else
                Debug.WriteLine("No idle threads");
        }
    }
    public class DiReCTThreadPool : IDisposable
    {
        private ThreadpoolController threadPoolController;
        private ThreadItem leaderThreadItem;
        private PriorityQueue<WorkItem> workQueue
            = new PriorityQueue<WorkItem>();
        private bool disposed = false;
        private object workQueueLock = new object();

        /// <summary>
        /// Constructor of DiReCT threadpool
        /// </summary>
        /// <param name="SizeMax">
        ///     Maximum threadpool size
        /// </param>
        /// <param name="SizeMin">
        ///     Minimum threadpool size
        /// </param>
        /// <param name="ThreadIdleTime">
        /// The number of seconds a thread must be idle before decreasing
        /// threadpool size to SizeMin
        /// </param>
        public DiReCTThreadPool(int SizeMax, int SizeMin = 5,
                                int IdleTime = 60)
        {
            //check if variables are proper
            if (SizeMax < SizeMin)
                throw new ArgumentException(
                    "Parameter can not be less than MinThread", "MaxThread");
            if (SizeMin < 1)
                throw new ArgumentException(
                    "Parameter can not be less than 1", "MinThread");
            if (IdleTime < 0)
                throw new ArgumentException(
                    "Parameter can not be negative", "IdleTime");

            // Create a thread pool controller
            threadPoolController = new ThreadpoolController(SizeMax,
                                                            SizeMin,
                                                            IdleTime,
                                                            ThreadWork);
            threadPoolController.Init();

            // Ask for a ThreadItem and assign it as a leader
            leaderThreadItem = threadPoolController.GetThreadItem();
            leaderThreadItem.PromoteNewLeaderEvent.Set();
        }

        private void ThreadWork(object ThreadItem)
        {
            ThreadItem myThreadItem = (ThreadItem)ThreadItem;

            // Check whether the thread pool/follower thread is terminating
            while (threadPoolController.IsRunning && myThreadItem.IsRunning)
            {
                Debug.WriteLine("Thread[{0}] Waiting to be a Leader",
                            (myThreadItem)._Thread.ManagedThreadId);

                myThreadItem.PromoteNewLeaderEvent.WaitOne();

                // Record the time when thread becomes a leader
                threadPoolController.BecomeLeaderTimePeriod = DateTime.Now;

                //Wait for work to arrive
                SpinWait.SpinUntil(() => (
                    workQueue.Count != 0 ||
                    !threadPoolController.IsRunning ||
                    !(myThreadItem.IsRunning)
                ));

                if (threadPoolController.IsRunning &&
                (myThreadItem).IsRunning)
                {
                    // Get work from WorkQueue
                    WorkItem Work = null;

                    if (workQueue.Count != 0)
                    {
                        lock (workQueueLock)
                            Work = workQueue.Dequeue();
                    }

                    if (Work != null)
                    {
                        // Ask for a thread and assign it as a leader
                        lock (threadPoolController.ThreadVariableChangeLock)
                            leaderThreadItem = threadPoolController.
                                               GetThreadItem();
                        if (leaderThreadItem != null)
                        {
                            leaderThreadItem.PromoteNewLeaderEvent.Set();
                        }


                        // Do work
                        try
                        {
                            switch (Work.GroupName)
                            {

                                //All DM Function
                                case FunctionGroupName.DataManagementFunction:
                                    
                                    DMModule.DMWorkerFunctionProcessor(Work);
                                    break;

                                case FunctionGroupName.QualityControlFunction:
                                    RTQCModule.RTQCWorkerFunctionProcessor(
                                                                        Work);

                                    break;
                            }
                        }
                        catch
                        {
                            // Reset after the thread is aborted
                            if (Thread.CurrentThread.ThreadState
                                == System.Threading.ThreadState.AbortRequested ||
                                Thread.CurrentThread.ThreadState.ToString()
                                == "Background, AbortRequested")
                            {
                                Debug.WriteLine(
                                    "Thread[{0}] is aborted，reset Thread[{0}]",
                                    Thread.CurrentThread.ManagedThreadId);
                                Thread.ResetAbort();
                            }
                        }
                    }
                    // Become leader if no leader, else join idle thread queue
                    lock (threadPoolController.ThreadVariableChangeLock)
                        if (leaderThreadItem == null)
                        {
                            leaderThreadItem = myThreadItem;
                            leaderThreadItem.PromoteNewLeaderEvent.Set();
                            Debug.WriteLine("Thread[{0}] become a new leader",
                                         Thread.CurrentThread.ManagedThreadId);
                        }
                        else
                        {
                            Debug.WriteLine("Thread[{0}] join idle queue",
                                         Thread.CurrentThread.ManagedThreadId);
                            threadPoolController.EnqueueIdleThreadQueue
                                = myThreadItem;
                        }
                }
            }

            // Release thread when finish
            Debug.WriteLine("Thread[{0}] Close Done",
                            (myThreadItem)._Thread.ManagedThreadId);
            threadPoolController.ReduceCurrentThreadQuantity();
            threadPoolController.RemoveThread(myThreadItem);
            (myThreadItem).PromoteNewLeaderEvent.Close();
            (myThreadItem)._Thread = null;
        }

        /// <summary>
        /// 新增工作
        /// </summary>
        /// <param name="Work">工作</param>
        /// <param name="Priority">優先權</param>
        public void AddThreadWork(WorkItem Work,
                                  WorkPriority Priority = WorkPriority.Normal)
        {
            lock (workQueueLock)
                workQueue.Enqueue(Priority, Work);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            threadPoolController.Dispose();

            if (disposing)
            {
                leaderThreadItem = null;
                workQueue = null;
                workQueueLock = null;
            }

            disposed = true;
        }

        ~DiReCTThreadPool()
        {
            Dispose(false);
        }
    }


    public class PriorityQueue<T>
    {
        public PriorityQueue()
        {
            // Build the collection of priority chains.
            _priorityChains = new SortedList<int, PriorityChain<T>>(); 
            // NOTE: should be Priority
            _cacheReusableChains = new Stack<PriorityChain<T>>(5);

            _head = _tail = null;
            _count = 0;
        }

        // NOTE: not used
        // public int Count {get{return _count;}}

        public WorkPriority MaxPriority // NOTE: should be Priority
        {
            get
            {
                int count = _priorityChains.Count;

                if (count > 0)
                {
                    return (WorkPriority)_priorityChains.Keys[count - 1];
                }
                else
                {
                    return WorkPriority.BelowNormal; 
                    // NOTE: should be Priority.Invalid;
                }
            }
        }

        public PriorityItem<T> Enqueue(WorkPriority priority, T data) 
            // NOTE: should be Priority
        {
            // Find the existing chain for this priority, or create a new one
            // if one does not exist.
            PriorityChain<T> chain = GetChain(priority);

            // Wrap the item in a PriorityItem so we can put it in our
            // linked list.
            PriorityItem<T> priorityItem = new PriorityItem<T>(data);

            // Step 1: Append this to the end of the "sequential" linked list.
            InsertItemInSequentialChain(priorityItem, _tail);

            // Step 2: Append the item into the priority chain.
            InsertItemInPriorityChain(priorityItem, chain, chain.Tail);

            return priorityItem;
        }

        public T Dequeue()
        {
            // Get the max-priority chain.
            int count = _priorityChains.Count;
            if (count > 0)
            {
                PriorityChain<T> chain = _priorityChains.Values[count - 1];
                Debug.Assert(chain != null, 
                    "PriorityQueue.Dequeue: a chain should exist.");

                PriorityItem<T> item = chain.Head;
                Debug.Assert(item != null, 
                    "PriorityQueue.Dequeue: a priority item should exist.");

                RemoveItem(item);

                return item.Data;
            }
            else
            {
                throw new InvalidOperationException();

            }
        }

        public T Peek()
        {
            T data = default(T);

            // Get the max-priority chain.
            int count = _priorityChains.Count;
            if (count > 0)
            {
                PriorityChain<T> chain = _priorityChains.Values[count - 1];
                Debug.Assert(chain != null, 
                    "PriorityQueue.Peek: a chain should exist.");

                PriorityItem<T> item = chain.Head;
                Debug.Assert(item != null, 
                    "PriorityQueue.Peek: a priority item should exist.");

                data = item.Data;
            }

            return data;
        }

        public void RemoveItem(PriorityItem<T> item)
        {
            Debug.Assert(item != null, 
                "PriorityQueue.RemoveItem: invalid item.");
            Debug.Assert(item.Chain != null, 
                "PriorityQueue.RemoveItem: a chain should exist.");

            PriorityChain<T> chain = item.Chain;

            // Step 1: Remove the item from its priority chain.
            RemoveItemFromPriorityChain(item);

            // Step 2: Remove the item from the sequential chain.
            RemoveItemFromSequentialChain(item);

            // Note: we do not clean up empty chains on purpose to reduce churn.
        }

        public void ChangeItemPriority(PriorityItem<T> item, 
            WorkPriority priority) // NOTE: should be Priority
        {
            // Remove the item from its current priority and insert it into
            // the new priority chain.  Note that this does not change the
            // sequential ordering.

            // Step 1: Remove the item from the priority chain.
            RemoveItemFromPriorityChain(item);

            // Step 2: Insert the item into the new priority chain.
            // Find the existing chain for this priority, or create a new one
            // if one does not exist.
            PriorityChain<T> chain = GetChain(priority);
            InsertItemInPriorityChain(item, chain);
        }

        private PriorityChain<T> GetChain(WorkPriority priority) 
            // NOTE: should be Priority
        {
            PriorityChain<T> chain = null;

            int count = _priorityChains.Count;
            if (count > 0)
            {
                if (priority == (WorkPriority)_priorityChains.Keys[0])
                {
                    chain = _priorityChains.Values[0];
                }
                else if (priority == 
                    (WorkPriority)_priorityChains.Keys[count - 1])
                {
                    chain = _priorityChains.Values[count - 1];
                }
                else if ((priority > (WorkPriority)_priorityChains.Keys[0]) &&
                    (priority < (WorkPriority)_priorityChains.Keys[count - 1]))
                {
                    _priorityChains.TryGetValue((int)priority, out chain);
                }
            }

            if (chain == null)
            {
                if (_cacheReusableChains.Count > 0)
                {
                    chain = _cacheReusableChains.Pop();
                    chain.Priority = priority;
                }
                else
                {
                    chain = new PriorityChain<T>(priority);
                }

                _priorityChains.Add((int)priority, chain);
            }

            return chain;
        }

        private void InsertItemInPriorityChain(PriorityItem<T> item,
            PriorityChain<T> chain)
        {
            // Scan along the sequential chain, in the previous direction,
            // looking for an item that is already in the new chain.  We will
            // insert ourselves after the item we found.  We can short-circuit
            // this search if the new chain is empty.
            if (chain.Head == null)
            {
                Debug.Assert(chain.Tail == null, 
                    "PriorityQueue.InsertItemInPriorityChain:" + 
                    " both the head and the tail should be null.");
                InsertItemInPriorityChain(item, chain, null);
            }
            else
            {
                Debug.Assert(chain.Tail != null, 
                    "PriorityQueue.InsertItemInPriorityChain:"+
                    " both the head and the tail should not be null.");

                PriorityItem<T> after = null;

                // Search backwards along the sequential chain looking for an
                // item already in this list.
                for (after = item.SequentialPrev; after != null;
                     after = after.SequentialPrev)
                {
                    if (after.Chain == chain)
                    {
                        break;
                    }
                }

                InsertItemInPriorityChain(item, chain, after);
            }
        }

        internal void InsertItemInPriorityChain(PriorityItem<T> item,
            PriorityChain<T> chain, PriorityItem<T> after)
        {
            Debug.Assert(chain != null, 
                "PriorityQueue.InsertItemInPriorityChain:"+
                " a chain must be provided.");
            Debug.Assert(item.Chain == null && item.PriorityPrev == null &&
                item.PriorityNext == null,
                "PriorityQueue.InsertItemInPriorityChain:"+
                " item must not already be in a priority chain.");

            item.Chain = chain;

            if (after == null)
            {
                // Note: passing null for after means insert at the head.

                if (chain.Head != null)
                {
                    Debug.Assert(chain.Tail != null, 
                        "PriorityQueue.InsertItemInPriorityChain:"+
                        " both the head and the tail should not be null.");

                    chain.Head.PriorityPrev = item;
                    item.PriorityNext = chain.Head;
                    chain.Head = item;
                }
                else
                {
                    Debug.Assert(chain.Tail == null, 
                        "PriorityQueue.InsertItemInPriorityChain:"+
                        " both the head and the tail should be null.");

                    chain.Head = chain.Tail = item;
                }
            }
            else
            {
                item.PriorityPrev = after;

                if (after.PriorityNext != null)
                {
                    item.PriorityNext = after.PriorityNext;
                    after.PriorityNext.PriorityPrev = item;
                    after.PriorityNext = item;
                }
                else
                {
                    Debug.Assert(item.Chain.Tail == after, 
                        "PriorityQueue.InsertItemInPriorityChain:"+
                        " the chain's tail should be the item we "+
                        "are inserting after.");
                    after.PriorityNext = item;
                    chain.Tail = item;
                }
            }

            chain.Count++;
        }

        private void RemoveItemFromPriorityChain(PriorityItem<T> item)
        {
            Debug.Assert(item != null, 
                "PriorityQueue.RemoveItemFromPriorityChain: invalid item.");
            Debug.Assert(item.Chain != null, 
                "PriorityQueue.RemoveItemFromPriorityChain:"+
                " a chain should exist.");

            // Step 1: Fix up the previous link
            if (item.PriorityPrev != null)
            {
                Debug.Assert(item.Chain.Head != item, 
                    "PriorityQueue.RemoveItemFromPriorityChain: "+
                    "the head should not point to this item.");

                item.PriorityPrev.PriorityNext = item.PriorityNext;
            }
            else
            {
                Debug.Assert(item.Chain.Head == item, 
                    "PriorityQueue.RemoveItemFromPriorityChain: "+
                    "the head should point to this item.");

                item.Chain.Head = item.PriorityNext;
            }

            // Step 2: Fix up the next link
            if (item.PriorityNext != null)
            {
                Debug.Assert(item.Chain.Tail != item, 
                    "PriorityQueue.RemoveItemFromPriorityChain: "+
                    "the tail should not point to this item.");

                item.PriorityNext.PriorityPrev = item.PriorityPrev;
            }
            else
            {
                Debug.Assert(item.Chain.Tail == item, 
                    "PriorityQueue.RemoveItemFromPriorityChain: "+
                    "the tail should point to this item.");

                item.Chain.Tail = item.PriorityPrev;
            }

            // Step 3: cleanup
            item.PriorityPrev = item.PriorityNext = null;
            item.Chain.Count--;
            if (item.Chain.Count == 0)
            {
                if (item.Chain.Priority == 
                    (WorkPriority)_priorityChains.Keys[_priorityChains.Count - 1])
                {
                    _priorityChains.RemoveAt(_priorityChains.Count - 1);
                }
                else
                {
                    _priorityChains.Remove((int)item.Chain.Priority);
                }

                if (_cacheReusableChains.Count < 10)
                {
                    _cacheReusableChains.Push(item.Chain);
                }
            }

            item.Chain = null;
        }

        internal void InsertItemInSequentialChain(PriorityItem<T> item,
            PriorityItem<T> after)
        {
            Debug.Assert(item.SequentialPrev == null &&
                item.SequentialNext == null, 
                "PriorityQueue.InsertItemInSequentialChain: "+
                "item must not already be in the sequential chain.");

            if (after == null)
            {
                // Note: passing null for after means insert at the head.

                if (_head != null)
                {
                    Debug.Assert(_tail != null, 
                        "PriorityQueue.InsertItemInSequentialChain: "+
                        "both the head and the tail should not be null.");

                    _head.SequentialPrev = item;
                    item.SequentialNext = _head;
                    _head = item;
                }
                else
                {
                    Debug.Assert(_tail == null, 
                        "PriorityQueue.InsertItemInSequentialChain: "+
                        "both the head and the tail should be null.");

                    _head = _tail = item;
                }
            }
            else
            {
                item.SequentialPrev = after;

                if (after.SequentialNext != null)
                {
                    item.SequentialNext = after.SequentialNext;
                    after.SequentialNext.SequentialPrev = item;
                    after.SequentialNext = item;
                }
                else
                {
                    Debug.Assert(_tail == after, 
                        "PriorityQueue.InsertItemInSequentialChain: "+
                        "the tail should be the item we are inserting after.");
                    after.SequentialNext = item;
                    _tail = item;
                }
            }

            _count++;
        }

        private void RemoveItemFromSequentialChain(PriorityItem<T> item)
        {
            Debug.Assert(item != null, 
                "PriorityQueue.RemoveItemFromSequentialChain: invalid item.");

            // Step 1: Fix up the previous link
            if (item.SequentialPrev != null)
            {
                Debug.Assert(_head != item, 
                    "PriorityQueue.RemoveItemFromSequentialChain: "+
                    "the head should not point to this item.");

                item.SequentialPrev.SequentialNext = item.SequentialNext;
            }
            else
            {
                Debug.Assert(_head == item, 
                    "PriorityQueue.RemoveItemFromSequentialChain: "+
                    "the head should point to this item.");

                _head = item.SequentialNext;
            }

            // Step 2: Fix up the next link
            if (item.SequentialNext != null)
            {
                Debug.Assert(_tail != item, 
                    "PriorityQueue.RemoveItemFromSequentialChain: "+
                    "the tail should not point to this item.");

                item.SequentialNext.SequentialPrev = item.SequentialPrev;
            }
            else
            {
                Debug.Assert(_tail == item, 
                    "PriorityQueue.RemoveItemFromSequentialChain: "+
                    "the tail should point to this item.");

                _tail = item.SequentialPrev;
            }

            // Step 3: cleanup
            item.SequentialPrev = item.SequentialNext = null;
            _count--;
        }

        public int Count { get { return _priorityChains.Count; } }

        // Priority chains...
        private SortedList<int, PriorityChain<T>> _priorityChains; 
        // NOTE: should be Priority
        private Stack<PriorityChain<T>> _cacheReusableChains;

        // Sequential chain...
        private PriorityItem<T> _head;
        private PriorityItem<T> _tail;
        private int _count;
    }

    internal class PriorityChain<T>
    {
        public PriorityChain(WorkPriority priority) // NOTE: should be Priority
        {
            _priority = priority;
        }

        public WorkPriority Priority
        {
            get { return _priority; } set { _priority = value; }
        } // NOTE: should be Priority
        public int Count { get { return _count; } set { _count = value; } }
        public PriorityItem<T> Head
        {
            get { return _head; } set { _head = value; }
        }
        public PriorityItem<T> Tail
        {
            get { return _tail; } set { _tail = value; }
        }

        private PriorityItem<T> _head;
        private PriorityItem<T> _tail;
        private WorkPriority _priority;
        private int _count;
    }

    public class PriorityItem<T>
    {
        public PriorityItem(T data)
        {
            _data = data;
        }

        public T Data { get { return _data; } }
        public bool IsQueued { get { return _chain != null; } }

        // Note: not used
        // public WorkPriority Priority { get { return _chain.Priority; } } 
        // NOTE: should be Priority

        internal PriorityItem<T> SequentialPrev
        {
            get { return _sequentialPrev; } set { _sequentialPrev = value; }
        }
        internal PriorityItem<T> SequentialNext
        {
            get { return _sequentialNext; } set { _sequentialNext = value; }
        }

        internal PriorityChain<T> Chain
        {
            get { return _chain; } set { _chain = value; }
        }
        internal PriorityItem<T> PriorityPrev
        {
            get { return _priorityPrev; } set { _priorityPrev = value; }
        }
        internal PriorityItem<T> PriorityNext
        {
            get { return _priorityNext; } set { _priorityNext = value; }
        }

        private T _data;

        private PriorityItem<T> _sequentialPrev;
        private PriorityItem<T> _sequentialNext;

        private PriorityChain<T> _chain;
        private PriorityItem<T> _priorityPrev;
        private PriorityItem<T> _priorityNext;
    }
}
