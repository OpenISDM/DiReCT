using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_wpf.Model
{
    public class FakeData
    {
        public FakeData()
        {
            LocationsFromServer = new ObservableCollection<Location>();
        }
        public ObservableCollection<Location> LocationsFromServer { get; set; }

        public void addLocation(double lat, double lon)
        {
            LocationsFromServer.Add(new Model.Location(lat, lon));
        }
    }

    public class Location
    {
        public Location(double lat, double lon)
        {
            latitude = lat;
            longitude = lon;
        }
        public double longitude { get; set; }
        public double latitude { get; set; }
    }
}
