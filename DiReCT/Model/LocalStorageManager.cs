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
using DiReCT.Logger;

namespace DiReCT.LocalStorage
{
    public enum RecordStorageCategory
    {
        DefectiveData = 0,
        CleanData
    }

    public class LocalStorage
    {
        public static string StorageFolder = @"./LocalStorage\";
        public static string TemporaryFolder = 
            StorageFolder + @"TemporaryFolder\";

        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;
        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;

        // Multi-threaded environment, used to lock resources
        static object StorageWorkQueueLock = new object();

        // Add work item, Update work item, Remove work item.
        static Queue<(Action<object>, object)> StorageWorkQueue;

        /// <summary>
        /// Local storage module initialization
        /// </summary>
        /// <param name="objectParameters"></param>
        public static void LSInit(object objectParameters)
        {
            moduleControlDataBlock
                 = (ModuleControlDataBlock)objectParameters;
            threadParameters = moduleControlDataBlock.ThreadParameters;

            try
            {
                // Initialize Ready/Abort Event and threadpool    
                ModuleReadyEvent = threadParameters.ModuleReadyEvent;
                ModuleAbortEvent = threadParameters.ModuleAbortEvent;

                StorageWorkQueue = new Queue<(Action<object>, object)>();

                Log.GeneralEvent
                    .Write("LSInit complete Phase 1 Initialization");

                // Wait for core StartWork Signal
                ModuleStartWorkEvent = threadParameters.ModuleStartWorkEvent;
                ModuleStartWorkEvent.WaitOne();

                Log.GeneralEvent
                    .Write("LSInit complete Phase 2 Initialization");
                Log.GeneralEvent
                    .Write("LS module is working...");

                //Local Storage start working
                LocalStorageWork();

                Log.GeneralEvent
                    .Write("LocalStorage module is aborting.");
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.Message);
                Log.ErrorEvent
                    .Write("LocalStorage thread failed.");
                threadParameters.ModuleInitFailedEvent.Set();
                Log.GeneralEvent
                    .Write("LocalStorage ModuleInitFailedEvent Set");
            }

            CleanupExit();
        }

        /// <summary>
        /// Function to handle work (add, update, remove)
        /// </summary>
        private static void LocalStorageWork()
        {
            // Check ModuleAbortEvent periodically
            while (!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime))
            {
                // Wait module abort event or work
                SpinWait.SpinUntil(() => (ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime) ||
                        StorageWorkQueue.Count != 0));

