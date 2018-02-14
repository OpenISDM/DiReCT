using System;
using System.Device.Location;

namespace DiReCT.Record
{
    /// <summary>
    /// The record
    /// </summary>
    public abstract class Record
    {
        // The obeject of Id
        public Guid Id { get; set; }
        // The object of note on disaster
        public string NoteOnDisaster { get; set; }
        // The object of multi media file paths
        public string MultiMediaFilePaths { get; set; }
        // The object of record location         //ex:臺大醫院
        public string RecordLocation { get; set; }
        // The object of record coordinate       //Ex: (25.0398916,121.5197956)
        public GeoCoordinate RecordCoordinate { get; set; }
    }

    /// <summary>
    /// The record of debris flow
    /// </summary>
    public class DebrisFlow : Record
    {
        // The object of slope angle
        public string SlopeAngle { get; set; }
        // The object of plantation data
        public string PlantationData { get; set; }
        // The object of rock data
        public string RockData { get; set; }
        // The object of catchment data
        public string CatchmentData { get; set; }
        // The object of sediment volume
        public float SedimentVolume { get; set; }
        // The object of number of debris flow torrents in towership
        public int NumberOfDebrisFlowTorrentsInTownship { get; set; }
        // The object of number of river
        public int NumberOfRiver { get; set; }
        // the object of warning criteria
        public int WarningCriteria { get; set; }
    }

    /// <summary>
    /// The record of flood
    /// </summary>
    public class Flood : Record
    {
        // The object of water level
        public float WaterLevel { get; set; }
        // The object of cause of disaster 
        public string CauseOfDisaster { get; set; }
        // The object of rainfall
        public float Rainfall { get; set; }
    }

    /// <summary>
    /// The record of general medical record
    /// </summary>
    public class GeneralMedicalRecord : Record
    {
        // The object of death toll
        public int DeathToll { get; set; }
        // The object of injury toll
        public int InjuryToll { get; set; }
    }

    /// <summary>
    /// The record of general damage record
    /// </summary>
    public class GeneralDamageRecord : Record
    {
        // The object of number of public utilites
        public int NumberOfPublicUtilites { get; set; }
        // The object of number of collapsed
        public int NumberOfTreeCollapsed { get; set; }
    }

    /// <summary>
    /// The record of total number of people
    /// </summary>
    public class TotalNumberOfPeople : GeneralMedicalRecord
    {
        // The object of number of missing
        public int NumberOfMissing { get; set; }
        // The object of number of evacuee
        public int NumberOfEvacuee { get; set; }
        // The object of number of trapped
        public int NumberOfTrapped { get; set; }
    }
}