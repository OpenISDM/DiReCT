using System.Collections.ObjectModel;

namespace DiReCT_wpf.Model
{
    public class LandslideRecord: RecordBase
    {
        public string injuryToll { get; set; }
        public string deathToll { get; set; }
        public ObservableCollection<string> conditions { get; set; }
    }
}
