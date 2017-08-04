using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_wpf.Model
{
    public class ListOfLocations
    {
        public ObservableCollection<LocationData> Locations { get; set; }
        public DataFromServer dataFromServer { get; set; }
        public ListOfLocations()
        {
            dataFromServer = new DataFromServer();
            Locations = new ObservableCollection<LocationData>();
            for (int i = 0; i < dataFromServer.data.LocationsFromServer.Count(); i++)
            {
                Locations.Add(new LocationData() { Longitude = dataFromServer.data.LocationsFromServer[i].longitude, Latitude = dataFromServer.data.LocationsFromServer[i].latitude });
                //Locations[i].WaterLevelTimeStamps.Add(new WaterLevelTimeStamp(DateTime.Now.AddHours(i * 0.5), i + 10));
                //Locations[i].WaterLevelTimeStamps.Add(new WaterLevelTimeStamp(DateTime.Now.AddHours((i+1) * 0.5), (i+1) + 10));
            }
        }
        static ListOfLocations _instance;

        public static ListOfLocations GetInstance()
        {
            if (_instance == null)
                _instance = new ListOfLocations();
            return _instance;
        }
    }
}
