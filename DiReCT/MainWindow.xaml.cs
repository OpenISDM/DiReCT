using DiReCT.MAN;
using DiReCT.Model;
using DiReCT.Model.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
    /// Interaction logic for MainWindow.xaml. Current Front End is set up to 
    /// test Methods, but the desired final mark up
    /// </summary>
    public partial class MainWindow : Window

    {
        DiReCTCore coreControl;
        public Type CurrentType; // Current type of Record

        public MainWindow()
        {
            InitializeComponent();
            coreControl = DiReCTCore.getInstance();
            CurrentType = DllFileLoader.CreateAnInstance().GetType();
        }

        /// <summary>
        /// Demo function that saves the current waterlevel value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveRecord_Click(object sender, RoutedEventArgs e)
        {
            // Sample records, load from dll
            dynamic testing = DllFileLoader.CreateAnInstance();

            int temp = -1;
            // Check if the input is appropriate, if not change the input to -1
            if (string.IsNullOrWhiteSpace(WaterLevelBox.Text) ||
                !Int32.TryParse(this.WaterLevelBox.Text, out temp))
            {
                WaterLevelBox.Text = "-1";
            }
            testing.WaterLevel = (double)temp;

            // Signal Core event
            WindowOnSavingRecord(testing);

            // Wait 0.5 second before updating dictionary
            /* This will not be the desired method to update dictionary */
            Thread.Sleep(500);
            UpdateDictionary();

            //....
            //To be implemented
            //Implement callback function that automatically updates the UI 
            //once the Record is processed.
            //....
            
        }

        /// <summary>
        /// Demo function that updates the UI dictionary list.
        /// </summary>
        public void UpdateDictionary()
        {
            dynamic[] or = RecordDictionaryManager.getAllCleanRecords();
            String post = "";
            for (int i = 0; i < or.Length; i++)
            {
                post += or[i].RecordID + "          "
                    + (or[i]).WaterLevel + "\n";
            }
            showMessageBlock.Text = post;


            dynamic[] Dor = RecordDictionaryManager.getAllDefectedRecords();
            post = "";
            for (int i = 0; i < Dor.Length; i++)
            {
                post += Dor[i].RecordID + "          "
                    + (Dor[i]).WaterLevel + "\n";
            }
            showDefectedBlock.Text = post;
        }

        /// <summary>
        /// Demo function that closes the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {           
            this.Close();
            coreControl.TerminateProgram();           
        }

        /// <summary>
        /// Demo function that save the current clean dictionary to XML files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            // Set up save file dialog
            saveFileDialog1.Filter = "xml files (*.xml)|*.xml";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            
            if (saveFileDialog1.ShowDialog() == true)
            {
                // if a file is selected              
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {                 
                        // Saving all clean record
                        SerializeHelper.SerializeDictionary(
                                            myStream, 
                                            RecordDictionaryManager.CleanData);
                        myStream.Close();
                }
            }  
        }

        /// <summary>
        /// Demo function that deserialized a XML file and save it to clean record
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetRecord_Click(object sender, RoutedEventArgs e)
        {
            Stream stream = null;
            Dictionary<int, dynamic> dic = null;

            // Set up open file dialog
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "xml files (*.xml)|*.xml";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

            // Check if user opens a file
            if (ofd.ShowDialog() == true)
            {
                // If opened file is correct
                if ((stream = ofd.OpenFile()) != null)
                {
                    using (stream)
                    {
                        // Deserialize the dictionary
                        SerializeHelper.DeserializeDictionary(
                                                        stream, 
                                                        out dic, 
                                                        CurrentType);
                    }

                    // Load saved dictionary to Clean Dictioanry
                    foreach (KeyValuePair<int, dynamic> x in dic)
                    {
                        Console.WriteLine(x.Key);
                        RecordDictionaryManager.CleanData.Add(x.Key, x.Value);
                    }

                    // Reflect new records on screen
                    dynamic[] or = RecordDictionaryManager.getAllCleanRecords();
                    String post = "";
                    for (int i = 0; i < or.Length; i++)
                    {
                        post += or[i].RecordID + "          " + 
                                (or[i]).WaterLevel + "\n";
                    }
                    this.showMessageBlock.Text = post;
                }
            } 
        }

        /// <summary>
        /// Demo function that clean the current clean dictionary 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResetRecord_Click(object sender, RoutedEventArgs e)
        {
            // Clean the entire clean dictionary
            RecordDictionaryManager.CleanData.Clear();

            // Updates the dictionary on the Screen
            dynamic[] or = RecordDictionaryManager.getAllCleanRecords();
            String post = "";
            for (int i = 0; i < or.Length; i++)
            {
                post += or[i].RecordID + "          " +
                        (or[i]).WaterLevel + "\n";
            }
            this.showMessageBlock.Text = post;
        }

        /// <summary>
        /// Minimize the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        #region Event Handlers

        // Event Handler for Saving Record
        public delegate void CallCoreEventHanlder(object obj);
        // Event handler
        public static event CallCoreEventHanlder MainWindowSavingRecord;

        /// <summary>
        /// This method is used to raise MainWindowSavingRecord event. It will
        /// ask a worker thread to process each subscribed method 
        /// </summary>
        /// <param name="obj"></param>
        public static void WindowOnSavingRecord(object obj)
        {           
            MainWindowSavingRecord?.BeginInvoke(obj, null, null);      
        }
        #endregion

    }
}
