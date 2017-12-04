using System;
using System.Threading;
using System.Diagnostics;

namespace Amib.Threading.Internal
{
    public partial class WorkItem : IHasWorkItemPriority
    {
        /// <summary>
        /// Indicates the state of the work item in the thread pool
        /// </summary>
        private enum WorkItemState
        {
            InQueue = 0,    // Nexts: InProgress, Canceled
            InProgress = 1,    // Nexts: Completed, Canceled
            Completed = 2,    // Stays Completed
            Canceled = 3,    // Stays Canceled
        }

        private static bool IsValidStatesTransition(WorkItemState currentState, WorkItemState nextState)
        {
            bool valid = false;

            switch (currentState)
            {
                case WorkItemState.InQueue:
                    valid = (WorkItemState.InProgress == nextState) || (WorkItemState.Canceled == nextState);
                    break;
                case WorkItemState.InProgress:
                    valid = (WorkItemState.Completed == nextState) || (WorkItemState.Canceled == nextState);
                    break;
                case WorkItemState.Completed:
                case WorkItemState.Canceled:
                    // Cannot be changed
                    break;
                default:
                    // Unknown state
                    Debug.Assert(false);
                    break;
            }

            return valid;
        }

        /// <summary>
        /// Work item info
        /// </summary>
        private readonly WorkItemInfo _workItemInfo;

        /// <summary>
        /// Represents the result state of the work item
        /// </summary>
        private readonly WorkItemResult _workItemResult;

        /// <summary>
        /// A reference to an object that indicates whatever the 
        /// WorkItemsGroup has been canceled
        /// </summary>
        private CanceledWorkItemsGroup _canceledWorkItemsGroup = CanceledWorkItemsGroup.NotCanceledWorkItemsGroup;

        /// <summary>
        /// A reference to an object that indicates whatever the 
        /// SmartThreadPool has been canceled
        /// </summary>
        private CanceledWorkItemsGroup _canceledSmartThreadPool = CanceledWorkItemsGroup.NotCanceledWorkItemsGroup;

        /// <summary>
        /// The work item group this work item belong to.
        /// </summary>
        private readonly IWorkItemsGroup _workItemsGroup;

        /// <summary>
        /// Stores how long the work item waited on the stp queue
        /// </summary>
        private Stopwatch _waitingOnQueueStopwatch;

        /// <summary>
        /// Stores how much time it took the work item to execute after it went out of the queue
        /// </summary>
        private Stopwatch _processingStopwatch;

        /// <summary>
        /// Called when the WorkItem starts
        /// </summary>
        private event WorkItemStateCallback _workItemStartedEvent;

        /// <summary>
        /// Called when the WorkItem completes
        /// </summary>
        private event WorkItemStateCallback _workItemCompletedEvent;

        /// <summary>
        /// A reference count to the _workItemCompleted. 
        /// When it reaches to zero _workItemCompleted is Closed
        /// </summary>
        private int _workItemCompletedRefCount;

        /// <summary>
        /// Hold the state of the work item
        /// </summary>
        private WorkItemState _workItemState;

        /// <summary>
        /// State with which to call the callback delegate.
        /// </summary>
        private object _state;

        /// <summary>
        /// The thread that executes this workitem.
        /// This field is available for the period when the work item is executed, before and after it is null.
        /// </summary>
        private Thread _executingThread;

        /// <summary>
        /// The absulote time when the work item will be timeout
        /// </summary>
        private long _expirationTime;

        /// <summary>
        /// Stores the caller's context
        /// </summary>
        private readonly CallerThreadContext _callerContext;

        /// <summary>
        /// Callback delegate for the callback.
        /// </summary>
        private readonly WorkItemCallback _callback;

        /// <summary>
        /// Holds the result of the mehtod
        /// </summary>
        private object _result;

        /// <summary>
        /// Hold the exception if the method threw it
        /// </summary>
        private Exception _exception;

        /// <summary>
        /// A ManualResetEvent to indicate that the result is ready
        /// </summary>
        private ManualResetEvent _workItemCompleted;

        /// <summary>
        /// Initialize the callback holding object.
        /// </summary>
        /// <param name="workItemsGroup">The workItemGroup of the workitem</param>
        /// <param name="workItemInfo">The WorkItemInfo of te workitem</param>
        /// <param name="callback">Callback delegate for the callback.</param>
        /// <param name="state">State with which to call the callback delegate.</param>
        /// 
        /// We assume that the WorkItem object is created within the thread
        /// that meant to run the callback
        public WorkItem(
            IWorkItemsGroup workItemsGroup,
            WorkItemInfo workItemInfo,
            WorkItemCallback callback,
            object state)
        {
            _workItemsGroup = workItemsGroup;
            _workItemInfo = workItemInfo;

