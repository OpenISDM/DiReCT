using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using SOPObservationRecord;
using Newtonsoft.Json;
using Record;

namespace DiReCT.Model
{
    public enum RecordStorageLocation
    {
        DefectiveData = 0,
        CleanData
    }


    public class RecordStorage
    {
        private static string Path = "./LocalStorage\\";
        private static string RecordFilePath = Path + "Record";
        private static string TemporaryFolder = Path + "TemporaryFolder\\";
        private static object FileLock = new object();
        private static object dictionaryLock = new object();

        private static Dictionary<int, List<dynamic>> defectiveData;
        private static Dictionary<int, List<dynamic>> cleanData;
        private static bool IsInitialization = false;

        public RecordStorage()
        {
            if (!IsInitialization)
            {
                //If loading failed, then create a dictionary
                if (!LoadOnFile(out defectiveData,out cleanData))
                {
                    defectiveData = new Dictionary<int, List<dynamic>>();
                    cleanData = new Dictionary<int, List<dynamic>>();
                }
                IsInitialization = true;
            }
        }

        private bool LoadOnFile(
            out Dictionary<int, List<dynamic>> defectiveData,
            out Dictionary<int, List<dynamic>> cleanData)
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
                        = new StreamReader(RecordFilePath,
                        Encoding.Default))
                    {
                        string buffer = streamReader.ReadToEnd();
                        JSON = JsonConvert.DeserializeObject(buffer);
                    }

                //Convert Json data into record objects
                if (!ConvertToRecord((string)JSON.Defective, out defectiveData))
                    defectiveData = new Dictionary<int, List<dynamic>>();
                if (ConvertToRecord((string)JSON.Clean, out cleanData))
                    cleanData = new Dictionary<int, List<dynamic>>();

