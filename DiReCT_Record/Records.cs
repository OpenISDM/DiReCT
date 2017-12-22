using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_Record
{
    public enum DisasterTypeEnum
    {
        Error = 0,
        Flood,
        LandSlide,
    };

    public class DisasterEvent
    {
        public Guid DisasterId { get; set; }
        public string DisasterName { get; set; }
        public int DisasterType { get; set; }
        public DateTime DisasterTime { get; set; }

        public DisasterEvent(DisasterTypeEnum disasterType)
        {
            DisasterType = (int)disasterType;
        }
    }
}
