using System.Collections.ObjectModel;

namespace DiReCT_wpf.Helpers
{
    class RecordGenerator
    {

        public static dynamic CreateFloodRecord(
            int waterLevel,
            ObservableCollection<string> causes,
            string currentLongitude,
            string currentLatitude,
            string currentTimeStamp)
        {
            dynamic record = DllFileLoader.CreateAnFloodInstance();
            record.waterLevel = waterLevel;
            record.PossibleCauseOfDisaster = causes;
            record.currentLongitude = currentLongitude;
            record.currentLatitude = currentLatitude;
            record.currentTimeStamp = currentTimeStamp;

            return record;
        }

        public static dynamic CreateLandslideRecord(
            int deathTroll,
            int injuryTroll,
            ObservableCollection<string> conditions,
            bool houseDamage,
            string houseSelected,
            bool farmDamage,
            string farmSelected,
            bool riverDamage,
            string riverSelected,
            bool groundDamage,
            string groundSelected,
            bool roadDamage,
            string roadSelected)
        {
            dynamic record = DllFileLoader.CreateALandslideInstance();

            record.deathTroll = deathTroll;
            record.injuryTroll = injuryTroll;
            record.checkedLandslideCondition = conditions;
            record.houseDamage = houseDamage;
            record.houseSelected = houseSelected;
            record.farmDamage = farmDamage;
            record.farmSelected = farmSelected;
            record.riverDamage = riverDamage;
            record.riverSelected = riverSelected;
            record.groundDamage = groundDamage;
            record.groundSelected = groundSelected;
            record.roadDamage = roadDamage;
            record.roadSelected = roadSelected;
            return record;
        }
    }
}
