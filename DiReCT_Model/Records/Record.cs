using System;
using System.Device.Location;
//The Record
public class Record
{
    //The Obeject Of Id
    public Guid Id { get; set; }
    //The Object Of Note On Disaster
    public string NoteOnDisaster { get; set; }
    //The Object Of Multi Media File Paths
    public string MultiMediaFilePaths { get; set; }
    //The Object Of Record Location         //ex:臺大醫院
    public string RecordLocation { get; set; }      
    //The Object Of Record Coordinate       //Ex: (25.0398916,121.5197956)
    public GeoCoordinate RecordCoordinate { get; set; }     
}

//The Record Of Debris Flow
public class DebrisFlow : Record
{
    //The Object Of Slope Angle
    public string SlopeAngle { get; set; }
    //The Object Of Plantation Data
    public string PlantationData { get; set; }
    //The Object Of Rock Data
    public string RockData { get; set; }
    //The Object Of Catchment Data
    public string CatchmentData { get; set; }
    //The Object Of Sediment Volume
    public float SedimentVolume { get; set; }
    //The Object Of Number Of Debris Flow Torrents In Towership
    public int NumberOfDebrisFlowTorrentsInTownship { get; set; }
    //The Object Of Number Of River
    public int NumberOfRiver { get; set; }
    //The Object Of Warning Criteria
    public int WarningCriteria { get; set; }
}

//The Record Of Flood
public class Flood : Record
{
    //The Object Of Water Level
    public float WaterLevel { get; set; }
    //The Object Of Cause Of Disaster 
    public string CauseOfDisaster { get; set; }
    //The Object Of Rainfall
    public float Rainfall { get; set; }
}

//The Record Of General Medical Record
public class GeneralMedicalRecord : Record
{
    //The Object Of Death Toll
    public int DeathToll { get; set; }
    //The Object Of Injury Toll
    public int InjuryToll { get; set; }
}

//The Record Of General Damage Record
public class GeneralDamageRecord : Record
{
   //The Object Of Number Of Public Utilites
    public int NumberOfPublicUtilites { get; set; }
   //The Object Of Number Of Collapsed
    public int NumberOfTreeCollapsed { get; set; }
}

//The Record Of Total Number Of People
public class TotalNumberOfPeople : GeneralMedicalRecord
{
    //The Object Of Number Of Missing
    public int NumberOfMissing { get; set; }
    //The Object Of Number Of Evacuee
    public int NumberOfEvacuee { get; set; }
    //The Object Of Number Of Trapped
    public int NumberOfTrapped { get; set; }
}
