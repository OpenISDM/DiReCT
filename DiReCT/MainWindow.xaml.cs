using DiReCT.MAN;
using DiReCT.Model;
using DiReCT.Model.Observations;
using DiReCT.Model.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        DiReCTCore coreControl;
        Type CurrentType;
        public MainWindow()
        {
            InitializeComponent();
            coreControl = DiReCTCore.getInstance();
            CurrentType = (new Flood()).GetType();
            Debug.WriteLine("MainWindow : " + Thread.CurrentThread.ManagedThreadId);
        }


        /// <summary>
        /// saves the current waterlevel value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveRecord_Click(object sender, RoutedEventArgs e)
        {
            
            //Sample records
            Flood testing = new Flood();
            int temp = -1;
            //Check if the input is appropriate, if not change the input to -1
            if (WaterLevelBox.Text == null ||
                string.IsNullOrWhiteSpace(WaterLevelBox.Text) || 
                !Int32.TryParse(this.WaterLevelBox.Text,out temp))
            {
                WaterLevelBox.Text = "-1";
            }
            testing.WaterLevel = (double)temp;

            //Pass record to Core
            DiReCTCore.CoreSaveRecord(testing, 
                                      null,
                                      null);

            //Wait for record to be saved
            Thread.Sleep(500);

            //Updates the dictionary on the Screen
            ObservationRecord[] or = DictionaryManager.getAllCleanRecords();
            String post = "";
            for (int i = 0; i < or.Length; i++)
            {
                post += or[i].RecordID + "          "
                    + ((Flood)or[i]).WaterLevel + "\n";
            }
            showMessageBlock.Text = post;


            ObservationRecord[] Dor = DictionaryManager.getAllDefectedRecords();
            post = "";
            for (int i = 0; i < Dor.Length; i++)
            {
                post += Dor[i].RecordID + "          "
                    + ((Flood)Dor[i]).WaterLevel + "\n";
            }
            showDefectedBlock.Text = post;
        }

        
        /// <summary>
        /// close the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
            coreControl.TerminateProgram();
            
        }

        /// <summary>
        /// save the current clean dictionary to XML files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            //Set up save file dialog
            saveFileDialog1.Filter = "xml files (*.xml)|*.xml";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            
            if (saveFileDialog1.ShowDialog() == true)
            {
                //if a file is selected
                
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    
                        SerializeHelper.SerializeDictionary(
                                                myStream, 
                                                DictionaryManager.cleanData);
                        myStream.Close();
                }
            }  
        }


        private void btnGetRecord_Click(object sender, RoutedEventArgs e)
        {
            Stream stream = null;
            Dictionary<int, ObservationRecord> dic = null;

            //Set up open file dialog
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "xml files (*.xml)|*.xml";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

            //Check if user opens a file
            if (ofd.ShowDialog() == true)
            {

                //if opened file is correct
                if ((stream = ofd.OpenFile()) != null)
                {
                    using (stream)
                    {
                        SerializeHelper.DeserializeDictionary(
                                                        stream, 
                                                        out dic, 
                                                        CurrentType);
                    }

                    //Load saved dictionary to Clean Dictioanry
                    foreach (KeyValuePair<int, ObservationRecord> x in dic)
                    {
                        Console.WriteLine(x.Key);
                        DictionaryManager.cleanData.Add(x.Key, x.Value);
                    }

                    //Reflect new records on screen
                    ObservationRecord[] or = DictionaryManager.
                                                    getAllCleanRecords();
                    String post = "";
                    for (int i = 0; i < or.Length; i++)
                    {
                        post += or[i].RecordID + "          " + 
                                ((Flood)or[i]).WaterLevel + "\n";
                    }
                    this.showMessageBlock.Text = post;
                }
            } 
        }

        /// <summary>
        /// clean the current clean dictionary 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResetRecord_Click(object sender, RoutedEventArgs e)
        {
            
            DictionaryManager.cleanData.Clear();

            //Updates the dictionary on the Screen
            ObservationRecord[] or = DictionaryManager.
                                                    getAllCleanRecords();
            String post = "";
            for (int i = 0; i < or.Length; i++)
            {
                post += or[i].RecordID + "          " +
                        ((Flood)or[i]).WaterLevel + "\n";
            }
            this.showMessageBlock.Text = post;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
