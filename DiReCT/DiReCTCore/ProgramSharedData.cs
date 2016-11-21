using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DiReCT
{
    class ThreadParameters
    {
        private AutoResetEvent readyToWorkEvent;
        private AutoResetEvent startWorkEvent;
        private AutoResetEvent terminateWorkEvent;

        public ThreadParameters()
        {
            readyToWorkEvent = new AutoResetEvent(false);
            startWorkEvent = new AutoResetEvent(false);
            terminateWorkEvent = new AutoResetEvent(false);
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

        public AutoResetEvent TerminateWorkEvent
        {
            get
            {
                return terminateWorkEvent;
            }
        }

        #endregion
    }
    class ModuleControlData
    {
        private ThreadParameters threadParameters;
        private int modulePriorityIncrement;

        public ModuleControlData()
        {
            threadParameters = new ThreadParameters();
            modulePriorityIncrement = 0;
        }

        #region Properties

        public ThreadParameters ThreadParameters
        {
            get
            {
                return threadParameters;
            }
        }

        public int ModulePriorityIncrement
        {
            get
            {
                return modulePriorityIncrement;
            }

            set
            {
                modulePriorityIncrement = value;
            }
        }

        #endregion
    }
}
