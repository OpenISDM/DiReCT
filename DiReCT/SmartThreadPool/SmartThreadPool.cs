using System;
using System.Security;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using Amib.Threading.Internal;

namespace Amib.Threading
{
    public partial class SmartThreadPool : WorkItemsGroupBase, IDisposable
    {
        #region Public Default Constants

        /// <summary>
        /// Default minimum number of threads the thread pool contains. (0)
        /// </summary>
        public const int DefaultMinWorkerThreads = 0;

        /// <summary>
        /// Default maximum number of threads the thread pool contains. (25)
        /// </summary>
        public const int DefaultMaxWorkerThreads = 25;

        /// <summary>
        /// Default idle timeout in milliseconds. (One minute)
        /// </summary>
        public const int DefaultIdleTimeout = 60 * 1000; // One minute

        /// <summary>
        /// Indicate to copy the security context of the caller and then use it in the call. (false)
        /// </summary>
        public const bool DefaultUseCallerCallContext = false;

        /// <summary>
        /// Indicate to dispose of the state objects if they support the IDispose interface. (false)
        /// </summary>
        public const bool DefaultDisposeOfStateObjects = false;

        /// <summary>
        /// The default work item priority (WorkItemPriority.Normal)
        /// </summary>
        public const WorkItemPriority DefaultWorkItemPriority = WorkItemPriority.Normal;

        /// <summary>
        /// The default option to run the post execute (CallToPostExecute.Always)
        /// </summary>
        public const CallToPostExecute DefaultCallToPostExecute = CallToPostExecute.Always;

        /// <summary>
        /// The default Max Stack Size. (null)
        /// </summary>
        public static readonly int? DefaultMaxStackSize = null;

        /// <summary>
        /// The default Max Queue Length (null).
        /// </summary>
	    public static readonly int? DefaultMaxQueueLength = null;

        /// <summary>
        /// The default fill state with params. (false)
        /// It is relevant only to QueueWorkItem of Action&lt;...&gt;/Func&lt;...&gt;
        /// </summary>
        public const bool DefaultFillStateWithArgs = false;

        /// <summary>
        /// The default thread backgroundness. (true)
        /// </summary>
        public const bool DefaultAreThreadsBackground = true;


        #endregion

        #region Member Variables

        /// <summary>
        /// Dictionary of all the threads in the thread pool.
        /// </summary>
        private readonly SynchronizedDictionary<Thread, ThreadEntry> _workerThreads = new SynchronizedDictionary<Thread, ThreadEntry>();

        /// <summary>
        /// Queue of work items.
        /// </summary>
        private readonly WorkItemsQueue _workItemsQueue = new WorkItemsQueue();

        /// <summary>
        /// Count the work items handled.
        /// Used by the performance counter.
        /// </summary>
        private int _workItemsProcessed;

        /// <summary>
        /// Number of threads that currently work (not idle).
        /// </summary>
        private int _inUseWorkerThreads;

        /// <summary>
        /// Stores a copy of the original STPStartInfo.
        /// It is used to change the MinThread and MaxThreads
        /// </summary>
        private STPStartInfo _stpStartInfo;

        /// <summary>
        /// Total number of work items that are stored in the work items queue 
        /// plus the work items that the threads in the pool are working on.
        /// </summary>
        private volatile int _currentWorkItemsCount;

        /// <summary>
        /// Signaled when the thread pool is idle, i.e. no thread is busy
        /// and the work items queue is empty
        /// </summary>
        //private ManualResetEvent _isIdleWaitHandle = new ManualResetEvent(true);
        private ManualResetEvent _isIdleWaitHandle = EventWaitHandleFactory.CreateManualResetEvent(true);

        /// <summary>
        /// An event to signal all the threads to quit immediately.
        /// </summary>
        //private ManualResetEvent _shuttingDownEvent = new ManualResetEvent(false);
        private ManualResetEvent _shuttingDownEvent = EventWaitHandleFactory.CreateManualResetEvent(false);

        /// <summary>
        /// A common object for all the work items int the STP
        /// so we can mark them to cancel in O(1)
        /// </summary>
        private CanceledWorkItemsGroup _canceledSmartThreadPool = new CanceledWorkItemsGroup();

        /// <summary>
        /// A flag to indicate if the Smart Thread Pool is now suspended.
        /// </summary>
        private bool _isSuspended;

