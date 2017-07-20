using SOPObservationRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOPFlood
{
    public class SOPFlood: SOPObservationRecord.SOPObservationRecord
    {
        public double WaterLevel { get; set; }

        /// <summary>
        /// 水質混濁度
        /// The turbidity of the water to the naked eye.
        /// </summary>
        public int WaterTurbidity;

        /// <summary>
        /// 有無停電
        /// whether power has failed at the record location.
        /// enum:
        ///      1. Yes
        ///      2. No
        ///      3. I don't know
        /// </summary>
        public int IsPowerFailure;

        /// <summary>
        /// 淹水原因
        /// The main causes of flooding
        /// enum:
        ///      1. Afternoon thunderstorms, 
        ///      2. Typhoon, 
        ///      3. Human Negligence, 
        ///      4. I don't know 
        /// </summary>
        public int FloodingReason;

        public override void SetObservationRecord()
        {
            throw new NotImplementedException();
        }
    }
}
