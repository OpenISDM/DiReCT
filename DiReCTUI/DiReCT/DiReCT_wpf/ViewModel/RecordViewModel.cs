using DiReCT_wpf.Helpers;
using DiReCT_wpf.Model;
using DiReCT_wpf.ScreenInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;

namespace DiReCT_wpf.ViewModel
{
    public class RecordViewModel : ViewModelBase
    {
        public string others { get; set; }
        public bool othersIsChecked { get; set; }
        private Microsoft.Maps.MapControl.WPF.Location currentLocation;
        public Microsoft.Maps.MapControl.WPF.Location CurrentLocation
        {
           
             get{
                if (currentLocation == null)
                {
                    
                    return new Microsoft.Maps.MapControl.WPF.Location(23.041, 121.1232);
                }
                else
                {
                    Debug.WriteLine(currentLocation.Latitude.ToString());
                    return currentLocation;
                }
            }
             set{ }
        }
        public List<CauseOfDisaster> AvailablePresentationObjects {
            get
            {
                for (int i = 0; i < availablePresentationObjects.Count(); i++)
                {
                    availablePresentationObjects[i].OnPropertyChanged("IsChecked");
                }
                return availablePresentationObjects;
            }
            set { }
        }
        private List<CauseOfDisaster> availablePresentationObjects;

        public string currentLongitude { get; set; }
        public string currentLatitude { get; set; }
        public string currentTimeStamp { get; set; }
        private int _waterLevel;
        private readonly object _someValueLock = new object();
        public int waterLevel
        {
            get
            {
                return _waterLevel;
            }
            set
            {
                _waterLevel = value;
                RaisePropertyChanged("waterLevel");
            }
        }
        public RelayCommand SaveCommand { get; set; }
        

        public override string Name
        {
            get
            {
                return "Record";
            }
        }

        public RecordViewModel()
        {
            initializeCheckBox();

            GeoCoordinateWatcher watcher;
            watcher = new GeoCoordinateWatcher();
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            watcher.TryStart(false, TimeSpan.FromMilliseconds(2000));
            startclock();
            SaveCommand = new RelayCommand(DoSaveRecord);
            _waterLevel = 230/2;
            others = "";
            othersIsChecked = false;
        }
        private void initializeCheckBox()
        {
            PossibleCausesOfDisaster possiblecauses = PossibleCausesOfDisaster.GetInstance();
            availablePresentationObjects = new List<CauseOfDisaster>();
            for (int i = 0; i < possiblecauses.causes.Count(); i++)
            {
                availablePresentationObjects.Add(new CauseOfDisaster(i, possiblecauses.causes[i], false));
            }
        }
        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            currentLocation = new Microsoft.Maps.MapControl.WPF.Location(e.Position.Location.Latitude, e.Position.Location.Longitude);
            currentLongitude = e.Position.Location.Longitude.ToString();
            currentLatitude = e.Position.Location.Latitude.ToString();
            RaisePropertyChanged("currentLongitude");
            RaisePropertyChanged("currentLatitude");
            RaisePropertyChanged("CurrentLocation");
        }
        private void startclock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += tickevent;
            timer.Start();
        }
        private void tickevent(Object sender, EventArgs e)
        {
            currentTimeStamp = DateTime.Now.ToString();
            RaisePropertyChanged("currentTimeStamp");
        }
       
        private void DoSaveRecord(object obj)
        {
            ObservableCollection<string> causes = new ObservableCollection<string>();
            for (int i = 0; i < availablePresentationObjects.Count(); i++)
            {
                if (availablePresentationObjects[i].IsChecked == true)
                {
                    availablePresentationObjects[i].IsChecked = false;
                    Debug.WriteLine(availablePresentationObjects[i].Name);
                    causes.Add(availablePresentationObjects[i].Name);
                }
            }

            //FloodRecord record = new FloodRecord();
            //record.Latitude = currentLatitude;
            //record.Longitude = currentLongitude;
            //record.Time = currentTimeStamp;
            //record.WaterLevel = _waterLevel.ToString();
            //record.causes = causes;
            if (_waterLevel.ToString() != "" && causes.Count() != 0)
            {
                _waterLevel = 230/2;
                RaisePropertyChanged("waterLevel");
            }
            othersIsChecked = false;
            others = "";
            RaisePropertyChanged("AvailablePresentationObjects");
            RaisePropertyChanged("others");
            RaisePropertyChanged("othersIsChecked");
            MenuViewBase recordView =HomeScreenViewModel.GetInstance().ShowRecordView();
            //recordView.RaiseUserInputReadyEvent(new SaveButtonClickedEventArgs(record));

            // Initialize the record object
            dynamic record = RecordGenerator.CreateFloodRecord(waterLevel, causes, 
                currentLatitude, currentLongitude, currentTimeStamp);
            // Signal Core for record
            recordView.OnSavingRecord(record);
        }       
    }
}