        /// <summary>
        /// A flag to indicate the threads to quit.
        /// </summary>
        private bool _shutdown;

        /// <summary>
        /// Counts the threads created in the pool.
        /// It is used to name the threads.
        /// </summary>
        private int _threadCounter;

        /// <summary>
        /// Indicate that the SmartThreadPool has been disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Holds all the WorkItemsGroup instaces that have at least one 
        /// work item int the SmartThreadPool
        /// This variable is used in case of Shutdown
        /// </summary>
        private readonly SynchronizedDictionary<IWorkItemsGroup, IWorkItemsGroup> _workItemsGroups = new SynchronizedDictionary<IWorkItemsGroup, IWorkItemsGroup>();

        /// <summary>
        /// Windows STP performance counters
        /// </summary>
        private ISTPInstancePerformanceCounters _windowsPCs = NullSTPInstancePerformanceCounters.Instance;

        /// <summary>
        /// Local STP performance counters
        /// </summary>
        private ISTPInstancePerformanceCounters _localPCs = NullSTPInstancePerformanceCounters.Instance;

        [ThreadStatic]
        private static ThreadEntry _threadEntry;

        /// <summary>
        /// An event to call after a thread is created, but before 
        /// it's first use.
        /// </summary>
        private event ThreadInitializationHandler _onThreadInitialization;

        /// <summary>
        /// An event to call when a thread is about to exit, after 
        /// it is no longer belong to the pool.
        /// </summary>
        private event ThreadTerminationHandler _onThreadTermination;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public SmartThreadPool()
        {
            _stpStartInfo = new STPStartInfo();
            Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idleTimeout">Idle timeout in milliseconds</param>
        public SmartThreadPool(int idleTimeout)
        {
            _stpStartInfo = new STPStartInfo
            {
                IdleTimeout = idleTimeout,
            };
            Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idleTimeout">Idle timeout in milliseconds</param>
        /// <param name="maxWorkerThreads">Upper limit of threads in the pool</param>
        public SmartThreadPool(
            int idleTimeout,
            int maxWorkerThreads)
        {
            _stpStartInfo = new STPStartInfo
            {
                IdleTimeout = idleTimeout,
                MaxWorkerThreads = maxWorkerThreads,
            };
            Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idleTimeout">Idle timeout in milliseconds</param>
        /// <param name="maxWorkerThreads">Upper limit of threads in the pool</param>
        /// <param name="minWorkerThreads">Lower limit of threads in the pool</param>
        public SmartThreadPool(
            int idleTimeout,
            int maxWorkerThreads,
            int minWorkerThreads)
        {
            _stpStartInfo = new STPStartInfo
            {
                IdleTimeout = idleTimeout,
                MaxWorkerThreads = maxWorkerThreads,
                MinWorkerThreads = minWorkerThreads,
            };
            Initialize();
        }

        private void Initialize()
        {
            ValidateSTPStartInfo();

            // _stpStartInfoRW stores a read/write copy of the STPStartInfo.
            // Actually only MaxWorkerThreads and MinWorkerThreads are overwritten

            _isSuspended = _stpStartInfo.StartSuspended;

            if (_stpStartInfo.EnableLocalPerformanceCounters)
            {
                _localPCs = new LocalSTPInstancePerformanceCounters();
            }

            // If the STP is not started suspended then start the threads.
            if (!_isSuspended)
            {
                StartOptimalNumberOfThreads();
            }
        }

        private void StartOptimalNumberOfThreads()
        {
            int threadsCount = Math.Max(_workItemsQueue.Count, _stpStartInfo.MinWorkerThreads);
            threadsCount = Math.Min(threadsCount, _stpStartInfo.MaxWorkerThreads);
            threadsCount -= _workerThreads.Count;
            if (threadsCount > 0)
            {
                StartThreads(threadsCount);
            }
        }

        private void ValidateSTPStartInfo()
        {
            if (_stpStartInfo.MinWorkerThreads < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "MinWorkerThreads", "MinWorkerThreads cannot be negative");
            }

            if (_stpStartInfo.MaxWorkerThreads <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "MaxWorkerThreads", "MaxWorkerThreads must be greater than zero");
            }

            if (_stpStartInfo.MinWorkerThreads > _stpStartInfo.MaxWorkerThreads)
            {
                throw new ArgumentOutOfRangeException(
                    "MinWorkerThreads, maxWorkerThreads",
                    "MaxWorkerThreads must be greater or equal to MinWorkerThreads");
            }

            if (_stpStartInfo.MaxQueueLength < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "MaxQueueLength",
                    "MaxQueueLength must be >= 0 or null (for unbounded)");
            }
        }

