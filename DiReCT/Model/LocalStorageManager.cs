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
 *      LocalStorageManager.cs
 * 
 * Abstract:
 *      
 *      Local Storage Manager provides functions for DM and DS 
 *      to access the event data, user data and record data 
 *      in the local storage.
 *
 * Authors:
 * 
 *      Hunter Hsieh, hunter205@iis.sinica.edu.tw  
 *      Jeff Chen, jeff@iis.sinica.edu.tw
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace DiReCT.Model
{
    public enum RecordStorageLocation
    {
        DefectiveData = 0,
        CleanData
    }

    public class LocalStorage
    {
        public static string StoragePath = @"./LocalStorage\";
        public static string TemporaryPath = StoragePath + @"TemporaryFolder\";

        /// <summary>
        /// A tool for copying picture media to temporary folder
        /// </summary>
        /// <param name="SourcePath"></param>
        /// <param name="FileName">The copied Id file name</param>
        /// <returns></returns>
        public static bool CopyMediaToTemporaryFolder(string SourcePath,
            out string FileName)
        {
            bool IsCopyDone = false;
            FileName = string.Empty;

            try
            {
                // Check if the source exists
                if (File.Exists(SourcePath))
                {
                    // Obtain file Extension
                    string Extension = "." +
                        Path.GetExtension(SourcePath);
                    // Generate a new Id ID file name
                    FileName = Guid.NewGuid().ToString() + Extension;
                    string DestPath = TemporaryPath + FileName;

                    // Check destination folder exists and copy
                    if (!File.Exists(DestPath))
                    {
                        File.Copy(SourcePath, DestPath);
                        IsCopyDone = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return IsCopyDone;
        }
    }


    public class RecordStorage
    {
        // This folder, used to store the record of each recorder
        // There will be one file for each recorder
        private static string RecordPath = 
            LocalStorage.StoragePath + @"Record\";

        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;
        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;

        // Multi-threaded environment, used to lock resources
        private static object RecordWorkQueueLock = new object();
        private static object FileLock = new object();
        private static object dictionaryLock = new object();

        // Add record work, Update record work, Remove record work
        private static Queue<(Action<(dynamic, RecordStorageLocation)>,
            (dynamic,RecordStorageLocation))> RecordWorkQueue;

        // Record storage loads the corresponding defective data or clean data 
        // according to the recorder's current data.
        private static Guid CurrentRecorder;
        // Tkey is record class full name, Tvalue is records
        private static Dictionary<string, List<dynamic>> defectiveData;
        private static Dictionary<string, List<dynamic>> cleanData;

        /// <summary>
        /// Record storage module initialization
        /// </summary>
        /// <param name="objectParameters"></param>
        public static void RSInit(object objectParameters)
        {
            moduleControlDataBlock
                 = (ModuleControlDataBlock)objectParameters;
            threadParameters = moduleControlDataBlock.ThreadParameters;

            try
            {
                // Initialize Ready/Abort Event and threadpool    
                ModuleReadyEvent = threadParameters.ModuleReadyEvent;
                ModuleAbortEvent = threadParameters.ModuleAbortEvent;

                RecordWorkQueue = new Queue
                    <(Action<(dynamic, RecordStorageLocation)>,
                    (dynamic, RecordStorageLocation))>();

                Debug.WriteLine("RSInit complete Phase 1 Initialization");

                // Wait for core StartWork Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Debug.WriteLine("RSInit complete Phase 2 Initialization");
                Debug.WriteLine("RS module is working...");

                RecordStoraageWork();

                Debug.WriteLine("RecordStorage module is aborting.");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("RecordStorage thread failed.");
                threadParameters.ModuleInitFailedEvent.Set();
                Debug.WriteLine("RecordStorage ModuleInitFailedEvent Set");
            }

            CleanupExit();
        }

        /// <summary>
        /// Obtain the recorder's data
        /// </summary>
        /// <param name="Records">Input record id</param>
        /// <returns>
        /// First dynamic array is defective Data,
        /// second dynamic array is cleanData
        /// </returns>
        public (dynamic[], dynamic[]) GetRecord(Guid Records)
        {
            // Check if the currently loaded logger is the desired logger
            // if true thhen return defective data and clean data
            if (Records == CurrentRecorder)
                return (RecordConsolidation(defectiveData),
                    RecordConsolidation(cleanData));

            // Read the data of a specific recorder
            // from the hard disk and send it back
            LoadOnFile(Records, 
                out Dictionary<string, List<dynamic>> defectiveRecord, 
                out Dictionary<string, List<dynamic>> cleanRecord);
            return (RecordConsolidation(defectiveRecord),
                    RecordConsolidation(cleanRecord));
        }

        /// <summary>
        /// Add record to local storage
        /// </summary>
        /// <param name="Record"></param>
        /// <param name="RecordLocation">
        /// defective data or clen data
        /// </param>
        public void Add(dynamic Record, RecordStorageLocation RecordLocation)
        {
            // Enqueue add record work to work queue
            lock (RecordWorkQueueLock)
                RecordWorkQueue.Enqueue((AddRecordWork,
                    (Record, RecordLocation)));
        }

        /// <summary>
        /// Update record to local storage
        /// </summary>
        /// <param name="Record"></param>
        /// <param name="RecordLocation">
        /// defective data or clen data
        /// </param>
        public void Update(dynamic Record,RecordStorageLocation RecordLocation)
        {
            // Enqueue update record work to work queue
            lock (RecordWorkQueueLock)
                RecordWorkQueue.Enqueue((UpdateRecordWork,
                    (Record, RecordLocation)));
        }

        /// <summary>
        /// Remove record to local storage
        /// </summary>
        /// <param name="Record"></param>
        /// <param name="RecordLocation">
        /// defective data or clen data
        /// </param>
        public void Remove(dynamic Record, 
            RecordStorageLocation RecordLocation)
        {
            // Enqueue remove record work to work queue
            lock (RecordWorkQueueLock)
                RecordWorkQueue.Enqueue((RemoveRecordWork,
                    (Record, RecordLocation)));
        }

        /// <summary>
        /// Function to handle work (add, update, remove)
        /// </summary>
        private static void RecordStoraageWork()
        {
            // Check ModuleAbortEvent periodically
            while (!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime))
            {
                // Wait module abort event or work
                SpinWait.SpinUntil(() => (ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime) && 
                        RecordWorkQueue.Count != 0));

                // Check whether inside the work queue has job or not
                lock (RecordWorkQueueLock)
                    if (RecordWorkQueue.Count != 0)
                    {
                        var Work = RecordWorkQueue.Dequeue();
                        Work.Item1.Invoke(Work.Item2);
                    }
            }
        }

        /// <summary>
        /// load json data on file then convert Json data into record objects
        /// </summary>
        /// <param name="Recorder">record id</param>
        /// <param name="defectiveData"></param>
        /// <param name="cleanData"></param>
        /// <returns></returns>
        private static bool LoadOnFile(
            Guid Recorder,
            out Dictionary<string, List<dynamic>> defectiveData,
            out Dictionary<string, List<dynamic>> cleanData)
        {
            bool LoadingSuccess = false;
            dynamic JSON = null;
            defectiveData = null;
            cleanData = null;

            try
            {
                // Open file and load json data
                lock (FileLock)
                    using (StreamReader streamReader
                        = new StreamReader(RecordPath + Recorder.ToString(),
                        Encoding.Default))
                    {
                        string buffer = streamReader.ReadToEnd();
                        JSON = JsonConvert.DeserializeObject(buffer);
                    }

                // Convert Json data into record objects
                lock (dictionaryLock)
                {
                    if (!ConvertToRecord((string)JSON.Defective,
                        out defectiveData))
                        defectiveData = new Dictionary<string,List<dynamic>>();

                    if (ConvertToRecord((string)JSON.Clean, 
                        out cleanData))
                        cleanData = new Dictionary<string, List<dynamic>>();
                }

                LoadingSuccess = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return LoadingSuccess;
        }

        /// <summary>
        /// work function for add record
        /// </summary>
        /// <param name="Record">
        /// Item1 is record
        /// Item2 is set the purpose of storage (defective data, clean data)
        /// </param>
        private static void AddRecordWork(
            (dynamic, RecordStorageLocation) Record)
        {
            // Check if the currently loaded logger is the desired logger
            // if true thhen adding
            // else reload data before adding
            lock (dictionaryLock)
                if (CurrentRecorder == Record.Item1.Recorder)
                    AddRecord(Record);
                else
                {
                    LoadOnFile(Record.Item1.Recorder,
                        out defectiveData,
                        out cleanData);

                    CurrentRecorder = Record.Item1.Recorder;

                    AddRecord(Record);
                }

            SaveChange();
        }

        /// <summary>
        /// work function for update record
        /// </summary>
        /// <param name="Record">
        /// Item1 is record
        /// Item2 is set the purpose of storage (defective data, clean data)
        /// </param>
        private static void UpdateRecordWork(
            (dynamic, RecordStorageLocation) Record)
        {
            // Check if the currently loaded logger is the desired logger
            // if true thhen updating
            // else reload data before updating
            lock (dictionaryLock)
                if (CurrentRecorder == Record.Item1.Recorder)
                    UpdateRecord(Record);
                else
                {
                    LoadOnFile(Record.Item1.Recorder,
                        out defectiveData,
                        out cleanData);

                    CurrentRecorder = Record.Item1.Recorder;

                    UpdateRecord(Record);
                }

            SaveChange();
        }

        /// <summary>
        /// work function for remove record
        /// </summary>
        /// <param name="Record">
        /// Item1 is record
        /// Item2 is set the purpose of storage (defective data, clean data)
        /// </param>
        private static void RemoveRecordWork(
            (dynamic, RecordStorageLocation) Record)
        {
            // Check if the currently loaded logger is the desired logger
            // if true thhen removing
            // else reload data before removing
            lock (dictionaryLock)
                if (CurrentRecorder == Record.Item1.Recorder)
                    RemoveRecord(Record);
                else
                {
                    LoadOnFile(Record.Item1.Recorder,
                        out defectiveData,
                        out cleanData);

                    CurrentRecorder = Record.Item1.Recorder;

                    RemoveRecord(Record);
                }

            SaveChange();
        }

        /// <summary>
        /// Add record function
        /// </summary>
        /// <param name="Record"></param>
        private static void AddRecord((dynamic, RecordStorageLocation) Record)
        {
            // Tkey of dictionary (defectiveData, cleanData)
            Type RecordType = Record.Item1.GetType();
            switch (Record.Item2)
            {
                case RecordStorageLocation.DefectiveData:
                    // Check if Tkey exists
                    // if exists then adding
                    // else add Tkey before adding new value
                    if (!defectiveData.ContainsKey(RecordType.FullName))
                        defectiveData.Add
                            (RecordType.FullName, new List<dynamic>());

                    defectiveData[RecordType.FullName].Add(Record.Item1);
                    break;

                case RecordStorageLocation.CleanData:
                    if (!cleanData.ContainsKey(RecordType.FullName))
                        cleanData.Add
                            (RecordType.FullName, new List<dynamic>());

                    cleanData[RecordType.FullName].Add(Record.Item1);
                    break;
            }
        }

        /// <summary>
        /// Update record function
        /// </summary>
        /// <param name="Record"></param>
        private static void UpdateRecord(
            (dynamic, RecordStorageLocation) Record)
        {
            // This function finds the key which stores record,  
            // and get the record list.
            // The the record with the same record id will be removed. 
            // The new record will added.
            var RecordId = Record.Item1.Id;
            Type RecordType = Record.Item1.GetType();
            List<dynamic> Records;
            switch (Record.Item2)
            {
                case RecordStorageLocation.DefectiveData:
                    Records = defectiveData[RecordType.FullName]
                        .Where(c => c.Id == RecordId)
                        .ToList();

                    foreach (var q in Records)
                        defectiveData[RecordType.FullName].Remove(q);

                    AddRecord(Record);
                    break;

                case RecordStorageLocation.CleanData:
                    Records = cleanData[RecordType.FullName]
                        .Where(c => c.Id == RecordId)
                        .ToList();

                    foreach (var q in Records)
                        cleanData[RecordType.FullName].Remove(q);

                    AddRecord(Record);
                    break;
            }
        }

        /// <summary>
        /// Remove record function
        /// </summary>
        /// <param name="Record"></param>
        private static void RemoveRecord(
            (dynamic, RecordStorageLocation) Record)
        {
            var RecordId = Record.Item1.Id;
            Type RecordType = Record.Item1.GetType();
            switch (Record.Item2)
            {
                case RecordStorageLocation.DefectiveData:
                    defectiveData[RecordType.FullName].Remove(Record.Item1);
                    break;

                case RecordStorageLocation.CleanData:
                    cleanData[RecordType.FullName].Remove(Record.Item1);
                    break;
            }
        }

        /// <summary>
        /// Save defectiveData and cleanData to file
        /// </summary>
        /// <returns></returns>
        private static bool SaveChange()
        {
            string Json;
            // Convert record to json
            lock (dictionaryLock)
                Json = JsonConvert.SerializeObject(new {
                    DefectiveData = JsonConvert.SerializeObject(defectiveData),
                    CleanData = JsonConvert.SerializeObject(cleanData)
                });

            // save json to file 
            return SaveJsonToFile(CurrentRecorder,Json);
        }

        /// <summary>
        /// Consolidate multiple lists into an array
        /// </summary>
        /// <param name="Records"></param>
        /// <returns></returns>
        private static dynamic[] RecordConsolidation(
            Dictionary<string, List<dynamic>> Records)
        {
            List<dynamic> RecordList = new List<dynamic>();
            foreach (var q in Records)
                RecordList.AddRange(q.Value);

            return RecordList.ToArray();
        }

        /// <summary>
        /// Save json data to file
        /// </summary>
        /// <param name="JSON">Input json data</param>
        /// <returns></returns>
        private static bool SaveJsonToFile(Guid Recorder,string JSON)
        {
            bool SaveSuccessfully = false;

            try
            {
                lock (FileLock)
                {
                    // Check the folder exists
                    if (!Directory.Exists(RecordPath))
                        Directory.CreateDirectory(RecordPath);

                    // Write data to the file
                    // Named using Logger Id
                    using (StreamWriter streamWriter
                        = new StreamWriter(RecordPath + Recorder.ToString()
                        , false))
                    {
                        streamWriter.WriteLine(JSON);
                    }
                }

                SaveSuccessfully = true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return SaveSuccessfully;
        }

        /// <summary>
        /// Convert json data to record object
        /// </summary>
        /// <param name="DataCategory">Input json data</param>
        /// /// <param name="OutPutData">Output Record Dictionary</param>
        /// <returns></returns>
        private static bool ConvertToRecord(string DataCategory, 
            out Dictionary<string, List<dynamic>> OutPutData)
        {
            bool IsConvertSuccess = false;
            OutPutData = 
                new Dictionary<string, List<dynamic>>();

            try
            {
                JObject JsonObject = JObject.Parse(DataCategory);

                // According to the number of record type
                foreach (KeyValuePair<string,JToken> KVP in JsonObject)
                {
                    // json reverse into objects, for example: list <flood>
                    Type DisasterType = Type.GetType(KVP.Key);
                    Type DisasterListType = typeof(List<>)
                        .MakeGenericType(new Type[] { DisasterType });

                    object Records = JsonConvert.DeserializeObject
                        (KVP.Value.ToString(), DisasterListType);

                    List<dynamic> RecordList = new List<dynamic>();
                    RecordList.AddRange((IEnumerable<dynamic>)Records);

                    // Add record list to the dictionary
                    OutPutData.Add(DisasterType.FullName, RecordList);
                }
                IsConvertSuccess = true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return IsConvertSuccess;
        }

        private static void CleanupExit()
        {
            defectiveData.Clear();
            defectiveData = null;
            cleanData.Clear();
            cleanData = null;
            RecordWorkQueue = null;
            RecordWorkQueueLock = null;
            FileLock = null;
            dictionaryLock = null;
        }
    }
}