using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiReCT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        DiReCTCore coreControl;
        public MainWindow()
        {
            InitializeComponent();
            coreControl = DiReCTCore.getInstance();
        }

        private void btnSaveRecord_Click(object sender, RoutedEventArgs e)
        {
            //
            // Do some GUI works, such as refreshing the screen and so on.
            //

            //testing
            InvalidateVisual();
            ObservationRecord testingRecord = new ObservationRecord();
            coreControl.SaveRecord(testingRecord,null,null);
            coreControl.WorkArriveEvent.Set();
            Debug.WriteLine("Saveing Records");          
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }
    }
}