        /// <summary>
        /// Starts new threads
        /// </summary>
        /// <param name="threadsCount">The number of threads to start</param>
        private void StartThreads(int threadsCount)
        {
            if (_isSuspended)
            {
                return;
            }

            lock (_workerThreads.SyncRoot)
            {
                // Don't start threads on shut down
                if (_shutdown)
                {
                    return;
                }

                for (int i = 0; i < threadsCount; ++i)
                {
                    // Don't create more threads then the upper limit
                    if (_workerThreads.Count >= _stpStartInfo.MaxWorkerThreads)
                    {
                        return;
                    }

                    // Create a new thread
                    Thread workerThread = new Thread(ProcessQueuedItems);

                    // Configure the new thread and start it
                    workerThread.IsBackground = _stpStartInfo.AreThreadsBackground;

                    workerThread.Start();
                    ++_threadCounter;

                    // Add it to the dictionary and update its creation time.
                    _workerThreads[workerThread] = new ThreadEntry(this);

                    _windowsPCs.SampleThreads(_workerThreads.Count, _inUseWorkerThreads);
                    _localPCs.SampleThreads(_workerThreads.Count, _inUseWorkerThreads);
                }
            }
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="workItemPriority">The work item priority</param>
        /// <returns>Returns a work item result</returns>
        public new IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, WorkItemPriority workItemPriority)
        {
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, WIGStartInfo, callback, state, workItemPriority);
            Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Returns true if the current running work item has been cancelled.
        /// Must be used within the work item's callback method.
        /// The work item should sample this value in order to know if it
        /// needs to quit before its completion.
        /// </summary>
        public static bool IsWorkItemCanceled
        {
            get
            {
                return CurrentThreadEntry.CurrentWorkItem.IsCanceled;
            }
        }

