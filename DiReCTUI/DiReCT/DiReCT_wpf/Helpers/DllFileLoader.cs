using System;
using System.Diagnostics;
using System.Reflection;

namespace DiReCT_wpf.Helpers
{
    class DllFileLoader
    {
        static Assembly Assemblies;

        // copy of record object and its name
        public static dynamic SOPRecord;        

        /// <summary>
        /// Create a flood record object
        /// </summary>
        /// <returns></returns>
        public static dynamic CreateAnFloodInstance()
        {
            dynamic type = FindClass("SOPFlood");
            return type;
        }
        
        /// <summary>
        /// Create a landslide record object
        /// </summary>
        /// <returns></returns>
        public static dynamic CreateALandslideInstance()
        {          
            dynamic type = FindClass("SOPLandslides");
            return type;
        }

        /// <summary>
        /// This function is to create an instance from dll file. The class name 
        /// must match the dll name in order to do so. For example, 
        /// SOPFlood.SOPFlood
        /// </summary>
        /// <param name="targetClassName"></param>
        /// <returns></returns>
        public static object FindClass(string targetClassName)
        {
            try
            {
                Assemblies = Assembly.LoadFrom(targetClassName + ".dll");
                return Assemblies.CreateInstance(targetClassName +
                                                "." + targetClassName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }

        }
    }
}
