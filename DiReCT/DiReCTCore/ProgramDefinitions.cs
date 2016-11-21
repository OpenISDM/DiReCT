using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT
{
    public enum ThreadRequiredModule
    {
        AAA = 0,
        DM,
        DS,
        MAN,
        RTQC,
        NumberOfModules
    };

    public enum TimeInterval
    {
        VeryShortTime = 1000,   // 1000 milliseconds, 1 second
        ShortTime = 10000,      // 10000 milliseconds, 10 seconds
        LongTime = 60000,       // 60000 milliseconds, 1 minute
        VeryLongTime = 300000   // 300000 milliseconds, 5 minutes
    };

    public enum EventIndex
    {
        StartWorkEvent = 0,
        TerminateWorkEvent,        
        NumberOfWorkEvents
    };
}
