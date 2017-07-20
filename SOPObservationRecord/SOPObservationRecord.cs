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
        /// Constructor.
        /// </summary>
        public SOPObservationRecord()
        {
            RecorderRecordLocation = new GeoCoordinate();
            EstimatedOccurrenceLocation = new GeoCoordinate();
            MultiMediaFilePaths = new List<string>();
        }

        /// <summary>
        /// This method provides the caller with the means to set observation 
        /// record and record information.    
        /// </summary>
        public abstract void SetObservationRecord();

        #region Properties
        /// <summary>
        /// This member stores the estimated time of the disaster.
        /// 
        /// e.g. Time entered by recorder manually or with tool's help.
        /// </summary>
        public long EstimatedOccurrenceTime { get; set; }


        /// <summary>
        /// This member stores the time when the data in this record are 
        /// observed.
        /// 
        /// e.g. The time of recorder observes or measures water depth and 
        /// captures the value.
        /// </summary>
        public long ObservationRecordCapturedTime { get; set; }


        /// <summary>
        /// The time stamp of the record set by DiReCT when the record
        /// is saved.
        /// 
        /// e.g. The time of recorder saves record.
        /// </summary>
        public long ObservationRecordSavedTime { get; set; }


        /// <summary>
        /// This member stores the coordinates of the location where
        /// the recorder is when the observation record is captured.
        /// </summary>
        public GeoCoordinate RecorderRecordLocation { get; set; }


        /// <summary>
        /// This member stores the coordinates of the location with 
        /// the observation data contained in this observation data 
        /// record.
        /// 
        /// e.g. The landslide occurred location.  
        /// </summary>
        public GeoCoordinate EstimatedOccurrenceLocation
        { get; set; }


        /// <summary>
        /// This string stores the notes or others information.
        /// </summary>
        public string NotesOnRecord { get; set; }


        /// <summary>
        /// This list stores paths to video, audio and photo
        /// associated with this observation record. 
        /// </summary>
        public List<string> MultiMediaFilePaths { get; set; }
        #endregion
    }
}