                // Check whether the work queue has work item.
                lock (StorageWorkQueueLock)
                    if (StorageWorkQueue.Count != 0)
                    {
                        // Work variable type is (Action<object>, object).
                        // The Item1 is Action<T> delegate. 
                        // ex: RecordStorage class -> AddRecordWork function
                        // The item2 is a object, 
                        // object which is to be fed into the work function.
                        var Work = StorageWorkQueue.Dequeue();
                        Work.Item1.Invoke(Work.Item2);
                    }
            }
        }

        /// <summary>
        /// Enqueue work item to the work queue
        /// </summary>
        /// <param name="action"></param>
        protected internal static void AddWorkItem(
            (Action<object>, object) action)
        {
            lock (StorageWorkQueueLock)
                StorageWorkQueue.Enqueue(action);
        }

        private static void CleanupExit()
        {
            //
            // Cleanup code
            //
            StorageWorkQueue = null;
            StorageWorkQueueLock = null;
        }

        /// <summary>
        /// A tool for copying multi media data to temporary folder
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
                    string DestPath = TemporaryFolder + FileName;

                    // Check whether destination folder exists and copy
                    if (!File.Exists(DestPath))
                    {
                        File.Copy(SourcePath, DestPath);
                        IsCopyDone = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.Message);
            }

            return IsCopyDone;
        }
    }


    public class RecordStorage
    {
        // Record folder, file is used to store the records of each 
        // record staff's.
        // Every record staff has their own file
        private static string RecordFolder =
            LocalStorage.StorageFolder + @"Record\";

        private static object FileLock = new object();
        private static object dictionaryLock = new object();

        // Record storage loads the corresponding defective data or clean data
        // according to the record staff's current data.
        private static Guid Current_Record_Staff;
        // Tkey is record class full name, Tvalue is records
        private static Dictionary<string, List<dynamic>> defectiveData;
        private static Dictionary<string, List<dynamic>> cleanData;

        // Encapsulate the information needed for the work
        // (add record, update record, remove record)
        private class RecordItem
        {
            public Guid Record_Staff_Id { get; set; }
            public dynamic Record { get; set; }
            public RecordStorageCategory Record_Location { get; set; }
        }

        /// <summary>
        /// Obtain the record staff's data
        /// </summary>
        /// <param name="Record_Staff_Id">Input record staff id</param>
        /// <returns>
        /// First dynamic array is defective data,
        /// second dynamic array is clean data
        /// </returns>
        public (dynamic[], dynamic[]) GetRecord(Guid Record_Staff_Id)
        {
            // Check if currently record logger is the desired record
            // if true then return defective data and clean data
            if (Record_Staff_Id == Current_Record_Staff)
                return (RecordConsolidation(defectiveData),
                    RecordConsolidation(cleanData));

            // Read the data of a specific record staff from the hard disk
            LoadRecordOnFile(Record_Staff_Id,
                out Dictionary<string, List<dynamic>> defectiveRecord,
                out Dictionary<string, List<dynamic>> cleanRecord);
            return (RecordConsolidation(defectiveRecord),
                    RecordConsolidation(cleanRecord));
        }

        /// <summary>
        /// Add record to local storage
        /// </summary>
        /// <param name="Record"></param>
        /// <param name="RecordTarget">
        /// defective data or clen data
        /// </param>
        public static void Add(
            Guid RecordStaffId,
            dynamic Record,
            RecordStorageCategory RecordTarget)
        {
            // Enqueue add record work item to work queue
            LocalStorage.AddWorkItem((AddRecordWork, new RecordItem
            {
                Record_Staff_Id = RecordStaffId,
                Record = Record,
                Record_Location = RecordTarget
            }));
        }

        /// <summary>
        /// Update record in local storage
        /// </summary>
        /// <param name="Record"></param>
        /// <param name="RecordTarget">
        /// defective data or clen data
        /// </param>
        public static void Update(
            Guid RecordStaffId,
            dynamic Record,
            RecordStorageCategory RecordTarget)
        {
            // Enqueue update record work item to work queue
            LocalStorage.AddWorkItem((UpdateRecordWork, new RecordItem
            {
                Record_Staff_Id = RecordStaffId,
                Record = Record,
                Record_Location = RecordTarget
            }));
        }

        /// <summary>
        /// Remove record to local storage
        /// </summary>
        /// <param name="Record"></param>
        /// <param name="RecordTarget">
        /// defective data or clen data
        /// </param>
        public static void Remove(
            Guid RecordStaffId,
            dynamic Record,
            RecordStorageCategory RecordTarget)
        {
            // Enqueue remove record work item to work queue
            LocalStorage.AddWorkItem((RemoveRecordWork, new RecordItem
            {
                Record_Staff_Id = RecordStaffId,
                Record = Record,
                Record_Location = RecordTarget
            }));
        }

        /// <summary>
        /// work function for add record
        /// </summary>
        /// <param name="ObjectParameters">RecordItem</param>
        private static void AddRecordWork(
            object ObjectParameters)
        {
            RecordItem recordItem =
                ObjectParameters as RecordItem;

            // Check if the currently loaded logger is the desired logger
            // if true thhen adding
            // else reload data before adding
            lock (dictionaryLock)
                if (Current_Record_Staff == recordItem.Record_Staff_Id)
                    AddRecord(recordItem);
                else
                {
                    LoadRecordOnFile(recordItem.Record_Staff_Id,
                        out defectiveData,
                        out cleanData);

                    Current_Record_Staff = recordItem.Record_Staff_Id;

                    AddRecord(recordItem);
                }

            SaveChange();
        }

        /// <summary>
        /// work function for update record
        /// </summary>
        /// <param name="ObjectParameters">RecordItem</param>
        private static void UpdateRecordWork(
            object ObjectParameters)
        {
            RecordItem recordItem =
                    ObjectParameters as RecordItem;

            // Check if the currently loaded logger is the desired logger
            // if true thhen updating
            // else reload data before updating
            lock (dictionaryLock)
                if (Current_Record_Staff == recordItem.Record_Staff_Id)
                    UpdateRecord(recordItem);
                else
                {
                    LoadRecordOnFile(recordItem.Record_Staff_Id,
                        out defectiveData,
                        out cleanData);

                    Current_Record_Staff = recordItem.Record_Staff_Id;

                    UpdateRecord(recordItem);
                }

            SaveChange();
        }

        /// <summary>
        /// work function for remove record
        /// </summary>
        /// <param name="ObjectParameters">RecordItem</param>
        private static void RemoveRecordWork(object ObjectParameters)
        {
            RecordItem recordItem =
                    ObjectParameters as RecordItem;
            // Check if the currently loaded logger is the desired logger
            // if true thhen removing
            // else reload data before removing
            lock (dictionaryLock)
                if (Current_Record_Staff == recordItem.Record_Staff_Id)
                    RemoveRecord(recordItem);
                else
                {
                    LoadRecordOnFile(recordItem.Record_Staff_Id,
                        out defectiveData,
                        out cleanData);

                    Current_Record_Staff = recordItem.Record_Staff_Id;

                    RemoveRecord(recordItem);
                }

            SaveChange();
        }

        /// <summary>
        /// Add record function
        /// </summary>
        /// <param name="recordItem"></param>
        private static void AddRecord(RecordItem recordItem)
        {
            // Tkey of dictionary (defectiveData, cleanData)
            Type RecordType = recordItem.Record.GetType();
            switch (recordItem.Record_Location)
            {
                case RecordStorageCategory.DefectiveData:
                    // Check if Tkey exists
                    // if exists then adding
                    // else add Tkey before adding new value
                    if (!defectiveData.ContainsKey(RecordType.FullName))
                        defectiveData.Add
                            (RecordType.FullName, new List<dynamic>());

                    defectiveData[RecordType.FullName].Add(recordItem.Record);
                    break;

                case RecordStorageCategory.CleanData:
                    if (!cleanData.ContainsKey(RecordType.FullName))
                        cleanData.Add
                            (RecordType.FullName, new List<dynamic>());

                    cleanData[RecordType.FullName].Add(recordItem.Record);
                    break;
            }
        }

        /// <summary>
        /// Update record function
        /// </summary>
        /// <param name="recordItem"></param>
        private static void UpdateRecord(RecordItem recordItem)
        {
            // This function finds the key which stores record,  
            // and get the record list.
            // The the record with the same record id will be removed. 
            // The new record will added.
            var RecordId = recordItem.Record.Id;
            Type RecordType = recordItem.Record.GetType();
            List<dynamic> Records;
            switch (recordItem.Record_Location)
            {
                case RecordStorageCategory.DefectiveData:
                    Records = defectiveData[RecordType.FullName]
                        .Where(c => c.Id == RecordId)
                        .ToList();

                    foreach (var q in Records)
                        defectiveData[RecordType.FullName].Remove(q);

                    AddRecord(recordItem);
                    break;

                case RecordStorageCategory.CleanData:
                    Records = cleanData[RecordType.FullName]
                        .Where(c => c.Id == RecordId)
                        .ToList();

                    foreach (var q in Records)
                        cleanData[RecordType.FullName].Remove(q);

                    AddRecord(recordItem);
                    break;
            }
        }

        /// <summary>
        /// Remove record function
        /// </summary>
        /// <param name="recordItem"></param>
        private static void RemoveRecord(
            RecordItem recordItem)
        {
            var RecordId = recordItem.Record.Id;
            Type RecordType = recordItem.Record.GetType();
            switch (recordItem.Record_Location)
            {
                case RecordStorageCategory.DefectiveData:
                    defectiveData[RecordType.FullName]
                        .Remove(recordItem.Record);
                    break;

                case RecordStorageCategory.CleanData:
                    cleanData[RecordType.FullName]
                        .Remove(recordItem.Record);
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
                Json = JsonConvert.SerializeObject(new
                {
                    DefectiveData =JsonConvert.SerializeObject(defectiveData),
                    CleanData = JsonConvert.SerializeObject(cleanData)
                });

            // save json to file 
            return SaveJsonToFile(Current_Record_Staff, Json);
        }

        /// <summary>
        /// Consolidate multiple lists into an array
        /// </summary>
        /// <param name="Records"></param>
        /// <returns></returns>
        //  This function referred by GetRecord function.
        private static dynamic[] RecordConsolidation(
            Dictionary<string, List<dynamic>> Records)
        {
            List<dynamic> RecordList = new List<dynamic>();
            foreach (var q in Records)
                RecordList.AddRange(q.Value);

            return RecordList.ToArray();
        }

        /// <summary>
        /// load record json data on file then convert Json data 
        /// into record objects
        /// </summary>
        /// <param name="RecordStaffId">Record staff id</param>
        /// <param name="defectiveData"></param>
        /// <param name="cleanData"></param>
        /// <returns></returns>
        //  This function referred by GetRecord function.
        private static bool LoadRecordOnFile(
            Guid RecordStaffId,
            out Dictionary<string, List<dynamic>> defectiveData,
            out Dictionary<string, List<dynamic>> cleanData)
        {
            bool LoadingSucceeded = false;
            dynamic JSON = null;
            defectiveData = null;
            cleanData = null;

            try
            {
                // Open file and load json data
                lock (FileLock)
                    using (StreamReader streamReader
                        = new StreamReader(RecordFolder +
                        RecordStaffId.ToString(),
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
                        defectiveData=new Dictionary<string, List<dynamic>>();

                    if (ConvertToRecord((string)JSON.Clean,
                        out cleanData))
                        cleanData = new Dictionary<string, List<dynamic>>();
                }

                LoadingSucceeded = true;
            }
            catch (Exception ex)
            {
                defectiveData = new Dictionary<string, List<dynamic>>();
                cleanData = new Dictionary<string, List<dynamic>>();
                Log.ErrorEvent.Write(ex.ToString());
                Log.ErrorEvent.Write("Load on file failed.");
            }
            return LoadingSucceeded;
        }

        /// <summary>
        /// Save json data to file
        /// </summary>
        /// <param name="JSON">Input json data</param>
        /// <returns></returns>
        //  This function referred by SaveChange function.
        private static bool SaveJsonToFile(Guid Record_Staff_Id, string JSON)
        {
            bool SaveSuccessfully = false;

            try
            {
                lock (FileLock)
                {
                    // Check the folder exists
                    if (!Directory.Exists(RecordFolder))
                        Directory.CreateDirectory(RecordFolder);

                    // Write data to the file
                    // Named using Logger Id
                    using (StreamWriter streamWriter
                        = new StreamWriter(RecordFolder +
                        Record_Staff_Id.ToString()
                        , false))
                    {
                        streamWriter.WriteLine(JSON);
                    }
                }

                SaveSuccessfully = true;
            }
            catch (Exception ex)
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
        //  This function referred by LoadRecordOnFile function.
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
                foreach (KeyValuePair<string, JToken> KVP in JsonObject)
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
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.ToString());
                Log.ErrorEvent.Write("Convert to record failed.");
            }

            return IsConvertSuccess;
        }
    }

    public class DutyStorage
    {
        // This folder, used to store from cloud downloads the files
        // required for this recording duty
        private static string DutyFolder =
            LocalStorage.StorageFolder + @"Duty\";

        private class DutyItem
        {
            public byte[] FileData { get; set; }
            public string FileName { get; set; }
        }

        /// <summary>
        /// save duty data 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="FileData"></param>
        public static void Save(string FileName, byte[] FileData)
        {
            // Enqueue add duty data work item to work queue
            LocalStorage.AddWorkItem((SaveFileWork, new DutyItem
            {
                FileData = FileData,
                FileName = FileName
            }));
        }

        /// <summary>
        /// delete duty data
        /// </summary>
        /// <param name="FileName"></param>
        public static void Delete(string FileName)
        {
            // Enqueue delete duty data work item to work queue
            LocalStorage.AddWorkItem((DeleteFileWork, new DutyItem
            {
                FileName = FileName
            }));
        }

        /// <summary>
        /// work function for save file
        /// </summary>
        /// <param name="ObjectParameters"></param>
        private static void SaveFileWork(object ObjectParameters)
        {
            DutyItem dutyItem = ObjectParameters as DutyItem;

            try
            {
                // Check the folder exists
                if (!Directory.Exists(DutyFolder))
                    Directory.CreateDirectory(DutyFolder);

                // Write file stream (byte array) to file
                using (FileStream fileStream = new FileStream(
                    DutyFolder + dutyItem.FileName,
                    FileMode.Create,
                    FileAccess.Write))
                {
                    fileStream.Write(dutyItem.FileData, 0,
                        dutyItem.FileData.Length);
                }

                Log.GeneralEvent.Write("Save " +
                    dutyItem.FileName +
                    " File successfully.");
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.ToString());
            }
        }

        /// <summary>
        /// work function for delete file
        /// </summary>
        /// <param name="ObjectParameters"></param>
        private static void DeleteFileWork(object ObjectParameters)
        {
            DutyItem dutyItem = ObjectParameters as DutyItem;

            try
            {
                // check file exists, then delete the file
                if (File.Exists(DutyFolder + dutyItem.FileName))
                {
                    File.SetAttributes(DutyFolder + dutyItem.FileName,
                        FileAttributes.Normal);
                    File.Delete(DutyFolder + dutyItem.FileName);

                    Log.GeneralEvent.Write("Save " +
                    dutyItem.FileName +
                    " File successfully.");
                }
            }
            catch (Exception ex)
            {
                Log.ErrorEvent.Write(ex.ToString());
            }
        }
    }
}