        /// <summary>
        /// A worker thread method that processes work items from the work items queue.
        /// </summary>
        private void ProcessQueuedItems()
        {
            // Keep the entry of the dictionary as thread's variable to avoid the synchronization locks
            // of the dictionary.
            CurrentThreadEntry = _workerThreads[Thread.CurrentThread];

            FireOnThreadInitialization();

            try
            {
                bool bInUseWorkerThreadsWasIncremented = false;

                // Process until shutdown.
                while (!_shutdown)
                {
                    // Update the last time this thread was seen alive.
                    // It's good for debugging.
                    CurrentThreadEntry.IAmAlive();

                    // The following block handles the when the MaxWorkerThreads has been
                    // incremented by the user at run-time.
                    // Double lock for quit.
                    if (_workerThreads.Count > _stpStartInfo.MaxWorkerThreads)
                    {
                        lock (_workerThreads.SyncRoot)
                        {
                            if (_workerThreads.Count > _stpStartInfo.MaxWorkerThreads)
                            {
                                // Inform that the thread is quiting and then quit.
                                // This method must be called within this lock or else
                                // more threads will quit and the thread pool will go
                                // below the lower limit.
                                InformCompleted();
                                break;
                            }
                        }
                    }

                    // Wait for a work item, shutdown, or timeout
                    WorkItem workItem = Dequeue();

                    // Update the last time this thread was seen alive.
                    // It's good for debugging.
                    CurrentThreadEntry.IAmAlive();

                    // On timeout or shut down.
                    if (null == workItem)
                    {
                        // Double lock for quit.
                        if (_workerThreads.Count > _stpStartInfo.MinWorkerThreads)
                        {
                            lock (_workerThreads.SyncRoot)
                            {
                                if (_workerThreads.Count > _stpStartInfo.MinWorkerThreads)
                                {
                                    // Inform that the thread is quiting and then quit.
                                    // This method must be called within this lock or else
                                    // more threads will quit and the thread pool will go
                                    // below the lower limit.
                                    InformCompleted();
                                    break;
                                }
                            }
                        }
                    }

                    // If we didn't quit then skip to the next iteration.
                    if (null == workItem)
                    {
                        continue;
                    }

                    try
                    {
                        // Initialize the value to false
                        bInUseWorkerThreadsWasIncremented = false;

                        // Set the Current Work Item of the thread.
                        // Store the Current Work Item  before the workItem.StartingWorkItem() is called, 
                        // so WorkItem.Cancel can work when the work item is between InQueue and InProgress 
                        // states.
                        // If the work item has been cancelled BEFORE the workItem.StartingWorkItem() 
                        // (work item is in InQueue state) then workItem.StartingWorkItem() will return false.
                        // If the work item has been cancelled AFTER the workItem.StartingWorkItem() then
                        // (work item is in InProgress state) then the thread will be aborted
                        CurrentThreadEntry.CurrentWorkItem = workItem;

                        // Change the state of the work item to 'in progress' if possible.
                        // We do it here so if the work item has been canceled we won't 
                        // increment the _inUseWorkerThreads.
                        // The cancel mechanism doesn't delete items from the queue,  
                        // it marks the work item as canceled, and when the work item
                        // is dequeued, we just skip it.
                        // If the post execute of work item is set to always or to
                        // call when the work item is canceled then the StartingWorkItem()
                        // will return true, so the post execute can run.
                        if (!workItem.StartingWorkItem())
                        {
                            continue;
                        }

                        // Execute the callback.  Make sure to accurately
                        // record how many callbacks are currently executing.
                        int inUseWorkerThreads = Interlocked.Increment(ref _inUseWorkerThreads);
                        _windowsPCs.SampleThreads(_workerThreads.Count, inUseWorkerThreads);
                        _localPCs.SampleThreads(_workerThreads.Count, inUseWorkerThreads);

                        // Mark that the _inUseWorkerThreads incremented, so in the finally{}
                        // statement we will decrement it correctly.
                        bInUseWorkerThreadsWasIncremented = true;

                        workItem.FireWorkItemStarted();

                        ExecuteWorkItem(workItem);
                    }
                    catch (Exception ex)
                    {
                        ex.GetHashCode();
                        // Do nothing
                    }
                    finally
                    {
                        workItem.DisposeOfState();

                        // Set the CurrentWorkItem to null, since we 
                        // no longer run user's code.
                        CurrentThreadEntry.CurrentWorkItem = null;

                        // Decrement the _inUseWorkerThreads only if we had 
                        // incremented it. Note the cancelled work items don't
                        // increment _inUseWorkerThreads.
                        if (bInUseWorkerThreadsWasIncremented)
                        {
                            int inUseWorkerThreads = Interlocked.Decrement(ref _inUseWorkerThreads);
                            _windowsPCs.SampleThreads(_workerThreads.Count, inUseWorkerThreads);
                            _localPCs.SampleThreads(_workerThreads.Count, inUseWorkerThreads);
                        }

                        // Notify that the work item has been completed.
                        // WorkItemsGroup may enqueue their next work item.
                        workItem.FireWorkItemCompleted();

                        // Decrement the number of work items here so the idle 
                        // ManualResetEvent won't fluctuate.
                        DecrementWorkItemsCount();
                    }
                }
            }
            catch (ThreadAbortException tae)
            {
                tae.GetHashCode();
                // Handle the abort exception gracfully.
                Thread.ResetAbort();
            }
            catch (Exception e)
            {
                Debug.Assert(null != e);
            }
            finally
            {
                InformCompleted();
                FireOnThreadTermination();
            }
        }

        #region Per thread properties

        /// <summary>
        /// A reference to the current work item a thread from the thread pool 
        /// is executing.
        /// </summary>
        internal static ThreadEntry CurrentThreadEntry
        {
            get
            {
                return _threadEntry;
            }
            set
            {
                _threadEntry = value;
            }
        }
        #endregion

        #region Fire Thread's Events

        private void FireOnThreadInitialization()
        {
            if (null != _onThreadInitialization)
            {
                foreach (ThreadInitializationHandler tih in _onThreadInitialization.GetInvocationList())
                {
                    try
                    {
                        tih();
                    }
                    catch (Exception e)
                    {
                        e.GetHashCode();
                        Debug.Assert(false);
                        throw;
                    }
                }
            }
        }

