using System.Collections.ObjectModel;

namespace SOPFlood
{
    /// <summary>
    /// This class contins the Flood record. In order to use this class in
    /// DiReCT, this project must be build first. There will be a SOPFlood.dll
    /// and SOPObservationRecord.dll file in its bin/Debug Folder. By moving 
    /// those files to the DiReCT/bin/Debug folder, DiReCT will be able to
    /// use them as Record type. Same goes to DiReCT UI.
    /// </summary>
    public class SOPFlood: SOPObservationRecord.SOPObservationRecord
    {
        public int waterLevel { get; set; }

        public ObservableCollection<string> PossibleCauseOfDisaster { get; set; }
    }
}
