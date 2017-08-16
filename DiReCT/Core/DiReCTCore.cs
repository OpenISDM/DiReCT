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
using DiReCT.Model.Utilities;
using System.Threading;

namespace DiReCT
{
    public partial class DiReCTCore
    {
        public static PriorityWorkQueue<WorkItem> CoreWorkQueue;
        public static bool IsRunning;
        /// <summary>
        /// Initialize necessary variables and set up event handlers between
        /// Core and individual modules
        /// </summary>
        public DiReCTCore()
        {
            // Initialize DiReCTCore
            CoreWorkQueue = new PriorityWorkQueue<WorkItem>(
                                         (int)WorkPriority.NumberOfPriorities);
            IsRunning = true;

            //Initialize CoreDM variables
            InitCoreDM();
        }

        /// <summary>
        /// This function is responsible for getting workItem from UI and send 
        /// the workItem to respective module. The main thread will mostly be 
        /// run inside this method waiting for workItem to arrive and distributing
        /// them to modules.
        /// </summary>
        public void Run()
        {
            while (IsRunning)
            {
                WorkItem workItem;
                // Wait for work to arrive
                int priority = CoreWorkQueue.Dequeue(out workItem); 

                if (priority != -1)
                {
                    switch (workItem.GroupName)
                    {                      
                        case FunctionGroupName.DataManagementFunction:
                            // Pass work Item to DM processor
                            CoreDMFunctionProcessor(workItem);
                            break;

                        case FunctionGroupName.AuthenticateAuthoriseFunction:
                            // Not implemented
                            break;

                        case FunctionGroupName.DataSyncFunction:
                            // Not implemented
                            break;

                        case FunctionGroupName.MonitorAlertNotificationFunction:
                            // Not implemented
                            break;

                        case FunctionGroupName.QualityControlFunction:
                            // Not implemented
                            break;

                        case FunctionGroupName.TerminateFunction:
                            IsRunning = false;
                            break;
                        default:
                            // Exception
                            break;
                    }
                }
                else
                {
                    throw new Exception();
                }
            }

        }

        
        public static DiReCTCore _instance { get; set; }
        /// <summary>
        /// This function ensures there is only one instance of Core
        /// </summary>
        /// <returns></returns>
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
            // Initialize workItem to terminate the program
            WorkItem workItem = new WorkItem(
                FunctionGroupName.TerminateFunction,
                AsyncCallName.TerminateProgram, null, null, null);
            // Enqueue the workItem
            CoreWorkQueue.Enqueue(workItem, (int)WorkPriority.Highest,
                new CancellationToken());
        }
    }
}
