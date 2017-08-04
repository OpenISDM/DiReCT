using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_wpf.Model
{
    public class LandslideRecord: RecordBase
    {
        public string injuryToll { get; set; }
        public string deathToll { get; set; }
        public ObservableCollection<string> conditions { get; set; }
    }
}
