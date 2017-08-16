/*
 * Copyright (c) 2016 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and 
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 * 
 *      DiReCT(Disaster Record Capture Tool)
 * 
 * File Description:
 * File Name:
 * 
 *      DMModule.cs
 * 
 * Abstract:
 *      
 *      Data Manager (DM) provides functions for other modules 
 *      to access the event data, user data and record data 
 *      in the local storage.
 *
 * Authors:
 * 
 *      Hunter Hsieh, hunter205@iis.sinica.edu.tw  
 *      Jeff Chen, jeff@iis.sinica.edu.tw
 * 
 */

using System;
using System.Threading;
using System.Diagnostics;
using DiReCT.Model.Utilities;
using DiReCT.Model;
using Amib.Threading;
using System.Collections.Generic;

namespace DiReCT
{
    class DMModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;
        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;
        static RecordDictionaryManager recordDictionaryManager;
        static SmartThreadPool moduleThreadPool;
        public static void DMInit(object objectParameters)
        {
            moduleControlDataBlock
                = (ModuleControlDataBlock)objectParameters;
            threadParameters = moduleControlDataBlock.ThreadParameters;
            
            try
            {
                // Initialize Ready/Abort Event and threadpool    
                ModuleReadyEvent = threadParameters.ModuleReadyEvent;
                ModuleAbortEvent = threadParameters.ModuleAbortEvent;                          
                moduleThreadPool = threadParameters.moduleThreadPool;
                // Initialize dictionary manager
                recordDictionaryManager = new RecordDictionaryManager();        
                //Event Handlers Initialization                           
                RecordSavingTriggerd += new SaveRecordEventHanlder(
                                        DMSavingRecordWrapper);

                ModuleReadyEvent.Set();

                Debug.WriteLine("DMInit complete Phase 1 Initialization");
                                       
                // Wait for core StartWork Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("DMInit complete Phase 2 Initialization");           
                Debug.WriteLine("DM module is working...");
                
                // Check ModuleAbortEvent periodically
                while (!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime))
                {
                    // Does nothing and wait for abort event
                }

                Debug.WriteLine("DM module is aborting.");
                CleanupExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("DM module thread failed.");
                threadParameters.ModuleInitFailedEvent.Set();
                Debug.WriteLine("DM ModuleInitFailedEvent Set");
                CleanupExit();
            }
        }

        private static void CleanupExit()
        {
            //
            // Cleanup code
            //
            Debug.WriteLine("DM module stopped successfully.");
            return;
        }

        #region DM functions

        /// <summary>
        /// Pass record to RTQC for validate
        /// </summary>
        /// <param name="workItem"></param>
        private static void SendRecordToRTQC(int index)
        {           
            dynamic record;

            //
            // To Be Implemented...
            // SOP should decide the action, eg. save record, pass to rtqc
            //
            
            try
            {
                // Get the record from buffer 
                if (DiReCTCore.GetRecordFromBuffer(index, out record))
                {
                    // Call RTQC API
                    RTQCModule.OnValidate(record);          
                }
                else
                {
                    // Exception, index not valid
                }
            }catch(Exception ex)
            {
                Debug.WriteLine("DMModule.SendRecordToRTQC: " + ex.Message);
            }          
        }

        /// <summary>
        /// Save the record into Dictionary
        /// </summary>
        /// <param name="result"></param>
        public static void SaveRecordtoDictionary(IWorkItemResult wir)
        {
            // Check RTQC return value
            // Since the return value can only be one, the current solution is
            // to use KEYVALUEPAIR to store the return value plus the records
            if(((KeyValuePair<dynamic,bool>)wir.Result).Value)
            {
                recordDictionaryManager.SaveRecord(false,
                    ((KeyValuePair<dynamic, bool>)wir.Result).Key);
            }
            //WorkItem workItem = (WorkItem)result;

            //if ((bool)workItem.OutputParameters)
            //{
            //    recordDictionaryManager.SaveRecord(false,
            //                    workItem.InputParameters);               
            //}
            //else
            //{
            //    recordDictionaryManager.SaveRecord(true,
            //                    workItem.InputParameters);
            //}

            // Debugging function
            DiReCTCore.PrintDictionary(null);                 
        }
        #endregion

        #region DM Event Handlers and subscribed event

        // Delegate that specify the parameter of event handler
        public delegate void SaveRecordEventHanlder(int index);
        // Event Handler for Saving Record
        public static event SaveRecordEventHanlder RecordSavingTriggerd;

        /// <summary>
        /// Function to initiate the Saving Record event
        /// </summary>
        /// <param name="index"></param>
        public static void OnSavingRecord(int index)
        {
            RecordSavingTriggerd?.BeginInvoke(index,null,null);
        }

        /// <summary>
        /// This method will be called when SavingRecord Event is raised.
        /// </summary>
        /// <param name="index">the index of the record in the buffer</param>
        public static void DMSavingRecordWrapper(int index)
        {
            moduleThreadPool.QueueWorkItem(SendRecordToRTQC,
                index);
        }

        #endregion 

    }
}


