using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT.Model
{
    /// <summary>
    /// This class is responsible for loading the dll files at runtime. 
    /// </summary>
    public class DllFileLoader
    {
        Assembly Assemblies;
        public ArrayList[] SOPClasses;
        public DllFileLoader()
        {
            Debug.WriteLine("DllFileLoad Initialize");
            //Print all dll class and method
            SOPClasses = LoadLibrary();
            //Initialize SOP to designate class
            string targetClassName = "SOPFlood";
            dynamic SOP = FindClass(targetClassName);
            
        }
        public ArrayList[] LoadLibrary()
        {
            string filepath = Environment.CurrentDirectory;
            Console.WriteLine(filepath);
            DirectoryInfo d = new DirectoryInfo(filepath);
            int dllCount = d.GetFiles("*.dll").Count();

            ArrayList[] Classes = new ArrayList[dllCount];
            ArrayList[] Methods = new ArrayList[dllCount];

            int i = 0;
            foreach (var file in d.GetFiles("*.dll"))
            {
                string dllName = file.ToString().Split('.').First();

                //Writing all the Class + Method name
                Console.WriteLine(file.ToString());
                if (dllName.Contains("SOP"))
                {
                    Classes[i] = GetAllTypesFromDLLstring(dllName);

                    foreach (var ii in Classes[i])
                    {
                        Methods[i] = GetAllTypesFromClass(dllName, ii.ToString());
                        //PRINT--------------------------------------
                        Console.WriteLine("----Classes: " + ii.ToString());
                        foreach (var jj in Methods[i])
                            Console.WriteLine("----Methods: " + jj.ToString());
                        //-------------------------------------------
                    }
                }
                i++;
            }
            return Classes;
        }
        public ArrayList GetAllTypesFromDLLstring(string dllName)
        {
            try
            {
                Assemblies = Assembly.Load(dllName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nError - couldn't obtain assemblies from " + dllName);
                Console.WriteLine("EXCEPTION OUTPUT\n" + ex.Message + "\n" + ex.InnerException);
                ArrayList _Quit = new ArrayList(1);
                _Quit.Add("QUIT");
                return _Quit;
            }

            Type[] AllTypes = Assemblies.GetTypes();

            ArrayList _Temp = new ArrayList();

            foreach (Type t in AllTypes)
            {
                _Temp.Add(t.ToString());
            }

            return _Temp;
        }
        public ArrayList GetAllTypesFromClass(string dllName, string className)
        {
            //Assembly _Assemblies = Assembly.Load(dllName);

            Type _Type;

            ArrayList _Temp = new ArrayList();
            try
            {
                _Type = Assemblies.GetType(className);
                MethodInfo[] _Methods = _Type.GetMethods();

                foreach (MethodInfo meth in _Methods)
                {
                    _Temp.Add(meth.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nError - couldn't obtain methods from " + dllName);
                Console.WriteLine("EXCEPTION:\n" + ex.Message + "\n" + ex.InnerException);
                _Temp.Clear();
                _Temp.Capacity = 1;
                _Temp.Add("QUIT");
            }
            return _Temp;
        }
        public object FindClass(string targetClassName)
        {
            
            try
            {
                Assemblies = Assembly.LoadFrom(targetClassName + ".dll");
                return Assemblies.CreateInstance(targetClassName + "." + targetClassName);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
            
        }
    }
}
