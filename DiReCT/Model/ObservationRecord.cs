/*
* Copyright (c) 2016 Academia Sinica, Institude of Information Science
*
* License:
*      GPL 3.0 : The content of this file is subject to the terms and 
*      conditions defined in file 'COPYING.txt', which is part of this 
*      source code package.
*
* Project Name:
* 
* 		DiReCT(Disaster Record Capture Tool)
* 
* File Description:
* File Name:
* 
* 		ObservationRecord.cs
* 
* Abstract:
*      
*      This abstract class contains basic observation record properties
*      for all types of disaster records capture via DiReCT.
*
* Authors:
* 
*      Hunter Hsieh, hunter205@iis.sinica.edu.tw  
*      Jeff Chen, jeff@iis.sinica.edu.tw
* 
*/
using System;
using System.Collections.Generic;
using System.Device.Location;

namespace DiReCT.Model.Observations
{
    public abstract class ObservationRecord
    {
        //flood + constructor, set 多的property.
        //or的放constructor + flood新的 
        //堤共 個別method set property


        /// <summary>
        /// Constructor.
        /// </summary>
        public ObservationRecord()
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
        public long EstimatedOccurrenceTime { get; protected set; }


        /// <summary>
        /// This member stores the time when the data in this record are 
        /// observed.
        /// 
        /// e.g. The time of recorder observes or measures water depth and 
        /// captures the value.
        /// </summary>
        public long ObservationRecordCapturedTime { get; protected set; }


        /// <summary>
        /// The time stamp of the record set by DiReCT when the record
        /// is saved.
        /// 
        /// e.g. The time of recorder saves record.
        /// </summary>
        public long ObservationRecordSavedTime { get; protected set; }

        
        /// <summary>
        /// This member stores the coordinates of the location where
        /// the recorder is when the observation record is captured.
        /// </summary>
        public GeoCoordinate RecorderRecordLocation { get; protected set; }


        /// <summary>
        /// This member stores the coordinates of the location with 
        /// the observation data contained in this observation data 
        /// record.
        /// 
        /// e.g. The landslide occurred location.  
        /// </summary>
        public GeoCoordinate EstimatedOccurrenceLocation
        { get; protected set; }
        

        /// <summary>
        /// This string stores the notes or others information.
        /// </summary>
        public string NotesOnRecord { get; protected set; }


        /// <summary>
        /// This list stores paths to video, audio and photo
        /// associated with this observation record. 
        /// </summary>
        public List<string> MultiMediaFilePaths { get; protected set; }
        #endregion
    }
}
