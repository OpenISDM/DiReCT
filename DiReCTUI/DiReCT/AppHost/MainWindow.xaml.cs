using System.Activities;
using System.Diagnostics;
using System.Windows;

namespace AppHost
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private WorkflowApplication wfApp;

        public MainWindow()
        {
            InitializeComponent();
        }

        public WorkflowApplication WorkflowApp
        {
            set { wfApp = value; }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Debug.WriteLine("UI Closing");

            wfApp.Aborted = null;
            wfApp.Abort();
        }
    }
}
