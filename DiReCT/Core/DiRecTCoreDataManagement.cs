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
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using System.Threading.Tasks;
using DiReCT.Model;

namespace DiReCT
{
    public partial class DiReCTCore
    {

        #region Utility
        // Buffer variables
        public volatile static dynamic[] RecordBuffer;
        private static bool isBufferFull;
        private static object bufferLock;

        // Dll file variables
        public static DllFileLoader DllFileLoader; 
       
        public static class Constant
        {
            public const int MAX_NUMBER_OF_THREADS = 10;
            public const int ID_LENGTH = 10;
            public const int BUFFER_NUMBER = 3;
        }
        #endregion 

        private void InitCoreDM()
        {
            // DM buffer initialization
            RecordBuffer = new dynamic[Constant.BUFFER_NUMBER];
            isBufferFull = false;
            bufferLock = new object();

            // Dll variable initialization
            DllFileLoader = new DllFileLoader();

            // Subscribe to Main Window Saving Record Event
            MainWindow.MainWindowSavingRecord +=
                new MainWindow.CallCoreEventHanlder(PassSaveRecordToCore);
        }

        /// <summary>
        /// Function processor for DM. This function is aimed to be used
        /// by Core to raise DM event.
        /// </summary>
        /// <param name="workItem">WorkItem for DM</param>
        public void CoreDMFunctionProcessor(WorkItem workItem)
        {
            // Switch case based on the method 
            switch (workItem.AsyncCallName)
            {
                case AsyncCallName.SaveRecord:                  
                    try
                    {
                        // Get the record from workItem
                        dynamic record = workItem.InputParameters;

                        int index;
                        lock (bufferLock)
                        {
                            // Save record to buffer
                            index = SaveRecordToBuffer(record);
                        }
                        // Raise DM Event to save record
                        DMModule.OnSavingRecord(index);  
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("DiReCTCoreDM.CoreDMFunctionProcessor");
                        Debug.WriteLine(ex.Message);
                    }
                    break;
                case AsyncCallName.GetRecord:
                    //To Be Implemented                  
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
                                               out dynamic record)
        {
            // Check if index is valid
            if (index >= RecordBuffer.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            record = null;
            bool HasSucceeded = false;

            try
            {
                lock (bufferLock)
                {
                    // Get the record from buffer
                    record = RecordBuffer[index];
                }
                // Free the buffer index
                FreeBufferSpace(index);

                HasSucceeded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return HasSucceeded;
        }


        /// <summary>
        /// Free a record from buffer. This function is aimed to be 
        /// used by GetRecordFromBuffer method. 
        /// </summary>
        /// <param name="index">the index in buffer</param>
        private static void FreeBufferSpace(int index)
        {
            // Check for appropriate index
            if (index >= RecordBuffer.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            lock (bufferLock)
            {
                RecordBuffer[index] = null;
                isBufferFull = false;
            }
        }

        /// <summary>
        /// Save a record to a free buffer space.
        /// </summary>
        /// <param name="record">the record to be saved</param>
        /// <returns>the index of buffer that the record is saved to; 
        /// -1 if not found</returns>
        public int SaveRecordToBuffer(dynamic record)
        {
            int index = -1;
            bool IsFound = false;

            // Wait for free buffer index
            SpinWait.SpinUntil(() => !isBufferFull);

            while (!IsFound)
            {
                lock (bufferLock)
                {
                    // Look for any available index 
                    for (int i = 0; i < RecordBuffer.Length; i++)
                    {
                        if (RecordBuffer[i] == null)
                        {
                            index = i;
                            // Save record onto buffer 
                            RecordBuffer[i] = record; 
                            IsFound = true;

                            try
                            {
                                // Check if all records are not NULL
                                isBufferFull = RecordBuffer.All(x =>
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
        /// This function runs on UI worker Threads.
        /// The aim of this function is to pass workItem to Core and save
        /// record to buffer.
        /// </summary>
        /// <param name="recordData">the record to be saved</param>
        /// <param name="callBackFunction">call back function</param>
        /// <param name="asyncState"></param>
        public static bool CoreSaveRecord(dynamic recordData,
                               AsyncCallback callBackFunction,
                               Object asyncState)
        {
            bool HasSucceeded = false;

            try
            {
                // Initialize workItem
                WorkItem workItem
                    = new WorkItem(FunctionGroupName.DataManagementFunction,
                                   AsyncCallName.SaveRecord,
                                   (Object)recordData,
                                   callBackFunction,
                                   asyncState); 

                // Token for work cancelling
                CancellationToken cancellationToken = new CancellationToken();
                // Enqueue workItem to core queue
                CoreWorkQueue.Enqueue(workItem,
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

        #region Events

        /// <summary>
        /// This function is subscribed Main Window Saving Record Event and 
        /// will pass the record to Core when the event raised. It is aimed 
        /// to handled by Main Window worker Thread (BeginInvoke)
        /// </summary>
        /// <param name="obj">Object to pass to DM Save Record</param>
        public static void PassSaveRecordToCore(object obj)
        {
            // Pass record to core
            CoreSaveRecord(obj, null, null);
        }

        #endregion

    }
}
