using SOPObservationRecord;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOPFlood
{
    public class SOPFlood: SOPObservationRecord.SOPObservationRecord
    {
        public int waterLevel { get; set; }

        public ObservableCollection<string> PossibleCauseOfDisaster { get; set; }
    }
}
