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
using DiReCT.Model.Observations;
using System.Diagnostics;

namespace DiReCT
{
    public partial class DiReCTCore
    {
        #region Utility

        public volatile static ObservationRecord[] recordBuffer;
        private static bool isBufferFull;
        private static object BufferLock;

        public static class Constant
        {
            /*
             * Define constnat here
             */

            public const int NUMBER_OF_MAX_THREADS = 10;

            public const int ID_LENGTH = 10;

            public const int BUFFER_NUMBER = 3;

        }

        private struct USER
        {
            //to be implemented...
            public String UserID;
            public String LastName;
            public String FirstName;

        }


        private enum SCHEDULEEVENT_TYPE
        {
            Appointment,
            Without_Appointment
        }

        private struct SCHEDULELOCATION
        {
            public String Location_Name;
            public Double CoordinateX;
            public Double CoordinateY;
            public DateTime Schedule_Arrival_Time;
        }

        private struct SCHEDULEEVENT
        {
            public String EventID;
            public SCHEDULEEVENT_TYPE ScheduleEvent_Type;
            public DateTime EventDateTime;
            public DateTime StartTime;
            public DateTime EndTime;
            public TimeSpan TimeInterval;

            //
            //TO BE ADDED...location, how many times, etc
            //
        }

        #endregion 

        /// <summary>
        /// Function processor for DM. This function is aimed to be used
        /// by Core to call different DM API.
        /// </summary>
        /// <param name="workItem">WorkItem for DM</param>
        public void CoreDMFunctionProcessor(WorkItem workItem)
        {
            //Switch based on method 
            switch (workItem.AsyncCallName)
            {
                case AsyncCallName.SaveRecord:
                    //Get the record from workItem
                    try
                    {

                        ObservationRecord record =
                            (ObservationRecord)workItem.InputParameters;

                        int index;
                        lock (BufferLock)
                        {
                            //save record to buffer
                            index = SaveRecordToBuffer(record);
                        }

                        //Call DM API wrapper
                        DMModule.DMWrapWorkItem(
                            AsyncCallName.SaveRecord,
                            null,
                            index,
                            null);

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("DiReCTCoreDM.CoreDMFunctionProcessor");
                        Debug.WriteLine(ex.Message);
                    }

                    break;

                case AsyncCallName.GetRecord:

                    //
                    //To Be Implemented...
                    //
                    break;
            }
        }

        #region Buffer Methods

        /// <summary>
        /// Get record from buffer based on index
        /// </summary>
        /// <param name="index">the index in buffer</param>
        /// <param name="record">the record to be assigned</param>
        /// <returns>whether record was assigned successfully</returns>
        public static bool GetRecordFromBuffer(int index,
                                               out ObservationRecord record)
        {

            //Check if index is valid
            if (index >= recordBuffer.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            record = null;
            bool HasSucceeded = false;

            try
            {
                lock (BufferLock)
                {
                    //Get the record from buffer
                    record = recordBuffer[index];
                }

                //Free the buffer index
                FreeBufferIndex(index);

                HasSucceeded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return HasSucceeded;
        }


        /// <summary>
        /// Free a ObservationRecord from buffer. This function is aimed to be 
        /// used by GetRecordFromBuffer
        /// </summary>
        /// <param name="index">the index in buffer</param>
        private static void FreeBufferIndex(int index)
        {

            if (index >= recordBuffer.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            lock (BufferLock)
            {
                recordBuffer[index] = null;

                isBufferFull = false;

            }
        }

        /// <summary>
        /// save a ObservationRecord to a free buffer index.
        /// </summary>
        /// <param name="record">the record to be saved</param>
        /// <returns>the index of buffer that the record is saved to; 
        /// -1 if not found</returns>
        public int SaveRecordToBuffer(ObservationRecord record)
        {
            int index = -1;
            bool IsFound = false;

            //Wait for free buffer index
            SpinWait.SpinUntil(() => !isBufferFull);

            while (!IsFound)
            {
                lock (BufferLock)
                {
                    //Look for any available index
                    for (int i = 0; i < recordBuffer.Length; i++)
                    {
                        if (recordBuffer[i] == null)
                        {
                            index = i;
                            recordBuffer[i] = record; //Save record onto buffer 
                            IsFound = true;

                            try
                            {
                                //check if all records are not NULL
                                isBufferFull = recordBuffer.All(x =>
                                                                x != null);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(
                                    "DiReCTCoreDM.SaveRecordToBuffer");
                                Debug.WriteLine(ex.Message);
                            }

                            break;
                        }
                    }
                }
            }
            return index;
        }
        #endregion

        #region CoreDM functions

        /// <summary>
        /// This function runs on UI Threads.
        /// The aim of this function is to pass workItem to Core and save
        /// recordData to buffer.
        /// </summary>
        /// <param name="recordData">the record to be saved</param>
        /// <param name="callBackFunction">call back function</param>
        /// <param name="asyncState"></param>
        public static bool CoreSaveRecord(ObservationRecord recordData,
                               AsyncCallback callBackFunction,
                               Object asyncState)
        {
            bool HasSucceeded = false;

            try
            {
                WorkItem workItem
                    = new WorkItem(FunctionGroupName.DataManagementFunction,
                                   AsyncCallName.SaveRecord,
                                   (Object)recordData,
                                   callBackFunction,
                                   asyncState);

                // Token for work cancelling
                CancellationToken cancellationToken = new CancellationToken();

                coreWorkQueue.Enqueue(workItem,
                                      (int)WorkPriority.Normal,
                                      cancellationToken);

                HasSucceeded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DiReCTCoreDataManagement.CoreSaveRecord Exception");
                Debug.WriteLine(ex.Message);
            }

            return HasSucceeded;
        }


        #endregion

    }
}
