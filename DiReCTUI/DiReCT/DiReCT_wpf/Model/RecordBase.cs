using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_wpf.Model
{
    public abstract class RecordBase
    {
        public string Time { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
    }
}
