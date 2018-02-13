using System;
using System.Device.Location;
//public class GeoCoordinate : IEquatable<GeoCoordinate> { }
//The Record
public class Record
{
    public Guid Id { get; set; }
    public string NoteOnDisaster { get; set; }
    public string MultiMediaFilePaths { get; set; }
    public string RecordLocation { get; set; }
    public GeoCoordinate RecordCoordinate { get; set; }
}
//The Record Of Debris Flow
public class DebrisFlow : Record
{
    public string SlopeAngle { get; set; }
    public string PlantationData { get; set; }
    public string RockData { get; set; }
    public string CatchmentData { get; set; }
    public float SedimentVolume { get; set; }
    public int NumbersOfDebrisFlowTorrentsInTownship { get; set; }
    public int NumbersOfRivers { get; set; }
    public int WarningCriteria { get; set; }
}
//The Record Of Flood
public class Flood : Record
{
    public float WaterLevel { get; set; }
    public string CauseOfDisaster { get; set; }
    public float Rainfall { get; set; }
}
//The Record Of General Medical Record
public class GeneralMedicalRecord : Record
{
    public int DeathToll { get; set; }
    public int InjuryToll { get; set; }
}
//The Record Of General Damage Record
public class GeneralDamageRecord : Record
{
    public int NumberOfPublicUtilites { get; set; }
    public int NumberOfTreeCollapsed { get; set; }
}
//The Record Of Total Number Of People
public class TotalNumberOfPeople : GeneralMedicalRecord
{
    public int NumberOfMissing { get; set; }
    public int NumberOfEvacuee { get; set; }
    public int NumberOfTrapped { get; set; }
}