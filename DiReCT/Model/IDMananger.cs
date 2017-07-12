using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT.Model
{
    class IDMananger
    {
        static int CURRENT_MAX;
        static int CURRENT_INDEX;
        static BitArray IDAvailability;
        static HashSet<int> IDStore;

        public IDMananger()
        {
            CURRENT_MAX = 10;
            CURRENT_INDEX = 0;
            IDAvailability = new BitArray(CURRENT_MAX);
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
                    return id;
                }
                else
                {
                    return -1;
                }
            }
        }

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
