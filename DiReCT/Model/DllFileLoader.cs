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
 *      Model/DllFileLoader.cs
 * 
 * Abstract:
 *      
 *      This class contains class that load Dll files at runtime. This 
 *      will allow external record type class to be added. 
 *
 * Authors:
 * 
 *      Hunter Hsieh, hunter205@iis.sinica.edu.tw  
 *      Joe Huang, huangjoe9@gmail.com
 * 
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DiReCT.Model
{
    /// <summary>
    /// This class is responsible for loading the dll files at runtime. 
    /// </summary>
    public class DllFileLoader
    {
        static Assembly Assemblies;
        public ArrayList[] SOPClasses;
        // copy of SOP object and its name
        public static dynamic SOP;
        public static string SOPTargetClassName;
        // copy of record object and its name
        public static dynamic SOPRecord;      
        const string RecordTargetClassName = "SOPFlood"; 

        public DllFileLoader()
        {
            Debug.WriteLine("DllFileLoad Initialize");
            // Print all dll class and method
            // SOPClasses = LoadLibrary();
            // Initialize SOP Record to designate class           
            SOPRecord = FindClass(RecordTargetClassName);

            // Initialzie SOP to designated class
            SOPTargetClassName = "SOP";
            SOP = FindClass(SOPTargetClassName);
        }

        /// <summary>
        /// API for other class to obtain the current SOP class
        /// </summary>
        /// <returns></returns>
        public static dynamic GetSOP()
        {
            return SOP;
        }

        /// <summary>
        /// API for other class to obtain the current record class instance
        /// </summary>
        /// <returns></returns>
        public static dynamic CreateAnInstance()
        {
            dynamic type = FindClass(RecordTargetClassName);
            return type;
        }
        /// <summary>
        /// This function returns an Instance of the object contained in dll 
        /// file
        /// </summary>
        /// <param name="targetClassName">the name of the class</param>
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

        /// <summary>
        /// This function loads all the dll files in DEBUG folder and print 
        /// the class and type name to the Console.
        /// </summary>
        /// <returns></returns>
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

                // Writing all the Class + Method name
                Console.WriteLine(file.ToString());
                if (dllName.Contains("SOP"))
                {
                    Classes[i] = GetAllTypesFromDLLstring(dllName);

                    foreach (var ii in Classes[i])
                    {
                        Methods[i] = GetAllTypesFromClass(dllName, 
                                                          ii.ToString());
                        // PRINT--------------------------------------
                        Console.WriteLine("----Classes: " + ii.ToString());
                        foreach (var jj in Methods[i])
                            Console.WriteLine("----Methods: " + jj.ToString());
                        // -------------------------------------------
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
                Console.WriteLine(
                    "\n\nError - couldn't obtain assemblies from " +
                    dllName);
                Console.WriteLine("EXCEPTION OUTPUT\n" +
                    ex.Message + "\n" +
                    ex.InnerException);
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
                Console.WriteLine("\n\nError - couldn't obtain methods from " +
                                  dllName);
                Console.WriteLine("EXCEPTION:\n" + ex.Message + "\n" + 
                                  ex.InnerException);
                _Temp.Clear();
                _Temp.Capacity = 1;
                _Temp.Add("QUIT");
            }
            return _Temp;
        }

        
    }
}
