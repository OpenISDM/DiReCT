using System;

namespace Amib.Threading
{
    /// <summary>
    /// Summary description for WIGStartInfo.
    /// </summary>
    public class WIGStartInfo
    {
        private bool _useCallerCallContext;
        private bool _disposeOfStateObjects;
        private CallToPostExecute _callToPostExecute;
        private PostExecuteWorkItemCallback _postExecuteWorkItemCallback;
        private bool _startSuspended;
        private WorkItemPriority _workItemPriority;
        private bool _fillStateWithArgs;

        protected bool _readOnly;

        public WIGStartInfo()
        {
            _fillStateWithArgs = SmartThreadPool.DefaultFillStateWithArgs;
            _workItemPriority = SmartThreadPool.DefaultWorkItemPriority;
            CallToPostExecute = SmartThreadPool.DefaultCallToPostExecute;
            _disposeOfStateObjects = SmartThreadPool.DefaultDisposeOfStateObjects;
            _useCallerCallContext = SmartThreadPool.DefaultUseCallerCallContext;
        }

        public WIGStartInfo(WIGStartInfo wigStartInfo)
        {
            _useCallerCallContext = wigStartInfo.UseCallerCallContext;
            _disposeOfStateObjects = wigStartInfo.DisposeOfStateObjects;
            _callToPostExecute = wigStartInfo.CallToPostExecute;
            _postExecuteWorkItemCallback = wigStartInfo.PostExecuteWorkItemCallback;
            _workItemPriority = wigStartInfo.WorkItemPriority;
            _startSuspended = wigStartInfo.StartSuspended;
            _fillStateWithArgs = wigStartInfo.FillStateWithArgs;
        }

        protected void ThrowIfReadOnly()
        {
            if (_readOnly)
            {
                throw new NotSupportedException("This is a readonly instance and set is not supported");
            }
        }

        /// <summary>
        /// Get/Set if to use the caller's security context
        /// </summary>
        public virtual bool UseCallerCallContext
        {
            get { return _useCallerCallContext; }
            set
            {
                ThrowIfReadOnly();
                _useCallerCallContext = value;
            }
        }


        /// <summary>
        /// Get/Set if to dispose of the state object of a work item
        /// </summary>
        public virtual bool DisposeOfStateObjects
        {
            get { return _disposeOfStateObjects; }
            set
            {
                ThrowIfReadOnly();
                _disposeOfStateObjects = value;
            }
        }


        /// <summary>
        /// Get/Set the run the post execute options
        /// </summary>
        public virtual CallToPostExecute CallToPostExecute
        {
            get { return _callToPostExecute; }
            set
            {
                ThrowIfReadOnly();
                _callToPostExecute = value;
            }
        }


        /// <summary>
        /// Get/Set the default post execute callback
        /// </summary>
        public virtual PostExecuteWorkItemCallback PostExecuteWorkItemCallback
        {
            get { return _postExecuteWorkItemCallback; }
            set
            {
                ThrowIfReadOnly();
                _postExecuteWorkItemCallback = value;
            }
        }


        /// <summary>
        /// Get/Set if the work items execution should be suspended until the Start()
        /// method is called.
        /// </summary>
        public virtual bool StartSuspended
        {
            get { return _startSuspended; }
            set
            {
                ThrowIfReadOnly();
                _startSuspended = value;
            }
        }


        /// <summary>
        /// Get/Set the default priority that a work item gets when it is enqueued
        /// </summary>
        public virtual WorkItemPriority WorkItemPriority
        {
            get { return _workItemPriority; }
            set { _workItemPriority = value; }
        }

        /// <summary>
        /// Get/Set the if QueueWorkItem of Action&lt;...&gt;/Func&lt;...&gt; fill the
        /// arguments as an object array into the state of the work item.
        /// The arguments can be access later by IWorkItemResult.State.
        /// </summary>
        public virtual bool FillStateWithArgs
        {
            get { return _fillStateWithArgs; }
            set
            {
                ThrowIfReadOnly();
                _fillStateWithArgs = value;
            }
        }

    }
}