using SOPObservationRecord;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOPLandslides
{
    public class SOPLandslides: SOPObservationRecord.SOPObservationRecord
    {
        public int deathTroll { get; set; }
        public int injuryTroll { get; set; }
        public ObservableCollection<string> checkedLandslideCondition { get; set; }
        public bool houseDamage { get; set; }
        public string houseSelected { get; set; }
        public bool farmDamage { get; set; }
        public string farmSelected { get; set; }
        public bool riverDamage { get; set; }
        public string riverSelected { get; set; }
        public bool groundDamage { get; set; }
        public string groundSelected { get; set; }
        public bool roadDamage { get; set; }
        public string roadSelected { get; set; }
    }
}
