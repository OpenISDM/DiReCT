using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT.Model
{
    /// <summary>
    /// This assign an ID for each record. This class only served to separate 
    /// each records for DEMO purpose and should be replaced with better ID
    /// management system
    /// </summary>
    class IDMananger
    {
        static int CURRENT_MAX;
        static int CURRENT_INDEX;
        static BitArray IDAvailability;
        static HashSet<int> IDStore;

        /// <summary>
        /// Sets up IDMananger
        /// </summary>
        public IDMananger()
        {
            CURRENT_MAX = 10;
            CURRENT_INDEX = 0;
            IDAvailability = new BitArray(CURRENT_MAX);
            IDStore = new HashSet<int>();
            try
            {
                for (int i = 0; i < CURRENT_MAX; i++)
                {
                    IDAvailability[i] = false;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// get an available ID and save it to HashSet
        /// </summary>
        /// <returns></returns>
        public static int getID()
        {
            if (CURRENT_INDEX < CURRENT_MAX)
            {
                IDAvailability[CURRENT_INDEX] = true;
                DateTime currentTime = DateTime.Now;
                int timeConverter = (int)(currentTime.Ticks % 100000);
                String temp = timeConverter.ToString() + CURRENT_INDEX.ToString();
                CURRENT_INDEX++;
                int id = Int32.Parse(temp);
                IDStore.Add(id);
                return id;
            }
            else
            {
                if (expandBitMap())
                {
                    IDAvailability[CURRENT_INDEX] = true;
                    DateTime currentTime = DateTime.Now;
                    int timeConverter = (int)(currentTime.Ticks % 100000);
                    String temp = timeConverter.ToString() + CURRENT_INDEX.ToString();
                    CURRENT_INDEX++;
                    int id = Int32.Parse(temp);
                    IDStore.Add(id);
                    return id;
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Expand the bit array when the index reaches the limit
        /// </summary>
        /// <returns></returns>
        private static bool expandBitMap()
        {
            bool success = false;
            try
            {
                CURRENT_MAX = CURRENT_MAX * 2;

                BitArray tempArray = new BitArray(CURRENT_MAX);
                IDAvailability.CopyTo(tempArray.Cast<bool>().ToArray(), 0);
                IDAvailability = tempArray;

                success = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return success;
        }


        private static IDMananger _instance;

        public static IDMananger getInstance()
        {
            if (_instance == null)
            {
                _instance = new IDMananger();
            }

            return _instance;
        }
    }
}
