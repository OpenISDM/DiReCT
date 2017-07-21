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
 *      Joe Huang, huangjoe9@gmail.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiReCT.Model.Utilities;
using System.Threading;
using DiReCT.Model.Observations;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows;

namespace DiReCT
{
    public partial class DiReCTCore
    {
        public static PriorityWorkQueue<WorkItem> coreWorkQueue;
        public static bool isRunning;
        /// <summary>
        /// Initialize necessary variables and set up event handlers between
        /// Core and individual modules
        /// </summary>
        public DiReCTCore()
        {
            // Initialize DiReCTCore
            coreWorkQueue = new PriorityWorkQueue<WorkItem>(
                                          (int)WorkPriority.NumberOfPriorities);
            isRunning = true;

            //Initialize CoreDM variables
            InitCoreDM();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            while (isRunning)
            {
                WorkItem workItem; //record
                int priority = coreWorkQueue.Dequeue(out workItem); //wait for work to arrive

                if (priority != -1)
                {
                    switch (workItem.GroupName)
                    {
                        //send the item to each queue
                        case FunctionGroupName.DataManagementFunction:
                            //make a method in Core DM to handle different methods
                            CoreDMFunctionProcessor(workItem);
                            break;

                        case FunctionGroupName.AuthenticateAuthoriseFunction:
                            break;

                        case FunctionGroupName.DataSyncFunction:
                            break;

                        case FunctionGroupName.MonitorAlertNotificationFunction:
                            break;

                        case FunctionGroupName.QualityControlFunction:
                            break;

                        case FunctionGroupName.TerminateFunction:
                            isRunning = false;
                            break;
                    }
                }
                else
                {
                    //Throws error
                }
            }

        }

        //Singleton pattern
        public static DiReCTCore _instance { get; set; }

        public static DiReCTCore getInstance()
        {
            if (_instance == null)
            {
                _instance = new DiReCTCore();
            }

            return _instance;
        }


        /// <summary>
        /// Function to escape from the Run function and close the program
        /// </summary>
        public void TerminateProgram()
        {
            WorkItem workItem = new WorkItem(FunctionGroupName.TerminateFunction,
                AsyncCallName.TerminateProgram, null, null, null);
            coreWorkQueue.Enqueue(workItem, (int)WorkPriority.Highest,
                new CancellationToken());
        }
    }
}
