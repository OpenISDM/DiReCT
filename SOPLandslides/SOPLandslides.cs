using System.Collections.ObjectModel;

namespace SOPLandslides
{
    /// <summary>
    /// This class contins the Landslides record. In order to use this class in
    /// DiReCT, this project must be build first. There will be a SOPFlood.dll
    /// and SOPObservationRecord.dll file in its bin/Debug Folder. By moving 
    /// those files to the DiReCT/bin/Debug folder, DiReCT will be able to
    /// use them as Record type. Same goes to DiReCT UI.
    /// </summary>
    public class SOPLandslides: SOPObservationRecord.SOPObservationRecord
    {
        public int deathTroll { get; set; }
        public int injuryTroll { get; set; }
        public ObservableCollection<string> checkedLandslideCondition { get; set; }
        public bool houseDamage { get; set; }
        public string houseSelected { get; set; }
        public bool farmDamage { get; set; }
        public string farmSelected { get; set; }
        public bool riverDamage { get; set; }
        public string riverSelected { get; set; }
        public bool groundDamage { get; set; }
        public string groundSelected { get; set; }
        public bool roadDamage { get; set; }
        public string roadSelected { get; set; }
    }
}
