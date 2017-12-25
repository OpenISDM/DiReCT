using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DiReCT_Record
{
    public enum EnumDisasterType
    {
        Error = 0,
        Flood,
        DebrisFlow,
    };

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
    public class DebrisFlow : RecordOfFieldVisits, IRecordOfDebrisFlow
    {
        public string CatchmentDatas { get; set; }
        public string RockDatas { get; set; }
        public string PlantationDatas { get; set; }
        public string SlopeDatas { get; set; }

        public DebrisFlow(List<Catchment> CatchmentList, List<Rock> RockList,
            List<Plantation> PlantationList, List<Slope> SlopeList)
        {
            CatchmentDatas = JsonConvert.SerializeObject(CatchmentList);
            RockDatas = JsonConvert.SerializeObject(RockDatas);
            PlantationDatas = JsonConvert.SerializeObject(PlantationList);
            SlopeDatas = JsonConvert.SerializeObject(SlopeList);
            DisasterType = (int)EnumDisasterType.DebrisFlow;
        }

        public DebrisFlow()
        {
            DisasterType = (int)EnumDisasterType.DebrisFlow;
        }

        public List<Catchment> GetCatchmentList()
        {
            return JsonConvert
                .DeserializeObject<List<Catchment>>(CatchmentDatas);
        }

        public List<Rock> GetRockList()
        {
            return JsonConvert
                .DeserializeObject<List<Rock>>(RockDatas);
        }

        public List<Plantation> GetPlantationList()
        {
            return JsonConvert
                .DeserializeObject<List<Plantation>>(PlantationDatas);
        }

        public List<Slope> GetSlopeList()
        {
            return JsonConvert
                .DeserializeObject<List<Slope>>(SlopeDatas);
        }
    }

    public class Flood : RecordOfFieldVisits, IRecordOfFlood
    {
        public int WaterLevel { get; set; }
        public string CauseOfDisaster { get; set; }
        public float RainFall { get; set; }

        public Flood()
        {
            DisasterType = (int)EnumDisasterType.Flood;
        }
    }
    #endregion

    #region RecordOfMedical
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
    public class DebrisFlowOfFacilityDamage
    {
        public string Description { get; set; }
    }

    public class FloodOfFacilityDamage
    {
        public string Description { get; set; }
    }
    #endregion
}