            if (_workItemInfo.UseCallerCallContext)
            {
                _callerContext = CallerThreadContext.Capture(_workItemInfo.UseCallerCallContext);
            }

            _callback = callback;
            _state = state;
            _workItemResult = new WorkItemResult(this);
            Initialize();
        }

        internal void Initialize()
        {
            // The _workItemState is changed directly instead of using the SetWorkItemState
            // method since we don't want to go throught IsValidStateTransition.
            _workItemState = WorkItemState.InQueue;

            _workItemCompleted = null;
            _workItemCompletedRefCount = 0;
            _waitingOnQueueStopwatch = new Stopwatch();
            _processingStopwatch = new Stopwatch();
            _expirationTime =
                _workItemInfo.Timeout > 0 ?
                DateTime.UtcNow.Ticks + _workItemInfo.Timeout * TimeSpan.TicksPerMillisecond :
                long.MaxValue;
        }

        #region Members exposed by WorkItemResult

        /// <summary>
        /// Cancel the work item if it didn't start running yet.
        /// </summary>
        /// <returns>Returns true on success or false if the work item is in progress or already completed</returns>
        private bool Cancel(bool abortExecution)
        {
            bool success = false;
            bool signalComplete = false;

            lock (this)
            {
                switch (GetWorkItemState())
                {
                    case WorkItemState.Canceled:
                        //Debug.WriteLine("Work item already canceled");
                        if (abortExecution)
                        {
                            Thread executionThread = Interlocked.CompareExchange(ref _executingThread, null, _executingThread);
                            if (null != executionThread)
                            {
                                executionThread.Abort(); // "Cancel"
                                // No need to signalComplete, because we already cancelled this work item
                                // so it already signaled its completion.
                                //signalComplete = true;
                            }
                        }
                        success = true;
                        break;
                    case WorkItemState.Completed:
                        //Debug.WriteLine("Work item cannot be canceled");
                        break;
                    case WorkItemState.InProgress:
                        if (abortExecution)
                        {
                            Thread executionThread = Interlocked.CompareExchange(ref _executingThread, null, _executingThread);
                            if (null != executionThread)
                            {
                                executionThread.Abort(); // "Cancel"
                                success = true;
                                signalComplete = true;
                            }
                        }
                        else
                        {
                            success = true;
                            signalComplete = true;
                        }
                        break;
                    case WorkItemState.InQueue:
                        // Signal to the wait for completion that the work
                        // item has been completed (canceled). There is no
                        // reason to wait for it to get out of the queue
                        signalComplete = true;
                        //Debug.WriteLine("Work item canceled");
                        success = true;
                        break;
                }

                if (signalComplete)
                {
                    SignalComplete(true);
                }
            }
            return success;
        }

        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits for the result, timeout, or cancel.
        /// In case of error the method throws and exception
        /// </summary>
        /// <returns>The result of the work item</returns>
        private object GetResult(
            int millisecondsTimeout,
            bool exitContext,
            WaitHandle cancelWaitHandle)
        {
            Exception e;
            object result = GetResult(millisecondsTimeout, exitContext, cancelWaitHandle, out e);
            if (null != e)
            {
                throw new WorkItemResultException("The work item caused an excpetion, see the inner exception for details", e);
            }
            return result;
        }

        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits for the result, timeout, or cancel.
        /// In case of error the e argument is filled with the exception
        /// </summary>
        /// <returns>The result of the work item</returns>
        private object GetResult(
            int millisecondsTimeout,
            bool exitContext,
            WaitHandle cancelWaitHandle,
            out Exception e)
        {
            e = null;

            // Check for cancel
            if (WorkItemState.Canceled == GetWorkItemState())
            {
                throw new WorkItemCancelException("Work item canceled");
            }

            // Check for completion
            if (IsCompleted)
            {
                e = _exception;
                return _result;
            }

            // If no cancelWaitHandle is provided
            if (null == cancelWaitHandle)
            {
                WaitHandle wh = GetWaitHandle();

                bool timeout = !STPEventWaitHandle.WaitOne(wh, millisecondsTimeout, exitContext);

                ReleaseWaitHandle();

                if (timeout)
                {
                    throw new WorkItemTimeoutException("Work item timeout");
                }
            }
            else
            {
                WaitHandle wh = GetWaitHandle();
                int result = STPEventWaitHandle.WaitAny(new WaitHandle[] { wh, cancelWaitHandle });
                ReleaseWaitHandle();

                switch (result)
                {
                    case 0:
                        // The work item signaled
                        // Note that the signal could be also as a result of canceling the 
                        // work item (not the get result)
                        break;
                    case 1:
                    case STPEventWaitHandle.WaitTimeout:
                        throw new WorkItemTimeoutException("Work item timeout");
                    default:
                        Debug.Assert(false);
                        break;

                }
            }

            // Check for cancel
            if (WorkItemState.Canceled == GetWorkItemState())
            {
                throw new WorkItemCancelException("Work item canceled");
            }

            Debug.Assert(IsCompleted);

            e = _exception;

            // Return the result
            return _result;
        }

