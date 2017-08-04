using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DiReCT_wpf.ServiceLocator;
using System.Activities;
using DiReCT_wf;


namespace AppHost
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        private string NextWorkFlow = "LoginWorkFlow";
        private object locker = new object();
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Thread.CurrentThread.Name = "Main thread";
            MainWindow window = new MainWindow();
            window.DataContext = ServiceLocator.Instance.RepresentationLayerMain;
            window.Show();

            StartWorkFlow();
        }

        private void StartWorkFlow()
        {
            WorkflowApplication wfApp = InitiateWorkFlow();
            ServiceLocator.Instance.CurrentWorkFlow = wfApp;
            Action action = () => StartWorkFlow();

            wfApp.Completed = delegate (WorkflowApplicationCompletedEventArgs ev)
            {
                ReadOutputs(NextWorkFlow, ev);
                Dispatcher.Invoke(action); //call startworkflow() through main thread
            };

            wfApp.Aborted = delegate (WorkflowApplicationAbortedEventArgs ev)
            {
                NextWorkFlow = "LoginWorkFlow";
                Dispatcher.Invoke(action); //call startworkflow() through main thread
            };

            wfApp.OnUnhandledException = delegate (WorkflowApplicationUnhandledExceptionEventArgs ev)
            {
                Dispatcher.Invoke(action); //call startworkflow() through main thread
                Console.WriteLine(ev.UnhandledException.ToString());
                return UnhandledExceptionAction.Terminate;
            };

            wfApp.Idle = delegate (WorkflowApplicationIdleEventArgs ev)
            {
                //nothing. workflow is waiting screen input, service call, or db call
            };

            wfApp.Run();
        }

        private WorkflowApplication InitiateWorkFlow()
        {
            WorkflowApplication wfApp = null;
            Dictionary<string, object> inputs;
            while (wfApp == null)
            {
                switch (NextWorkFlow)
                {
                    case "LoginWorkFlow":
                        inputs = new Dictionary<string, object>() { };
                        wfApp = new WorkflowApplication(new LoginWorkFlow(), inputs);
                        break;
                    case "MenuWorkFlow":
                        inputs = new Dictionary<string, object>() { };
                        wfApp = new WorkflowApplication(new MenuWorkFlow(), inputs);
                        break;
                    case "RecordWorkFlow":
                        inputs = new Dictionary<string, object>() { };
                        wfApp = new WorkflowApplication(new RecordWorkFlow(), inputs);
                        break;
                    case "MainWorkFlow":
                        inputs = new Dictionary<string, object>() { };
                        wfApp = new WorkflowApplication(new MainWorkFlow(), inputs);
                        break;
                    case "OtherWorkFlow":
                        inputs = new Dictionary<string, object>() { };
                        wfApp = new WorkflowApplication(new OtherWorkFlow(), inputs);
                        break;
                    default:
                        //Trace("Couldn't Identify workflow");
                        NextWorkFlow = "MenuWorkFlow";
                        break;
                }
            }

            return wfApp;
        }
        private void ReadOutputs(string workflowName, WorkflowApplicationCompletedEventArgs e)
        {
            NextWorkFlow = e.Outputs["ServiceArg"].ToString();

        }
    }
}
