using System;

namespace SOPObservationRecord
{
    /// <summary>
    /// This class contains the abstract record. This record should be inherited
    /// by other specific record type, such as Flood or Landslides, and should
    /// not be used alone.
    /// </summary>
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