        private void FireOnThreadTermination()
        {
            if (null != _onThreadTermination)
            {
                foreach (ThreadTerminationHandler tth in _onThreadTermination.GetInvocationList())
                {
                    try
                    {
                        tth();
                    }
                    catch (Exception e)
                    {
                        e.GetHashCode();
                        Debug.Assert(false);
                        throw;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Inform that the current thread is about to quit or quiting.
        /// The same thread may call this method more than once.
        /// </summary>
        private void InformCompleted()
        {
            // There is no need to lock the two methods together 
            // since only the current thread removes itself
            // and the _workerThreads is a synchronized dictionary
            if (_workerThreads.Contains(Thread.CurrentThread))
            {
                _workerThreads.Remove(Thread.CurrentThread);
                _windowsPCs.SampleThreads(_workerThreads.Count, _inUseWorkerThreads);
                _localPCs.SampleThreads(_workerThreads.Count, _inUseWorkerThreads);
            }
        }

        /// <summary>
        /// Waits on the queue for a work item, shutdown, or timeout.
        /// </summary>
        /// <returns>
        /// Returns the WaitingCallback or null in case of timeout or shutdown.
        /// </returns>
        private WorkItem Dequeue()
        {
            WorkItem workItem =
                _workItemsQueue.DequeueWorkItem(_stpStartInfo.IdleTimeout, _shuttingDownEvent);

            return workItem;
        }

        /// <summary>
        /// Put a new work item in the queue
        /// </summary>
        /// <param name="workItem">A work item to queue</param>
        internal override void Enqueue(WorkItem workItem)
        {
            // Make sure the workItem is not null
            Debug.Assert(null != workItem);

            IncrementWorkItemsCount();

            workItem.CanceledSmartThreadPool = _canceledSmartThreadPool;
            _workItemsQueue.EnqueueWorkItem(workItem);
            workItem.WorkItemIsQueued();

            // If all the threads are busy then try to create a new one
            if (_currentWorkItemsCount > _workerThreads.Count)
            {
                StartThreads(1);
            }
        }

        private void IncrementWorkItemsCount()
        {
            _windowsPCs.SampleWorkItems(_workItemsQueue.Count, _workItemsProcessed);
            _localPCs.SampleWorkItems(_workItemsQueue.Count, _workItemsProcessed);

            int count = Interlocked.Increment(ref _currentWorkItemsCount);
            //Trace.WriteLine("WorkItemsCount = " + _currentWorkItemsCount.ToString());
            if (count == 1)
            {
                IsIdle = false;
                _isIdleWaitHandle.Reset();
            }
        }

        private void ExecuteWorkItem(WorkItem workItem)
        {
            try
            {
                workItem.Execute();
            }
            catch
            { }
        }

        private void DecrementWorkItemsCount()
        {
            int count = Interlocked.Decrement(ref _currentWorkItemsCount);
            //Trace.WriteLine("WorkItemsCount = " + _currentWorkItemsCount.ToString());
            if (count == 0)
            {
                IsIdle = true;
                _isIdleWaitHandle.Set();
            }

            Interlocked.Increment(ref _workItemsProcessed);

            if (!_shutdown)
            {
                // The counter counts even if the work item was cancelled
                _windowsPCs.SampleWorkItems(_workItemsQueue.Count, _workItemsProcessed);
                _localPCs.SampleWorkItems(_workItemsQueue.Count, _workItemsProcessed);
            }

        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (!_shutdown)
                {
                    Shutdown(true, 0);
                }

                if (null != _shuttingDownEvent)
                {
                    _shuttingDownEvent.Close();
                    _shuttingDownEvent = null;
                }
                _workerThreads.Clear();

                if (null != _isIdleWaitHandle)
                {
                    _isIdleWaitHandle.Close();
                    _isIdleWaitHandle = null;
                }

                _isDisposed = true;
            }
        }

        private void ValidateNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().ToString(), "The SmartThreadPool has been shutdown");
            }
        }
        #endregion

        /// <summary>
        /// WorkItemsGroup start information (readonly)
        /// </summary>
        public override WIGStartInfo WIGStartInfo
        {
            get { return _stpStartInfo.AsReadOnly(); }
        }

        #region Public Methods

