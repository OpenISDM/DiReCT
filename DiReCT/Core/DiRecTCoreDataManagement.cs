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
 *      DiReCTCoreDataManagement is a part of DiReCTCore class.
 *      It gives the ability to execute data management functions on a thread
 *      different from the GUI thread.
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
using System.Threading;
using DiReCT.Model.Utilities;

namespace DiReCT
{
    public partial class DiReCTCore
    {
        public void SaveRecord(ObservationRecord RecordData, 
                               AsyncCallback CallBackFunction, 
                               Object AsyncState)
        {
            WorkItem workItem
                = new WorkItem(FunctionGroupName.DataManagementFunction,
                               AsyncCallName.SaveRecord,
                               (Object)RecordData,
                               CallBackFunction,
                               AsyncState);
            
            // Token for work cancelling
            CancellationToken cancellationToken = new CancellationToken();

            coreWorkQueue.Enqueue(workItem, 
                                  (int)WorkPriority.Normal, 
                                  cancellationToken);
        }

        // Implement the processor to handle work items
        private void DataManagementFunctionProcessor(WorkItem WorkItem)
        {
            // A switch case for each AsyncCallName
            // {
            //      Update current record buffer
            //      Raise DMSaveRecordEvent and pass the ID of current buffer
            //      Wait for AsyncResult if needed
            // }
        }

        //
        // More Data management functions here...
        //
    }
}
