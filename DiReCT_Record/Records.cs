using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DiReCT_Record
{
    //Enumerate disaster type
    public enum EnumDisasterType
    {
        Error = 0,
        Flood,
        DebrisFlow,
    };

    //Disaster events
    public class DisasterEvent
    {
        public Guid DisasterId { get; set; }
        public string DisasterName { get; set; }
        public int DisasterType { get; set; }
        public DateTime DisasterTime { get; set; }

        public DisasterEvent(EnumDisasterType disasterType)
        {
            DisasterType = (int)disasterType;
        }
    }

    #region RecordOfFieldVisits
    //Debris flow record of field visits
    public class DebrisFlowOfFieldVisits : RecordOfFieldVisits, IRecordOfDebrisFlow
    {

        //Catchment Datas
        public string CatchmentDatas { get; set; }
        //Rock Datas
        public string RockDatas { get; set; }
        //Plantation Datas
        public string PlantationDatas { get; set; }
        //Slope datas
        public string SlopeDatas { get; set; }

        //JSON is used because the database can't be inserted into the list
        public DebrisFlowOfFieldVisits(List<Catchment> CatchmentList, List<Rock> RockList,
            List<Plantation> PlantationList, List<Slope> SlopeList)
        {
            CatchmentDatas = JsonConvert.SerializeObject(CatchmentList);
            RockDatas = JsonConvert.SerializeObject(RockDatas);
            PlantationDatas = JsonConvert.SerializeObject(PlantationList);
            SlopeDatas = JsonConvert.SerializeObject(SlopeList);
            DisasterType = (int)EnumDisasterType.DebrisFlow;
        }

        public DebrisFlowOfFieldVisits()
        {
            DisasterType = (int)EnumDisasterType.DebrisFlow;
        }

        //Catchment data converted to list
        public List<Catchment> GetCatchmentList()
        {
            return JsonConvert
                .DeserializeObject<List<Catchment>>(CatchmentDatas);
        }

        //Rock data converted to list
        public List<Rock> GetRockList()
        {
            return JsonConvert
                .DeserializeObject<List<Rock>>(RockDatas);
        }

        //Plantation data converted to list
        public List<Plantation> GetPlantationList()
        {
            return JsonConvert
                .DeserializeObject<List<Plantation>>(PlantationDatas);
        }

        //Slope data converted to list
        public List<Slope> GetSlopeList()
        {
            return JsonConvert
                .DeserializeObject<List<Slope>>(SlopeDatas);
        }
    }

    public class FloodOfFieldVisits : RecordOfFieldVisits, IRecordOfFlood
    {
        //Water Leve
        public int WaterLevel { get; set; }
        //Cause Of Disaster
        public string CauseOfDisaster { get; set; }
        //Rain Fall
        public float RainFall { get; set; }

        public FloodOfFieldVisits()
        {
            DisasterType = (int)EnumDisasterType.Flood;
        }
    }
    #endregion

    #region RecordOfMedical
    //General Medical Record
    public class GeneralMedicalRecord : RecordOfMedical
    {
        public GeneralMedicalRecord(EnumDisasterType disasterType)
        {
            DisasterType = (int)disasterType;
        }

        public GeneralMedicalRecord()
        {

        }
    }
    #endregion

    #region RecordOfFacilityDamage
    //General Facility Damage Record
    public class GeneralFacilityDamageRecord : RecordOfFacilityDamage
    {
        public GeneralFacilityDamageRecord(EnumDisasterType disasterType)
        {
            DisasterType = (int)disasterType;
        }

        public GeneralFacilityDamageRecord()
        {

        }
    }
    #endregion
}
