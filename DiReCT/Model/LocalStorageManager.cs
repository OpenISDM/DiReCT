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
                //Check if the source exists
                if (File.Exists(SourcePath))
                {
                    //Obtain file Extension
                    string Extension = "." +
                        System.IO.Path.GetExtension(SourcePath);
                    //Generate a new Id ID file name
                    FileName = Guid.NewGuid().ToString() + Extension;
                    string DestPath = TemporaryPath + FileName;

                    //Check destination folder exists and copy
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
        private static string RecordPath = 
            LocalStorage.StoragePath + @"Record\";

        static ModuleControlDataBlock moduleControlDataBlock;
        static ThreadParameters threadParameters;
        static ManualResetEvent ModuleAbortEvent, ModuleStartWorkEvent;
        static AutoResetEvent ModuleReadyEvent;

        private static object RecordWorkQueueLock = new object();
        private static object FileLock = new object();
        private static object dictionaryLock = new object();

        private static Queue<(Action<(dynamic, RecordStorageLocation)>,
            (dynamic,RecordStorageLocation))> RecordWorkQueue;
        private static Guid CurrentRecorder;
        private static Dictionary<string, List<dynamic>> defectiveData;
        private static Dictionary<string, List<dynamic>> cleanData;

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

        public (dynamic[], dynamic[]) GetRecord(Guid Records)
        {
            if (Records == CurrentRecorder)
                return (RecordConsolidation(defectiveData),
                    RecordConsolidation(cleanData));


            LoadOnFile(Records, 
                out Dictionary<string, List<dynamic>> defectiveRecord, 
                out Dictionary<string, List<dynamic>> cleanRecord);
            return (RecordConsolidation(defectiveRecord),
                    RecordConsolidation(cleanRecord));
        }

        public void Add(dynamic Record, RecordStorageLocation RecordLocation)
        {
            lock (RecordWorkQueueLock)
                RecordWorkQueue.Enqueue((AddRecordWork,
                    (Record, RecordLocation)));
        }

        public void Update(dynamic Record,RecordStorageLocation RecordLocation)
        {
            lock (RecordWorkQueueLock)
                RecordWorkQueue.Enqueue((UpdateRecordWork,
                    (Record, RecordLocation)));
        }

        public void Remove(dynamic Record, 
            RecordStorageLocation RecordLocation)
        {
            lock (RecordWorkQueueLock)
                RecordWorkQueue.Enqueue((RemoveRecordWork,
                    (Record, RecordLocation)));
        }

        private static void RecordStoraageWork()
        {
            while(!ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime))
            {
                SpinWait.SpinUntil(() => (ModuleAbortEvent
                        .WaitOne((int)TimeInterval.VeryVeryShortTime) && 
                        RecordWorkQueue.Count != 0));

                if (RecordWorkQueue.Count != 0)
                {
                    var Work = RecordWorkQueue.Dequeue();
                    Work.Item1.Invoke(Work.Item2);
                }
            }
        }

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
                //Open file and get json data
                lock (FileLock)
                    using (StreamReader streamReader
                        = new StreamReader(RecordPath + Recorder.ToString(),
                        Encoding.Default))
                    {
                        string buffer = streamReader.ReadToEnd();
                        JSON = JsonConvert.DeserializeObject(buffer);
                    }

                //Convert Json data into record objects
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

        private static void AddRecordWork(
            (dynamic, RecordStorageLocation) Record)
        {
            lock(dictionaryLock)
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

        private static void UpdateRecordWork(
            (dynamic, RecordStorageLocation) Record)
        {
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

        private static void RemoveRecordWork(
            (dynamic, RecordStorageLocation) Record)
        {
            lock(dictionaryLock)
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

        private static void AddRecord((dynamic, RecordStorageLocation) Record)
        {
            Type RecordType = Record.Item1.GetType();
            switch (Record.Item2)
            {
                case RecordStorageLocation.DefectiveData:
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

        private static void UpdateRecord(
            (dynamic, RecordStorageLocation) Record)
        {
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
            lock (dictionaryLock)
                Json = JsonConvert.SerializeObject(new {
                    DefectiveData = JsonConvert.SerializeObject(defectiveData),
                    CleanData = JsonConvert.SerializeObject(cleanData)
                });

            return SaveJsonToFile(CurrentRecorder,Json);
        }

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
                    //Check the folder exists
                    if (!Directory.Exists(RecordPath))
                        Directory.CreateDirectory(RecordPath);

                    //Write data to the file
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

                foreach (KeyValuePair<string,JToken> KVP in JsonObject)
                {
                    Type DisasterType = Type.GetType(KVP.Key);
                    Type DisasterListType = typeof(List<>)
                        .MakeGenericType(new Type[] { DisasterType });

                    object Records = JsonConvert.DeserializeObject
                        (KVP.Value.ToString(), DisasterListType);

                    List<dynamic> RecordList = new List<dynamic>();
                    RecordList.AddRange((IEnumerable<dynamic>)Records);

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