                LoadingSuccess = true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return LoadingSuccess;
        }

        /// <summary>
        /// Add Record
        /// </summary>
        /// <param name="Record">Record</param>
        /// <param name="storageLocation">DefectiveData or CleanData</param>
        public void Add(dynamic Record, RecordStorageLocation storageLocation)
        {
            DisasterCategory disasterCategory;
            switch (storageLocation)
            {
                case RecordStorageLocation.DefectiveData:
                    //Get the classification of this disaster record
                    disasterCategory = RecordClassification(Record);
                    if (disasterCategory != DisasterCategory.Error)
                    {
                        lock(dictionaryLock)
                        {
                            if (!defectiveData.ContainsKey((int)disasterCategory))
                                defectiveData.Add((int)disasterCategory,
                                    new List<dynamic>());

                            defectiveData[(int)disasterCategory].Add(Record);
                        }
                    }
                    break;

                case RecordStorageLocation.CleanData:
                     disasterCategory = RecordClassification(Record);
                    if (disasterCategory != DisasterCategory.Error)
                    {
                        lock(dictionaryLock)
                        {
                            if (!cleanData.ContainsKey((int)disasterCategory))
                                cleanData.Add((int)disasterCategory,
                                    new List<dynamic>());

                            cleanData[(int)disasterCategory].Add(Record);
                        }
                    }
                    break;
            }
        }

        public void AddRange(dynamic[] Records, 
            RecordStorageLocation storageLocation)
        {
            foreach (dynamic Record in Records)
                Add(Record, storageLocation);
        }

        /// <summary>
        /// Remove Record
        /// </summary>
        /// <param name="Record">Record</param>
        /// <param name="storageLocation">DefectiveData or CleanData</param>
        /// <returns></returns>
        public bool Remove(dynamic Record, 
            RecordStorageLocation storageLocation)
        {
            DisasterCategory disasterCategory;
            switch (storageLocation)
            {
                case RecordStorageLocation.DefectiveData:
                    //Get the classification of this disaster record
                    disasterCategory = RecordClassification(Record);
                    lock(dictionaryLock)
                        if (disasterCategory != DisasterCategory.Error)
                            return defectiveData[(int)disasterCategory].Remove(Record);
                    break;

                case RecordStorageLocation.CleanData:
                    disasterCategory = RecordClassification(Record);
                    lock (dictionaryLock)
                        if (disasterCategory != DisasterCategory.Error)
                            return cleanData[(int)disasterCategory].Remove(Record);
                    break;
            }
            return false;
        }

        public void RemoveRange(dynamic[] Records, 
            RecordStorageLocation storageLocation)
        {
            foreach (dynamic Record in Records)
                Remove(Record, storageLocation);
        }

        /// <summary>
        /// Get the all record
        /// </summary>
        /// <param name="storageLocation">DefectiveData or CleanData</param>
        /// <returns></returns>
        public dynamic[] GetRecords(RecordStorageLocation storageLocation)
        {
            List<dynamic> Records = new List<dynamic>();

            switch (storageLocation)
            {
                case RecordStorageLocation.DefectiveData:
                    lock(dictionaryLock)
                        foreach (var RecordDictionary in defectiveData)
                            Records.AddRange(RecordDictionary.Value);
                    break;

                case RecordStorageLocation.CleanData:
                    lock (dictionaryLock)
                        foreach (var RecordDictionary in cleanData)
                            Records.AddRange(RecordDictionary.Value);
                    break;
            }
            return Records.ToArray();
        }

        /// <summary>
        /// Get the specified record
        /// </summary>
        /// <param name="disasterCategory"></param>
        /// <param name="storageLocation">DefectiveData or CleanData</param>
        /// <returns></returns>
        public dynamic[] GetRecords(DisasterCategory disasterCategory,
            RecordStorageLocation storageLocation)
        {
            switch (storageLocation)
            {
                case RecordStorageLocation.DefectiveData:
                    lock (dictionaryLock)
                        return defectiveData[(int)disasterCategory].ToArray();

                case RecordStorageLocation.CleanData:
                    lock (dictionaryLock)
                        return cleanData[(int)disasterCategory].ToArray();
            }
            return null;
        }

        /// <summary>
        /// Save defectiveData and cleanData to file
        /// </summary>
        /// <returns></returns>
        public bool SaveChange()
        {
            return SaveRecordDictionaryToFile(
                GetRecords(RecordStorageLocation.DefectiveData), 
                GetRecords(RecordStorageLocation.CleanData));
        }

        /// <summary>
        /// Return the record disaster type
        /// </summary>
        /// <param name="Record">Input Record</param>
        /// <returns></returns>
        private DisasterCategory RecordClassification(dynamic Record)
        {
            if (Record.DisasterType == (int)DisasterCategory.Flood)
                return DisasterCategory.Flood;
            else if (Record.DisasterType == (int)DisasterCategory.LandSlide)
                return DisasterCategory.LandSlide;
            else
                return DisasterCategory.Error;
        }

        /// <summary>
        /// Save defective data and clean data to file
        /// </summary>
        /// <param name="DefectiveData">Input defective data</param>
        /// <param name="CleanData">Input clean data</param>
        /// <returns></returns>
        private bool SaveRecordDictionaryToFile(
            dynamic[] DefectiveData,
            dynamic[] CleanData)
        {
            bool SaveSuccessfully = false;

            try
            {
                lock (dictionaryLock)
                {
                    //Transform defective data and clean data into json
                    string JsonRresult = JsonConvert.SerializeObject(
                        new
                        {
                            Defective = ConvertToJson(DefectiveData),
                            Clean = ConvertToJson(CleanData)
                        });

                    if (SaveJsonToFile(JsonRresult))
                        SaveSuccessfully = true;
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return SaveSuccessfully;
        }       

        /// <summary>
        /// Save json data to file
        /// </summary>
        /// <param name="JSON">Input json data</param>
        /// <returns></returns>
        private bool SaveJsonToFile(string JSON)
        {
            bool SaveSuccessfully = false;

            try
            {
                lock (FileLock)
                {
                    //Check the folder exists
                    if (!Directory.Exists(Path))
                        Directory.CreateDirectory(Path);

                    if (!Directory.Exists(TemporaryFolder))
                        Directory.CreateDirectory(TemporaryFolder);

                    //Write data to the file
                    using (StreamWriter streamWriter
                        = new StreamWriter(RecordFilePath, false))
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
        private bool ConvertToRecord(string DataCategory, 
            out Dictionary<int, List<dynamic>> OutPutData)
        {
            bool IsConvertSuccess = false;
            OutPutData = 
                new Dictionary<int, List<dynamic>>();

            try
            {
                dynamic RecardClassification = 
                    JsonConvert.DeserializeObject(DataCategory);

                //Get flood records json data then convert to flood record array
                FloodRecord[] FloodRecords
                    = JsonConvert.DeserializeObject<FloodRecord[]>(
                    RecardClassification[((int)DisasterCategory.Flood).ToString()]
                    .ToString());

                //Get Landslides records json data 
                //then convert to Landslides record array
                LandslidesRecord[] LandslidesRecords
                    = JsonConvert.DeserializeObject<LandslidesRecord[]>(
                    RecardClassification[((int)DisasterCategory.LandSlide).ToString()]
                    .ToString());

                if(FloodRecords != null)
                {
                    OutPutData.Add((int)DisasterCategory.Flood,
                        new List<dynamic>());

                    OutPutData[(int)DisasterCategory.Flood]
                        .AddRange(FloodRecords);
                }
                if(LandslidesRecords != null)
                {
                    OutPutData.Add((int)DisasterCategory.LandSlide,
                        new List<dynamic>());

                    OutPutData[(int)DisasterCategory.LandSlide]
                        .AddRange(LandslidesRecords);
                }
                IsConvertSuccess = true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return IsConvertSuccess;
        }

        /// <summary>
        /// Convert records object to json
        /// </summary>
        /// <param name="Records">Input all record</param>
        /// <returns></returns>
        private string ConvertToJson(dynamic[] Records)
        {
            string result = string.Empty;
            //Tkey is the record type, TValue is the records object
            Dictionary<int, List<dynamic>> DisasterDictionary
                = new Dictionary<int, List<dynamic>>();

            try
            {
                foreach(dynamic Record in Records)
                {
                    //Check the dictionary whether it includes this record type
                    if (DisasterDictionary
                        .ContainsKey(Record.DisasterType))
                    {
                        DisasterDictionary[Record.DisasterType]
                            .Add(Record);
                    }
                    else
                    {
                        //Add this record type to dictionary
                        DisasterDictionary.Add(
                            Record.DisasterType,
                            new List<dynamic>());

                        DisasterDictionary[Record.DisasterType]
                            .Add(Record);
                    }
                }

                result = JsonConvert.SerializeObject(DisasterDictionary);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.HelpLink.ToString());
            }

            return result;
        }

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
                    string DestPath = TemporaryFolder + FileName;

                    //Check destination folder exists and copy
                    if (!File.Exists(DestPath))
                    {
                        File.Copy(SourcePath, DestPath);
                        IsCopyDone = true;
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return IsCopyDone;
        }
    }
}