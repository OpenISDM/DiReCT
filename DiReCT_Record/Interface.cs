using System.Collections.Generic;

namespace DiReCT_Record
{
    #region RecordOfFieldVisits
    public interface IRecordOfDebrisFlow
    {
        string CatchmentDatas { get; set; }
        string RockDatas { get; set; }
        string PlantationDatas { get; set; }
        string SlopeDatas { get; set; }

        List<Catchment> GetCatchmentList();
        List<Rock> GetRockList();
        List<Plantation> GetPlantationList();
        List<Slope> GetSlopeList();
    }

    public interface IRecordOfFlood
    {
        int WaterLevel { get; set; }
        string CauseOfDisaster { get; set; }
        float RainFall { get; set; }
    }
    #endregion

    #region RecordOfMedical

    #endregion

    #region RecordOfFacilityDamage

    #endregion
}
