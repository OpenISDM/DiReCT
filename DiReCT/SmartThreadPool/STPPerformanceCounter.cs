using System;

namespace Amib.Threading
{
    public interface ISTPPerformanceCountersReader
    {
        long InUseThreads { get; }
        long ActiveThreads { get; }
        long WorkItemsQueued { get; }
        long WorkItemsProcessed { get; }
    }
}

namespace Amib.Threading.Internal
{
    internal interface ISTPInstancePerformanceCounters : IDisposable
    {
        void Close();
        void SampleThreads(long activeThreads, long inUseThreads);
        void SampleWorkItems(long workItemsQueued, long workItemsProcessed);
    }

    internal class NullSTPInstancePerformanceCounters : ISTPInstancePerformanceCounters, ISTPPerformanceCountersReader
    {
        private static readonly NullSTPInstancePerformanceCounters _instance = new NullSTPInstancePerformanceCounters();

        public static NullSTPInstancePerformanceCounters Instance
        {
            get { return _instance; }
        }

        public void Close() { }
        public void Dispose() { }

        public void SampleThreads(long activeThreads, long inUseThreads) { }
        public void SampleWorkItems(long workItemsQueued, long workItemsProcessed) { }
        public long InUseThreads
        {
            get { return 0; }
        }

        public long ActiveThreads
        {
            get { return 0; }
        }

        public long WorkItemsQueued
        {
            get { return 0; }
        }

        public long WorkItemsProcessed
        {
            get { return 0; }
        }
    }

    internal class LocalSTPInstancePerformanceCounters : ISTPInstancePerformanceCounters, ISTPPerformanceCountersReader
    {
        public void Close() { }
        public void Dispose() { }

        private long _activeThreads;
        private long _inUseThreads;
        private long _workItemsQueued;
        private long _workItemsProcessed;

        public long InUseThreads
        {
            get { return _inUseThreads; }
        }

        public long ActiveThreads
        {
            get { return _activeThreads; }
        }

        public long WorkItemsQueued
        {
            get { return _workItemsQueued; }
        }

        public long WorkItemsProcessed
        {
            get { return _workItemsProcessed; }
        }

        public void SampleThreads(long activeThreads, long inUseThreads)
        {
            _activeThreads = activeThreads;
            _inUseThreads = inUseThreads;
        }

        public void SampleWorkItems(long workItemsQueued, long workItemsProcessed)
        {
            _workItemsQueued = workItemsQueued;
            _workItemsProcessed = workItemsProcessed;
        }
    }
}
