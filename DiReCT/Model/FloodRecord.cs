/*
* Copyright (c) 2016 Academia Sinica, Institude of Information Science
*
* License:
*      GPL 3.0 : This file is subject to the terms and conditions defined
*      in file 'COPYING.txt', which is part of this source code package.
*
* Project Name:
* 
* 		DiReCT(Disaster Record Capture Tool)
* 
* File Description:
* File Name:
* 
* 		FloodRecord.cs
* 
* Abstract:
*      
*       FloodRecord is a subclass inherited ObservationRecord.    
*       This class contains specific flood properties.   
*
* Authors:
* 
*      Hunter Hsieh, hunter205@iis.sinica.edu.tw  
*      Jeff Chen, jeff@iis.sinica.edu.tw
* 
*/
using System;
using System.Collections.Generic;


namespace DiReCT.Model.Observations
{
    public class Flood : ObservationRecord
    {
        


        public void sSetObservationRecord(string a, int b, int c)
        {
            
        }

        public override void SetObservationRecord()
        {
            throw new NotImplementedException();
        }





        /// <summary>
        /// 淹水深度
        /// The water levels of the flood.
        /// </summary>
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


    }
}
