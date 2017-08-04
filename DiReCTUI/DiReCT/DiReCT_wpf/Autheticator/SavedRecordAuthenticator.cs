using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiReCT_wpf.Model;
using DiReCT_wpf.ScreenInterface;
namespace DiReCT_wpf.Autheticator
{
   public class SavedRecordAuthenticator
    {
        private RecordBase record;

        public SavedRecordAuthenticator(Object r)
        {
            SaveButtonClickedEventArgs e = (SaveButtonClickedEventArgs)r;

            record = (RecordBase)e.SavedRecord;
        }

        public string Authenticate()
        {
            /*string address = record.Address;
            string waterLevelString = record.WaterLevel;
            double waterLevelDouble;
            Debug.WriteLine("address = " + address);
            Debug.WriteLine("waterLevel = " + waterLevelString);
            if (String.IsNullOrEmpty(address))
            {

                return "Please fill in address.";
            }
            else if (String.IsNullOrEmpty(waterLevelString))
            {

                return "Please fill in water level.";
            }
            try
            {
                waterLevelDouble = Convert.ToDouble(waterLevelString);

            }
            catch (FormatException)
            {
                return "The foramat of water level is incorrect.";
            }
            catch (OverflowException)
            {
                return "The value of water level is outside the range.";
            }*/



            return "sucess";
        }
    }
}
