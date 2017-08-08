using System.Collections.ObjectModel;

namespace DiReCT_wpf.Model
{
    public class FloodRecord: RecordBase
    {
        public string WaterLevel { get; set; }
        public ObservableCollection<string> causes { get; set; }
    }
}
