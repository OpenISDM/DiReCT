using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_wpf.Model
{
    public class FloodRecord: RecordBase
    {
        public string WaterLevel { get; set; }
        public ObservableCollection<string> causes { get; set; }
    }
}
