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
 *      RecordDictionaryManager.cs
 * 
 * Abstract:
 *      
 *      This file contains the class that manages the record dictionaries of 
 *      clean and defective records, the clean and defected. The class also 
 *      provides API to access and change the dictionary. 
 *      
 *
 * Authors:
 * 
 *      Hunter Hsieh, hunter205@iis.sinica.edu.tw  
 *      Joe Huang, huangjoe9@gmail.com
 * 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DiReCT.Model
{
    class RecordDictionaryManager
    {
        public static volatile Dictionary<int, dynamic> CleanData;
        public static volatile Dictionary<int, dynamic> DefectiveData;
       
        /// <summary>
        /// Initialize the Dictionary objects
        /// </summary>
        public RecordDictionaryManager()
        {
            CleanData = new Dictionary<int, dynamic>();
            DefectiveData = new Dictionary<int, dynamic>();           
        }

        /// <summary>
        /// Get the record given recordID
        /// </summary>
        /// <param name="recordID">the ID of record</param>
        /// <param name="record">the record to be initialized</param>
        /// <returns>whether record is successfully obtained</returns>
        public bool GetRecord(int recordID, out dynamic record)
        {
            bool HasSucceeded = false;
            record = null;

            try
            {
                // Check record in both dictionary
                if (CleanData.ContainsKey(recordID))
                {
                    record = CleanData[recordID];
                }
                else if (DefectiveData.ContainsKey(recordID))
                {
                    record = DefectiveData[recordID];
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
        /// Save a specific Record item to defective/clean dictionary
        /// </summary>
        /// <param name="isDefected">whether the record is saving to clean or 
        /// defected Dictionary</param>
        /// <param name="record">the record being saved</param>
        public bool SaveRecord(bool isDefective, dynamic record)
        {
            int recordID;
            bool HasSucceeded = false;
           
            try
            {
                recordID = record.RecordID;
                Debug.WriteLine(recordID);
                // Save record to defective or clean dictioanry
                if (isDefective)
                {
                    DefectiveData.Add(recordID, record);
                }
                else
                {
                    CleanData.Add(recordID, record);
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
                            Dictionary<int, dynamic> newDictionary)
        {
            bool HasSucceeded = false;

            try
            {
                if (newDictionary != null)
                {
                    CleanData.Union(newDictionary);
                }

                HasSucceeded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "DictionaryHelper.AddRecordsToCleanDictionary" +
                    "Exception");
                Debug.WriteLine(ex.Message);
            }
            return HasSucceeded;
        }

        //To be deleted
        internal static dynamic[] getAllCleanRecords()
        {
            dynamic[] records = null;
            records = CleanData.Values.ToArray();
            return records;
        }

        internal static dynamic[] getAllDefectedRecords()
        {
            dynamic[] records = null;
            records = DefectiveData.Values.ToArray();
            return records;
        }

    }
}
