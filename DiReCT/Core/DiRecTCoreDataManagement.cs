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
 *      Joe Huang, huangjoe9@gmail.com
 * 
 */

using System;
using System.Linq;
using System.Threading;
using DiReCT.Model.Utilities;
using System.Diagnostics;
using DiReCT.Model;
using System.Collections;
using DiReCT_wpf.ScreenInterface;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DiReCT
{
    public partial class DiReCTCore
    {

        #region Utility
        // Buffer variables
        public volatile static dynamic[] RecordBuffer;
        private static object[] bufferLock;
        private static BitArray bufferSpaceAvailable;
 
        // Dll file variables
        public static DllFileLoader DllFileLoader;

        public static class Constant
        {
            public const int MAX_NUMBER_OF_THREADS = 10;
            public const int ID_LENGTH = 10;
            public const int BUFFER_NUMBER = 140;
        }
        #endregion 

        private void InitCoreDM()
        {
            // DM buffer initialization
            RecordBuffer = new dynamic[Constant.BUFFER_NUMBER];
            bufferLock = new object[Constant.BUFFER_NUMBER];
            bufferSpaceAvailable = new BitArray(Constant.BUFFER_NUMBER);
            for(int i = 0; i < Constant.BUFFER_NUMBER; i++)
            {
                bufferLock[i] = new object();   
                bufferSpaceAvailable[i] = true; // Set every space to available
            }

            // Dll variable initialization
            DllFileLoader = new DllFileLoader();

            // Subscribe to UI Saving Record Event           
            MenuViewBase.UIRecordSavingTriggerd +=
                new MenuViewBase.SaveRecordEventHanlder(PassSaveRecordToCore);          
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
            bool GotRecord = false;

            try
            {
                record = RecordBuffer[index];
                // Free the buffer index
                FreeBufferSpace(index);

                GotRecord = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return GotRecord;
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

            // Free the buffer space
            RecordBuffer[index] = null;
            // Change buffer space availability to true
            lock (bufferLock[index])
            {
                bufferSpaceAvailable[index] = true;
            }
        }

        /// <summary>
        /// Save a record to a free buffer space. The thread will loop until 
        /// there is a free space
        /// </summary>
        /// <param name="record">the record to be saved</param>
        /// <returns>the index of buffer that the record is saved to; 
        /// -1 if not found</returns>
        public int SaveRecordToBuffer(dynamic record)
        {
            int index = -1;
            bool isFound = false;
            // Wait for free buffer index
            SpinWait.SpinUntil(() => bufferSpaceAvailable.
                                     Cast<bool>().Contains(true));

            // Look for any available index 
            // The while loop is to prevent two or multiple possible writer 
            // Waiting at the same time and race for the same space.
            while (isFound == false)
            {
                for (int i = 0; i < Constant.BUFFER_NUMBER; i++)
                {   
                    // Lock the buffer index            
                    lock (bufferLock[i])
                    {                        
                        if (bufferSpaceAvailable[i])
                        {
                            index = i;
                            // Save record onto buffer 
                            RecordBuffer[i] = record;
                            // Mark buffer space as unavailable
                            bufferSpaceAvailable[i] = false;
                            isFound = true;
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
        /// The aim of this function is to queue workItems to be executed 
        /// by worker threads and save record to buffer.
        /// </summary>
        /// <param name="recordData">the record to be saved</param>
        /// <param name="callBackFunction">call back function</param>
        /// <param name="asyncState"></param>
        public static bool CoreSaveRecord(dynamic recordData,
                               AsyncCallback callBackFunction,
                               Object asyncState)

        {
            bool HasEnqueued = false;
           
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
                // Set return to true
                HasEnqueued = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DiReCTCoreDataManagement.CoreSaveRecord Exception");
                Debug.WriteLine(ex.Message);
            }
            return HasEnqueued;
        }


        #endregion

        #region Events

        /// <summary>
        /// This function is subscribed Main Window Saving Record Event and 
        /// will pass the record to Core when the event raised. It is aimed 
        /// to be executed by Main Window worker Thread (BeginInvoke)
        /// </summary>
        /// <param name="record">record to be passed to DM Save Record</param>
        public static void PassSaveRecordToCore(dynamic record)
        {         
            // Pass record to core
            CoreSaveRecord(record, null, null);
        }

        public static void PrintDictionary(object obj)
        {              
            dynamic[] or = RecordDictionaryManager.getAllCleanRecords();
            Debug.WriteLine("____________________________________________________________________");
            Debug.WriteLine("Current Count is " + or.Count());
            for (int i = 0; i < or.Length; i++)
            {
                if (or[i].GetType().ToString().Contains("Flood"))
                {

                    int id = or[i].RecordID;
                    int wl = (or[i]).waterLevel;
                    string cl = or[i].currentLongitude;
                    string clt = or[i].currentLatitude;
                    string ct = or[i].currentTimeStamp;

                    Debug.WriteLine("Flood Record:\n" +
                        "ID :" + id +
                        "\nWaterLevel: " + wl +
                        "\nCurrent Latitude: " + cl +
                        "\nCurrent Longitude: " + clt +
                        "\nCurrent TimeStamp: " + ct +
                        "\nPossible Cause:");

                    for (int j = 0; j < or[i].PossibleCauseOfDisaster.Count; j++)
                    {
                        string tempCause = or[i].PossibleCauseOfDisaster[j];
                        Debug.WriteLine(tempCause);
                    }
                    Debug.WriteLine("");
                }
                else
                {
                    int id = or[i].RecordID;
                    
                    int dead = or[i].deathTroll;
                    int injury = or[i].injuryTroll;

                    Debug.WriteLine("LandSlides Record:\n" +
                        "ID :" + id +
                        "\ndeathTroll: " + dead +
                        "\ninjuryTroll: " + injury                    
                        );                 
                }
            }
            Debug.WriteLine("____________________________________________________________________");
        }
       
        #endregion

    }
}
