/*
 * Copyright (c) 2016 Academia Sinica, Institude of Information Science
 * 
 *  This file is part of DiReCT.
 *
 *  DiReCT is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Foobar is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Project Name:
 * 
 *      DiReCT(Disaster Record Capture Tool)
 * 
 * File Description:
 * File Name:
 * 
 *      DiReCTCore.cs
 * 
 * Abstract:
 *      
 *      DiReCTCore is the background worker for user interface. 
 *      It gives the ability to execute time-consuming operations 
 *      asynchronously ("in the background"), on a thread different 
 *      from the UI thread.
 *
 * Authors:
 * 
 *      Hunter Hsieh, hunter205@iis.sinica.edu.tw  
 *      Jeff Chen, jeff@iis.sinica.edu.tw
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiReCT.Model.Utilities;
using System.Threading;

namespace DiReCT
{
    public partial class DiReCTCore
    {
        PriorityWorkQueue<WorkItem> coreWorkQueue;
        private AutoResetEvent workArriveEvent;
        public AutoResetEvent WorkArriveEvent
        {
            get
            {
                return workArriveEvent;
            }
            set
            {
                workArriveEvent = value;
            }
        }

        public DiReCTCore()
        {
            // Initialize DiReCTCore
            coreWorkQueue = new PriorityWorkQueue<WorkItem>(
                                          (int)WorkPriority.NumberOfPriorities);
            WorkArriveEvent = new AutoResetEvent(false);
        }

        public void Run()
        {           
            //
            // Wait for work arrive events
            // Dequeue and unwrap Workitems
            // A switch case for each events, e.g. WorkArriveEvent
            // {
            //      A switch case for each FunctionGroupName
            //      {
            //          Execute each function processor
            //      }
            // }
            //
           
        }

        public static DiReCTCore _instance { get; set; }

        public static DiReCTCore getInstance()
        {
            if(_instance == null)
            {
                _instance = new DiReCTCore();
            }

            return _instance;
        }
    }
}
