using DiReCT.Model;
using DiReCT.Model.Observations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT.Model
{
    class DictionaryManager
    {
        private static Dictionary<int, ObservationRecord> cleanData;
        private static Dictionary<int, ObservationRecord> defectedData;
        static IDMananger IDmanager;

        /// <summary>
        /// Initialize a Dictionary object 
        /// </summary>
        public DictionaryManager()
        {
            cleanData = new Dictionary<int, ObservationRecord>();
            defectedData = new Dictionary<int, ObservationRecord>();
            IDmanager = IDMananger.getInstance();
        }

        /// <summary>
        /// Get the record given recordID
        /// </summary>
        /// <param name="recordID">the ID of record</param>
        /// <param name="record">the record to be initialized</param>
        /// <returns>whether record is successfully obtained</returns>
        public bool GetRecord(int recordID, out ObservationRecord record)
        {
            bool HasSucceeded = false;
            record = null;

            try
            {
                //check record in both dictionary
                if (cleanData.ContainsKey(recordID))
                {
                    record = cleanData[recordID];
                }
                else if (defectedData.ContainsKey(recordID))
                {
                    record = defectedData[recordID];
                }

                HasSucceeded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Dictionary.getRecord Exception");
                Debug.WriteLine(ex.Message);
            }

            return HasSucceeded;
        }

        /// <summary>
        /// save a specific ObservationRecord item to defect or clean dictionary
        /// </summary>
        /// <param name="isDefected">whether the record is saving to clean or 
        /// defected Dictionary</param>
        /// <param name="record">the record being saved</param>
        public bool SaveRecord(bool isDefected, ObservationRecord record)
        {
            int recordID;
            bool HasSucceeded = false;
            try
            {
                recordID = record.getID();

                //Save record to defect or clean dictioanry
                if (isDefected)
                {
                    defectedData.Add(recordID, record);
                }
                else
                {
                    cleanData.Add(recordID, record);
                }

                HasSucceeded = true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Dictionary.saveRecord Exception");
                Debug.WriteLine(ex.Message);
            }

            return HasSucceeded;
        }

        /// <summary>
        /// Add multiple records inside a dictionary to Clean Dictionary
        /// </summary>
        /// <param name="newDictionary">dictioanry to be added</param>
        /// <returns>whether dictionary was successfully added</returns>
        public bool AddRecordsToCleanDictionary(
                            Dictionary<int, ObservationRecord> newDictionary)
        {
            bool HasSucceeded = false;

            try
            {
                if (newDictionary != null)
                {
                    cleanData.Union(newDictionary);
                }

                HasSucceeded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DictionaryHelper.AddRecordsToCleanDictionary" +
                    "Exception");
                Debug.WriteLine(ex.Message);
            }

            return HasSucceeded;
        }

    }
}
