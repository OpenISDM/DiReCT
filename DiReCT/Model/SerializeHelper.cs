using DiReCT.Model.Observations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DiReCT.Model
{
    class SerializeHelper
    {
        /// <summary>
        /// Deserialize XML file to Dicionary
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="dictionary"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool DeserializeDictionary(
                            Stream stream,
                            out Dictionary<int, ObservationRecord> dictionary,
                            Type type)
        {
            bool HasSucceeded = false;
            dictionary = new Dictionary<int, ObservationRecord>();
            ArrayList list = new ArrayList();

            try
            {
                //Set up deserializer based on the input Type
                var deserializer = new XmlSerializer(typeof(ArrayList),
                                             new Type[] { type });

                //Deserialize the file to arraylist
                list = (ArrayList)deserializer.Deserialize(stream);

                //Add each record in list to dictionary based on ID
                foreach (ObservationRecord record in list)
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
                            Dictionary<int, ObservationRecord> dictionary)
        {
            bool HasSucceeded = false;
            ArrayList list = new ArrayList();

            try
            {
                //Set up the serializer and assume all Types inside the dictionary
                //are the same.
                var serializer = new XmlSerializer(
                    typeof(ArrayList),
                    new Type[] { dictionary.First().Value.GetType() });

                //Add all records in dictionary to arraylist
                foreach (KeyValuePair<int, ObservationRecord> x in dictionary)
                {
                    list.Add(x.Value);
                }

                //Serialize the arraylist
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
