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
 *      Model/SerializeHelper.cs
 * 
 * Abstract:
 *      
 *      This file contains class that serialize and deserialize the record 
 *      dictionaries. 
 *
 * Authors:
 * 
 *      Hunter Hsieh, hunter205@iis.sinica.edu.tw  
 *      Joe Huang, huangjoe9@gmail.com
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DiReCT.Model
{
    class SerializeHelper
    {
        /// <summary>
        /// Deserialize XML file to Dicionary with integer as key.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="dictionary"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool DeserializeDictionary(
                            Stream stream,
                            out Dictionary<int, dynamic> dictionary,
                            Type type)
        {
            bool HasSucceeded = false;
            dictionary = new Dictionary<int, dynamic>();
            ArrayList list = new ArrayList();

            try
            {
                // Set up deserializer based on the input Type
                var deserializer = new XmlSerializer(typeof(ArrayList),
                                             new Type[] { type });
                // Deserialize the file to arraylist
                list = (ArrayList)deserializer.Deserialize(stream);
                // Add each record in list to dictionary based on ID
                foreach (dynamic record in list)
                {
                    dictionary.Add(record.RecordID, record);
                }

                HasSucceeded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SerializeHelper.Dserialize Exception");
                Debug.WriteLine(ex.Message);
            }

            return HasSucceeded;
        }


        /// <summary>
        /// Serialize dictionary to specified location as XML 
        /// </summary>
        /// <param name="stream">the output location</param>
        /// <param name="dictionary">the dictionary to be serialize</param>
        /// <returns></returns>
        public static bool SerializeDictionary(
                            Stream stream,
                            Dictionary<int, dynamic> dictionary)
        {           
            bool HasSucceeded = false;
            ArrayList list = new ArrayList();

            try
            {
                // Set up the serializer and assume all Types inside the 
                // dictionary are the same.
                var serializer = new XmlSerializer(
                    typeof(ArrayList),
                    new Type[] { dictionary.First().Value.GetType() });

                // Add all records in dictionary to arraylist
                foreach (KeyValuePair<int, dynamic> x in dictionary)
                {
                    list.Add(x.Value);
                }

                // Serialize the arraylist
                serializer.Serialize(stream, list);
                HasSucceeded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SerializeHelper.Serialize Exception");
                Debug.WriteLine(ex.Message);
            }
            return HasSucceeded;
        }

    }
}
