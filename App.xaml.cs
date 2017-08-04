using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Diagnostics;

namespace DiReCT
{
    /// <summary>
    /// The main entry point for the application
    /// </summary>

    public partial class DiReCTMainProgram: Application
    {
       
        private static Thread[] moduleThreadHandles = new Thread[(int)ThreadRequiredModule.NumberOfModules];
        //private static Thread UIThreadHandle;
        private static ModuleControlData[] modulesControlData = new ModuleControlData[(int)ThreadRequiredModule.NumberOfModules];
        private static AutoResetEvent[] moduleReadyEvents = new AutoResetEvent[(int)ThreadRequiredModule.NumberOfModules];
        private static bool notInitializationTimeout = true; // Whether initialization processes were completed in time
        private void DiReCTStarup (object sender, StartupEventArgs startupEventArgs)
        {
            Debug.WriteLine("HI");
            // Initialize thread objects of modules
            moduleThreadHandles[(int)ThreadRequiredModule.AAA] = new Thread(AAAModule.AAAInit);
            moduleThreadHandles[(int)ThreadRequiredModule.DM] = new Thread(DMModule.DMInit);
            moduleThreadHandles[(int)ThreadRequiredModule.DS] = new Thread(DSModule.DSInit);
            moduleThreadHandles[(int)ThreadRequiredModule.MAN] = new Thread(MANModule.MANInit);
            moduleThreadHandles[(int)ThreadRequiredModule.RTQC] = new Thread(RTQCModule.RTQCInit);

            // Initialize the thread object of UI thread
            //UIThreadHandle = new Thread(UIMainFunction);
            //UIThreadHandle.SetApartmentState(ApartmentState.STA);

            // Initialize control data of modules
            for (int i=0;i<(int)ThreadRequiredModule.NumberOfModules;i++)
            {
                modulesControlData[i] = new ModuleControlData();
            }

            // Start to execute modules
            try
            {
                moduleThreadHandles[(int)ThreadRequiredModule.AAA].Start(modulesControlData[(int)ThreadRequiredModule.AAA].ThreadParameters);
                moduleThreadHandles[(int)ThreadRequiredModule.DM].Start(modulesControlData[(int)ThreadRequiredModule.DM].ThreadParameters);
                moduleThreadHandles[(int)ThreadRequiredModule.DS].Start(modulesControlData[(int)ThreadRequiredModule.DS].ThreadParameters);
                moduleThreadHandles[(int)ThreadRequiredModule.MAN].Start(modulesControlData[(int)ThreadRequiredModule.MAN].ThreadParameters);
                moduleThreadHandles[(int)ThreadRequiredModule.RTQC].Start(modulesControlData[(int)ThreadRequiredModule.RTQC].ThreadParameters);
            }
            catch (OutOfMemoryException e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine("Some module thread cannot start.");
                goto InitializationFail;
            }

            // Set up the array of Ready events
            for (int i = 0; i < (int)ThreadRequiredModule.NumberOfModules; i++)
            {
                moduleReadyEvents[i] = modulesControlData[i].ThreadParameters.ReadyToWorkEvent;
            }

            System.Threading.Timer timerOfIntialization = new System.Threading.Timer(new TimerCallback(InitializationTimeOutEventHandler),
                                                                                     null,
                                                                                     (int)TimeInterval.LongTime,
                                                                                     Timeout.Infinite);

            while (notInitializationTimeout)
            {
                if (WaitHandle.WaitAll(moduleReadyEvents, (int)TimeInterval.VeryShortTime, true))
                {
                    Debug.WriteLine("Phase 1 initialization of all modules complete!");
                    break;
                }
                else
                {
                    for (int i = 0; i < moduleThreadHandles.Length; i++)
                    {
                        Thread moduleThreadHandle = moduleThreadHandles[i];
                        if (!moduleThreadHandle.IsAlive)
                        {
                            Debug.WriteLine("Phase 1 initialization of " + i + "-th module fails!");
                            notInitializationTimeout = false;
                            goto InitializationFail;
                        }
                    }
                }
            }

            timerOfIntialization.Dispose();

            foreach (ModuleControlData moduleControlData in modulesControlData)
            {
                moduleControlData.ThreadParameters.StartWorkEvent.Set();
            }

            //try
            //{
            //    UIThreadHandle.Start(coreControl);
            //}
            //catch (OutOfMemoryException e)
            //{
            //    Debug.WriteLine(e.Message);
            //    Debug.WriteLine("UI thread can not start!");
            //    goto InitializationFail;
            //}

            //UIThreadHandle.Join();

            Return:
                // singal all created thread to prepare to terminate
                foreach (ModuleControlData moduleControlData in modulesControlData)
                {
                    moduleControlData.ThreadParameters.TerminateWorkEvent.Set();
                }

                // wait for all created threads to be terminated

                foreach (Thread moduleThreadHandle in moduleThreadHandles)
                {
                    if (moduleThreadHandle.ThreadState != System.Threading.ThreadState.Unstarted)
                    {
                        moduleThreadHandle.Join();
                    }
                }
                return;

            InitializationFail:
                goto Return;


            // Turn into UI thread
            MainWindow = new MainWindow();
            MainWindow.Show();
        }
        private static void InitializationTimeOutEventHandler(object state)
        {
            notInitializationTimeout = false;
        }
    }
}
