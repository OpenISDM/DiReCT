using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_wpf.Model
{
    public class PossibleCausesOfDisaster
    {
        public List<string> causes;
        public PossibleCausesOfDisaster()
        {
            causes = new List<string>()
            {
                "Unknown",
                "Excessive precipitation",
                "Seawater intrusion",
                "Dam water discharge",
                "Pumping station failure",
                "Insufficient pumping",
                "Upstream overdevelopment",
                "Land subsidence",
                "Relatively low elevation",
                "River overflowing",
                "Broken embankment",
                "Incomplete levee construction",
                "Unclosed watergate",
                "Mudslide related",
                "Drainage channel blockage",
                "River siltation",
                "River obstruction",
                "Inundation Effect",
                "Road construction",
                "Earthquake related"
            };
        }
        static PossibleCausesOfDisaster possiblecauses;
        public static PossibleCausesOfDisaster GetInstance()
        {
            if (possiblecauses == null)
                possiblecauses = new PossibleCausesOfDisaster();
            return possiblecauses;
        }
    }
}
