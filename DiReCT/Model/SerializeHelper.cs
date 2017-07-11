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
        public static bool DeserializeDictionary(
                            Stream stream,
                            out Dictionary<int, ObservationRecord> dictionary,
                            Type type)
        {
            bool HasSucceeded = false;
            dictionary = new Dictionary<int, ObservationRecord>();

            try
            {
                ArrayList list = new ArrayList();

                var serializer = new XmlSerializer(typeof(ArrayList),
                                             new Type[] { type });
                //This requires a global variable to keep track of what type of 
                //record is currently being operated.

                list = (ArrayList)serializer.Deserialize(stream);


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



        public static bool SerializeDictionary(
                            Stream stream,
                            Dictionary<int, ObservationRecord> dictionary)
        {
            bool success = false;
            ArrayList list = new ArrayList();
            try
            {
                var serializer = new XmlSerializer(
                    typeof(ArrayList),
                    new Type[] { dictionary.First().Value.GetType() });

                foreach (KeyValuePair<int, ObservationRecord> x in dictionary)
                {
                    list.Add(x.Value);
                }

                serializer.Serialize(stream, list);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SerializeHelper.Serialize Exception");
                Debug.WriteLine(ex.Message);
            }
            return success;

        }

    }
}
