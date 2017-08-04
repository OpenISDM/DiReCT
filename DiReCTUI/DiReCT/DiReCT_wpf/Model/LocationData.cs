using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_wpf.Model
{
    public class LocationData
    {
        public LocationData()
        {
            Random rd = new Random();
            WaterLevelTimeStamps = new ObservableCollection<WaterLevelTimeStamp>();

        }
        public ObservableCollection<WaterLevelTimeStamp> WaterLevelTimeStamps { get; set; }

        public void addWaterLevel(DateTime date, double level)
        {
            WaterLevelTimeStamps.Add(new WaterLevelTimeStamp(date, level));
        }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
    public class WaterLevelTimeStamp
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }

        public WaterLevelTimeStamp(DateTime date, double value)
        {
            Date = date;
            Value = value;
        }
    }
}
