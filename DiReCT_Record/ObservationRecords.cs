using System;
using System.Collections.Generic;
using System.Device.Location;


namespace DiReCT_Record
{
    public enum RecordTypeEnum
    {
        Field_Visits = 0,
        Medical,
        Facility_Damage
    }

    public abstract class ObservationRecord
    {
        public Guid RecordId { get; set; }
        public Guid DisasterId { get; set; }
        public Guid RecorderId { get; set; }
        public int RecordType { get; set; }
        public int DisasterType { get; set; }
        public string RecordUnit { get; set; }
        public string NotesOnDisaster { get; set; }
        public DateTime RecordTime { get; set; }
        public Dictionary<string, string> MultiMediaFilePaths { get; set; }

        public ObservationRecord()
        {
            if (RecordId != null)
            {
                RecordId = Guid.NewGuid();
                RecordTime = DateTime.Now;
            }
        }
    }

    public abstract class RecordOfFieldVisits: ObservationRecord
    {
        public string RecordLocation { get; set; }
        public GeoCoordinate RecordCoordinate { get; set; }

        public RecordOfFieldVisits()
        {
            RecordType = (int)RecordTypeEnum.Field_Visits;
        }
    }

    public abstract class RecordOfMedical: ObservationRecord
    {
        public int DeathToll { get; set; }
        public int InjuryToll { get; set; }
        public int NumberOfMissing { get; set; }
        public int NumberOfEvacuee { get; set; }

        public RecordOfMedical()
        {
            RecordType = (int)RecordTypeEnum.Medical;
        }
    }

    public abstract class RecordOfFacilityDamage: ObservationRecord
    {
        public string RecordLocation { get; set; }
        public GeoCoordinate RecordCoordinate { get; set; }

        public RecordOfFacilityDamage()
        {
            RecordType = (int)RecordTypeEnum.Facility_Damage;
        }
    }
}
