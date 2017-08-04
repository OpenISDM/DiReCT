using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOPObservationRecord
{
    abstract public class SOPObservationRecord
    {
        public int RecordID
        {
            get; set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SOPObservationRecord()
        {           
            RecordID = (int)(DateTime.Now.Ticks % 100000);
        }

        public string currentLongitude { get; set; }
        public string currentLatitude { get; set; }
        public string currentTimeStamp { get; set; }
    }
}