        /// <summary>
        /// Empties the queue of work items and abort the threads in the pool.
        /// </summary>
        private void Shutdown(bool forceAbort, int millisecondsTimeout)
        {
            ValidateNotDisposed();

            ISTPInstancePerformanceCounters pcs = _windowsPCs;

            if (NullSTPInstancePerformanceCounters.Instance != _windowsPCs)
            {
                // Set the _pcs to "null" to stop updating the performance
                // counters
                _windowsPCs = NullSTPInstancePerformanceCounters.Instance;

                pcs.Dispose();
            }

            Thread[] threads;
            lock (_workerThreads.SyncRoot)
            {
                // Shutdown the work items queue
                _workItemsQueue.Dispose();

                // Signal the threads to exit
                _shutdown = true;
                _shuttingDownEvent.Set();

                // Make a copy of the threads' references in the pool
                threads = new Thread[_workerThreads.Count];
                _workerThreads.Keys.CopyTo(threads, 0);
            }

            int millisecondsLeft = millisecondsTimeout;
            Stopwatch stopwatch = Stopwatch.StartNew();
            //DateTime start = DateTime.UtcNow;
            bool waitInfinitely = (Timeout.Infinite == millisecondsTimeout);
            bool timeout = false;

            // Each iteration we update the time left for the timeout.
            foreach (Thread thread in threads)
            {
                // Join don't work with negative numbers
                if (!waitInfinitely && (millisecondsLeft < 0))
                {
                    timeout = true;
                    break;
                }

                // Wait for the thread to terminate
                bool success = thread.Join(millisecondsLeft);
                if (!success)
                {
                    timeout = true;
                    break;
                }

                if (!waitInfinitely)
                {
                    // Update the time left to wait
                    //TimeSpan ts = DateTime.UtcNow - start;
                    millisecondsLeft = millisecondsTimeout - (int)stopwatch.ElapsedMilliseconds;
                }
            }

            if (timeout && forceAbort)
            {
                // Abort the threads in the pool
                foreach (Thread thread in threads)
                {

                    if ((thread != null)
                        && thread.IsAlive                      
                        )
                    {
                        try
                        {
                            thread.Abort(); // Shutdown
                        }
                        catch (SecurityException e)
                        {
                            e.GetHashCode();
                        }
                        catch (ThreadStateException ex)
                        {
                            ex.GetHashCode();
                            // In case the thread has been terminated 
                            // after the check if it is alive.
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cancel all work items using thread abortion
        /// </summary>
        /// <param name="abortExecution">True to stop work items by raising ThreadAbortException</param>
        public override void Cancel(bool abortExecution)
        {
            _canceledSmartThreadPool.IsCanceled = true;
            _canceledSmartThreadPool = new CanceledWorkItemsGroup();

            ICollection workItemsGroups = _workItemsGroups.Values;
            foreach (WorkItemsGroup workItemsGroup in workItemsGroups)
            {
                workItemsGroup.Cancel(abortExecution);
            }

            if (abortExecution)
            {
                foreach (ThreadEntry threadEntry in _workerThreads.Values)
                {
                    WorkItem workItem = threadEntry.CurrentWorkItem;
                    if (null != workItem &&
                        threadEntry.AssociatedSmartThreadPool == this &&
                        !workItem.IsCanceled)
                    {
                        threadEntry.CurrentWorkItem.GetWorkItemResult().Cancel(true);
                    }
                }
            }
        }

        /// <summary>
        /// Start the thread pool if it was started suspended.
        /// If it is already running, this method is ignored.
        /// </summary>
        public override void Start()
        {
            if (!_isSuspended)
            {
                return;
            }
            _isSuspended = false;

            ICollection workItemsGroups = _workItemsGroups.Values;
            foreach (WorkItemsGroup workItemsGroup in workItemsGroups)
            {
                workItemsGroup.OnSTPIsStarting();
            }

            StartOptimalNumberOfThreads();
        }

        #endregion

        internal void RegisterWorkItemsGroup(IWorkItemsGroup workItemsGroup)
        {
            _workItemsGroups[workItemsGroup] = workItemsGroup;
        }

        internal void UnregisterWorkItemsGroup(IWorkItemsGroup workItemsGroup)
        {
            if (_workItemsGroups.Contains(workItemsGroup))
            {
                _workItemsGroups.Remove(workItemsGroup);
            }
        }

        public bool IsShuttingdown
        {
            get { return _shutdown; }
        }
    }
}
