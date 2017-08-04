using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_wpf.Helpers
{
    class DllFileLoader
    {
        static Assembly Assemblies;

        // copy of record object and its name
        public static dynamic SOPRecord;
        public static string RecordTargetClassName = "SOPFlood";

        public static dynamic CreateAnInstance()
        {
            dynamic type = FindClass(RecordTargetClassName);
            return type;
        }

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
