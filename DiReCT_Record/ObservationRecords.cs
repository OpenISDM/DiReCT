using System;
using System.Collections.Generic;
using System.Device.Location;


namespace DiReCT_Record
{
    /// <summary>
    /// Enumerate record type(Field Visits、Medical...)
    /// </summary>
    public enum RecordTypeEnum
    {
        Field_Visits = 0,
        Medical,
        Facility_Damage
    }

    /// <summary>
    /// The items shared by all records
    /// </summary>
    public abstract class ObservationRecord
    {
        //Record Id
        public Guid RecordId { get; set; }
        //Disaster Id, used to associate other data tables
        public Guid DisasterId { get; set; }
        //Recorder Id
        public Guid RecorderId { get; set; }
        //Record Type(Field Visits、Medical...)
        public int RecordType { get; set; }
        //Disaster Type, used for data classification
        public int DisasterType { get; set; }
        //Record Unit
        public string RecordUnit { get; set; }
        //Notes On Disaster
        public string NotesOnDisaster { get; set; }
        //Record Time
        public DateTime RecordTime { get; set; }
        //Multimedia File Paths
        public Dictionary<string, string> MultiMediaFilePaths { get; set; }

        //When a new record is generated, the initial value is given
        public ObservationRecord()
        {
            if (RecordId != null)
            {
                RecordId = Guid.NewGuid();
                RecordTime = DateTime.Now;
            }
        }
    }

    //Record Of Field Visits
    public abstract class RecordOfFieldVisits: ObservationRecord
    {
        //Record Location
        public string RecordLocation { get; set; }
        //Record Coordinate
        public GeoCoordinate RecordCoordinate { get; set; }

        //When a new record is generated, the initial value is given
        public RecordOfFieldVisits()
        {
            RecordType = (int)RecordTypeEnum.Field_Visits;
        }
    }

    //Record Of Medical
    public abstract class RecordOfMedical: ObservationRecord
    {
        //Death Toll
        public int DeathToll { get; set; }
        //Injury Toll
        public int InjuryToll { get; set; }
        //The number of missing
        public int NumberOfMissing { get; set; }
        //The number of evacuee
        public int NumberOfEvacuee { get; set; }

        //When a new record is generated, the initial value is given
        public RecordOfMedical()
        {
            RecordType = (int)RecordTypeEnum.Medical;
        }
    }

    //Record Of Facility Damage
    public abstract class RecordOfFacilityDamage: ObservationRecord
    {
        //Record Location
        public string RecordLocation { get; set; }
        //Record Coordinate
        public GeoCoordinate RecordCoordinate { get; set; }

        //The number of public utilities
        public int NumberOfPublicUtilities { get; set; }
        //The number of trees collapsed
        public int NumberOfTreeCollapsed { get; set; }

        //When a new record is generated, the initial value is given
        public RecordOfFacilityDamage()
        {
            RecordType = (int)RecordTypeEnum.Facility_Damage;
        }
    }
}