        /// <summary>
        /// A wait handle to wait for completion, cancel, or timeout 
        /// </summary>
        private WaitHandle GetWaitHandle()
        {
            lock (this)
            {
                if (null == _workItemCompleted)
                {
                    _workItemCompleted = EventWaitHandleFactory.CreateManualResetEvent(IsCompleted);
                }
                ++_workItemCompletedRefCount;
            }
            return _workItemCompleted;
        }

        private void ReleaseWaitHandle()
        {
            lock (this)
            {
                if (null != _workItemCompleted)
                {
                    --_workItemCompletedRefCount;
                    if (0 == _workItemCompletedRefCount)
                    {
                        _workItemCompleted.Close();
                        _workItemCompleted = null;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true when the work item has completed or canceled
        /// </summary>
        private bool IsCompleted
        {
            get
            {
                lock (this)
                {
                    WorkItemState workItemState = GetWorkItemState();
                    return ((workItemState == WorkItemState.Completed) ||
                            (workItemState == WorkItemState.Canceled));
                }
            }
        }

        /// <summary>
        /// Returns true when the work item has canceled
        /// </summary>
        public bool IsCanceled
        {
            get
            {
                lock (this)
                {
                    return (GetWorkItemState() == WorkItemState.Canceled);
                }
            }
        }

        #endregion

        #region IHasWorkItemPriority Members

        /// <summary>
        /// Returns the priority of the work item
        /// </summary>
        public WorkItemPriority WorkItemPriority
        {
            get
            {
                return _workItemInfo.WorkItemPriority;
            }
        }

        #endregion

        /// <summary>
        /// Change the state of the work item to in progress if it wasn't canceled.
        /// </summary>
        /// <returns>
        /// Return true on success or false in case the work item was canceled.
        /// If the work item needs to run a post execute then the method will return true.
        /// </returns>
        public bool StartingWorkItem()
        {
            _waitingOnQueueStopwatch.Stop();
            _processingStopwatch.Start();

            lock (this)
            {
                if (IsCanceled)
                {
                    bool result = false;
                    if ((_workItemInfo.PostExecuteWorkItemCallback != null) &&
                        ((_workItemInfo.CallToPostExecute & CallToPostExecute.WhenWorkItemCanceled) == CallToPostExecute.WhenWorkItemCanceled))
                    {
                        result = true;
                    }

                    return result;
                }

                Debug.Assert(WorkItemState.InQueue == GetWorkItemState());

                // No need for a lock yet, only after the state has changed to InProgress
                _executingThread = Thread.CurrentThread;

                SetWorkItemState(WorkItemState.InProgress);
            }

            return true;
        }


        private WorkItemState GetWorkItemState()
        {
            lock (this)
            {
                if (WorkItemState.Completed == _workItemState)
                {
                    return _workItemState;
                }

                long nowTicks = DateTime.UtcNow.Ticks;

                if (WorkItemState.Canceled != _workItemState && nowTicks > _expirationTime)
                {
                    _workItemState = WorkItemState.Canceled;
                }

                if (WorkItemState.InProgress == _workItemState)
                {
                    return _workItemState;
                }

                if (CanceledSmartThreadPool.IsCanceled || CanceledWorkItemsGroup.IsCanceled)
                {
                    return WorkItemState.Canceled;
                }

                return _workItemState;
            }
        }

        internal void FireWorkItemStarted()
        {
            try
            {
                if (null != _workItemStartedEvent)
                {
                    _workItemStartedEvent(this);
                }
            }
            catch // Suppress exceptions
            { }
        }

        /// <summary>
        /// Set the result of the work item to return
        /// </summary>
        /// <param name="result">The result of the work item</param>
        /// <param name="exception">The exception that was throw while the workitem executed, null
        /// if there was no exception.</param>
        internal void SetResult(object result, Exception exception)
        {
            _result = result;
            _exception = exception;
            SignalComplete(false);
        }

        internal void FireWorkItemCompleted()
        {
            try
            {
                if (null != _workItemCompletedEvent)
                {
                    _workItemCompletedEvent(this);
                }
            }
            catch // Suppress exceptions
            { }
        }

        /// <summary>
        /// Sets the work item's state
        /// </summary>
        /// <param name="workItemState">The state to set the work item to</param>
        private void SetWorkItemState(WorkItemState workItemState)
        {
            lock (this)
            {
                if (IsValidStatesTransition(_workItemState, workItemState))
                {
                    _workItemState = workItemState;
                }
            }
        }

        /// <summary>
        /// Execute the work item and the post execute
        /// </summary>
        public void Execute()
        {
            CallToPostExecute currentCallToPostExecute = 0;

            // Execute the work item if we are in the correct state
            switch (GetWorkItemState())
            {
                case WorkItemState.InProgress:
                    currentCallToPostExecute |= CallToPostExecute.WhenWorkItemNotCanceled;
                    ExecuteWorkItem();
                    break;
                case WorkItemState.Canceled:
                    currentCallToPostExecute |= CallToPostExecute.WhenWorkItemCanceled;
                    break;
                default:
                    Debug.Assert(false);
                    throw new NotSupportedException();
            }

            // Run the post execute as needed
            if ((currentCallToPostExecute & _workItemInfo.CallToPostExecute) != 0)
            {
                PostExecute();
            }

            _processingStopwatch.Stop();
        }

        /// <summary>
        /// Execute the work item
        /// </summary>
        private void ExecuteWorkItem()
        {
            CallerThreadContext ctc = null;
            if (null != _callerContext)
            {
                ctc = CallerThreadContext.Capture(_callerContext.CapturedCallContext);
                CallerThreadContext.Apply(_callerContext);
            }

            Exception exception = null;
            object result = null;

            try
            {
                try
                {
                    result = _callback(_state);
                }
                catch (Exception e)
                {
                    // Save the exception so we can rethrow it later
                    exception = e;
                }

                // Remove the value of the execution thread, so it will be impossible to cancel the work item,
                // since it is already completed.
                // Cancelling a work item that already completed may cause the abortion of the next work item!!!
                Thread executionThread = Interlocked.CompareExchange(ref _executingThread, null, _executingThread);

                if (null == executionThread)
                {
                    // Oops! we are going to be aborted..., Wait here so we can catch the ThreadAbortException
                    Thread.Sleep(60 * 1000);

                    // If after 1 minute this thread was not aborted then let it continue working.
                }
            }
            // We must treat the ThreadAbortException or else it will be stored in the exception variable
            catch (ThreadAbortException tae)
            {
                tae.GetHashCode();
                // Check if the work item was cancelled
                // If we got a ThreadAbortException and the STP is not shutting down, it means the 
                // work items was cancelled.
                if (!SmartThreadPool.CurrentThreadEntry.AssociatedSmartThreadPool.IsShuttingdown)
                {
                    Thread.ResetAbort();
                }
            }

            if (null != _callerContext)
            {
                CallerThreadContext.Apply(ctc);
            }

            if (!SmartThreadPool.IsWorkItemCanceled)
            {
                SetResult(result, exception);
            }
        }

        internal void WorkItemIsQueued()
        {
            _waitingOnQueueStopwatch.Start();
        }

        /// <summary>
        /// Signals that work item has been completed or canceled
        /// </summary>
        /// <param name="canceled">Indicates that the work item has been canceled</param>
        private void SignalComplete(bool canceled)
        {
            SetWorkItemState(canceled ? WorkItemState.Canceled : WorkItemState.Completed);
            lock (this)
            {
                // If someone is waiting then signal.
                if (null != _workItemCompleted)
                {
                    _workItemCompleted.Set();
                }
            }
        }

        /// <summary>
        /// Runs the post execute callback
        /// </summary>
        private void PostExecute()
        {
            if (null != _workItemInfo.PostExecuteWorkItemCallback)
            {
                try
                {
                    _workItemInfo.PostExecuteWorkItemCallback(_workItemResult);
                }
                catch (Exception e)
                {
                    Debug.Assert(null != e);
                }
            }
        }

        public TimeSpan WaitingTime
        {
            get
            {
                return _waitingOnQueueStopwatch.Elapsed;
            }
        }

        public TimeSpan ProcessTime
        {
            get
            {
                return _processingStopwatch.Elapsed;
            }
        }

        internal CanceledWorkItemsGroup CanceledWorkItemsGroup
        {
            get { return _canceledWorkItemsGroup; }
            set { _canceledWorkItemsGroup = value; }
        }

        internal CanceledWorkItemsGroup CanceledSmartThreadPool
        {
            get { return _canceledSmartThreadPool; }
            set { _canceledSmartThreadPool = value; }
        }

        internal IWorkItemResult GetWorkItemResult()
        {
            return _workItemResult;
        }

        internal event WorkItemStateCallback OnWorkItemStarted
        {
            add
            {
                _workItemStartedEvent += value;
            }
            remove
            {
                _workItemStartedEvent -= value;
            }
        }

        internal event WorkItemStateCallback OnWorkItemCompleted
        {
            add
            {
                _workItemCompletedEvent += value;
            }
            remove
            {
                _workItemCompletedEvent -= value;
            }
        }

        public void DisposeOfState()
        {
            if (_workItemInfo.DisposeOfStateObjects)
            {
                IDisposable disp = _state as IDisposable;
                if (null != disp)
                {
                    disp.Dispose();
                    _state = null;
                }
            }
        }
    }
}
