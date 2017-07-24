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
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

using DiReCT.Model.Utilities;
using DiReCT;
using System.Collections;
using DiReCT.Model;
using System.Windows.Threading;

namespace DiReCT
{
    class DMModule
    {
        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;
        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;
        static DiReCTThreadPool moduleThreadPool;
        static RecordDictionaryManager recordDictionaryManager;
        const int THREADPOOL_SIZE = 10;

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
                moduleThreadPool = new DiReCTThreadPool(THREADPOOL_SIZE);
                ModuleReadyEvent.Set();

                Debug.WriteLine("DMInit complete Phase 1 Initialization");

                // Wait for core StartWork Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("DMInit complete Phase 2 Initialization");

                //
                // Main Thread of DM module (begin)
                //               
                // Initialize dictionary manager
                recordDictionaryManager = new RecordDictionaryManager();

                // Whenever the SaveRecord Event is called, DMSavingRecordWrapper 
                // will be called
                RecordSavingTriggerd += new SaveRecordEventHanlder(
                                                       DMSavingRecordWrapper);
                              
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

        /// <summary>
        /// determines which methods to call. This function is aimed to be called
        /// by Threadpool worker thread.
        /// </summary>
        /// <param name="workItem"></param>
        internal static void DMWorkerFunctionProcessor(WorkItem workItem)
        {
            switch (workItem.AsyncCallName)
            {
                case AsyncCallName.SaveRecord:
                    SendRecordToRTQC(workItem);
                    break;
                //More cases
            }
        }
   
        /// <summary>
        /// Pass record to RTQC for validate
        /// </summary>
        /// <param name="workItem"></param>
        internal static void SendRecordToRTQC(WorkItem workItem)
        {
            // Get the index of record in buffer from workItem
            int index = (int)workItem.InputParameters;
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
                    RTQCModule.RTQCWrapWorkItem(AsyncCallName.Validate,
                        new AsyncCallback(SaveRecordtoDictionary),
                        record,
                        null);

                    workItem.Complete();
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
        static void SaveRecordtoDictionary(IAsyncResult result)
        {
            WorkItem workItem = (WorkItem)result;

            if ((bool)workItem.OutputParameters)
            {
                recordDictionaryManager.SaveRecord(false,
                                workItem.InputParameters);
            }
            else
            {
                recordDictionaryManager.SaveRecord(true,
                                workItem.InputParameters);
            }
        }

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
        /// This method will be called when SavingRecord Event is raised
        /// </summary>
        /// <param name="index">the index of the record in the buffer</param>
        public static void DMSavingRecordWrapper(int index)
        {
            
            WorkItem workItem = new WorkItem(
                FunctionGroupName.DataManagementFunction,
                AsyncCallName.SaveRecord,
                index,
                null,
                null);

            moduleThreadPool.AddThreadWork(workItem);
        }

    }
}


