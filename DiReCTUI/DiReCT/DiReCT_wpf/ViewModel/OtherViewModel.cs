using DiReCT_wpf.Helpers;
using DiReCT_wpf.Model;
using DiReCT_wpf.ScreenInterface;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DiReCT_wpf.ViewModel
{
    public class OtherViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private Microsoft.Maps.MapControl.WPF.Location currentLocation;
        public Microsoft.Maps.MapControl.WPF.Location CurrentLocation
        {

            get
            {
                if (currentLocation == null)
                {

                    return new Microsoft.Maps.MapControl.WPF.Location(23.041, 121.1232);
                }
                else
                {
                    return currentLocation;
                }
            }
            set { }
        }
        public int deathToll { get; set; }
        public int injuryToll { get; set; }
        public ObservableCollection<bool> conditions { get; set; }
        public ObservableCollection<string> landslideCondition { get; set; }
        
        public ObservableCollection<string> checkedLandslideCondition { get; set; }
        public bool houseDamage { get; set; }
        public string houseSelected { get; set; }
        public bool farmDamage { get; set; }
        public string farmSelected { get; set; }
        public bool riverDamage { get; set; }
        public string riverSelected { get; set; }
        public bool groundDamage { get; set; }
        public string groundSelected { get; set; }
        public bool roadDamage { get; set; }
        public string roadSelected { get; set; }
        public DateTime currentDateTime { get; set; }
        public string estimatedTimeStamp { get; set; }
        public string currentLongitude { get; set; }
        public string currentLatitude { get; set; }
        public string currentTimeStamp { get; set; }
        public object photoUploaded { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand UploadCommand { get; set; }
        public RelayCommand SelectionChanged { get; set; }
        public Microsoft.Maps.MapControl.WPF.MapLayer Layer { get; set; }
        public OtherViewModel()
        {
            GeoCoordinateWatcher watcher;
            watcher = new GeoCoordinateWatcher();
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            watcher.TryStart(false, TimeSpan.FromMilliseconds(2000));
            landslideCondition = new ObservableCollection<string> { "House was buried", "Farm was buried", "Road was buried", "River siltation", "Ground cracked" };
            checkedLandslideCondition = new ObservableCollection<string>();
            startclock();
            SaveCommand = new RelayCommand(DoSaveRecord);
            UploadCommand = new RelayCommand(LoadClick);
            SelectionChanged = new RelayCommand(ItemSelectionChanged);
            currentDateTime = DateTime.Now;
            Layer = new Microsoft.Maps.MapControl.WPF.MapLayer();
            deathToll = -1;
            injuryToll = -1;
            houseDamage = false;
            houseSelected = null;
            riverDamage = false;
            riverSelected = null;
            farmDamage = false;
            farmSelected = null;
            groundDamage = false;
            groundSelected = null;
            roadDamage = false;
            roadSelected = null;
        }
        private void DoSaveRecord(object obj)
        {
            dynamic records = RecordGenerator.CreateLandslideRecord(
                deathToll,
                injuryToll,
                checkedLandslideCondition,
                houseDamage,
                houseSelected,
                farmDamage,
                farmSelected,
                riverDamage,
                riverSelected,
                groundDamage,
                groundSelected,
                roadDamage,
                roadSelected
                );

            MenuViewBase recordView = HomeScreenViewModel.GetInstance().ShowOtherView();
            recordView.OnSavingRecord(records);

            LandslideRecord record = new LandslideRecord();
            record.Time = currentDateTime.ToString();
            record.Latitude = currentLatitude.ToString();
            record.Longitude = currentLongitude.ToString();
            record.deathToll = deathToll.ToString();
            record.injuryToll = injuryToll.ToString();
            record.conditions = new ObservableCollection<string>() { null, null, null, null, null };
            photoUploaded = null;
            Layer = new Microsoft.Maps.MapControl.WPF.MapLayer();
            foreach (var e in checkedLandslideCondition.ToList())
            {
                if (e.Contains("House")) record.conditions[0] = e;
                else if (e.Contains("Farm")) record.conditions[1] = e;
                else if (e.Contains("Road")) record.conditions[2] = e;
                else if (e.Contains("River")) record.conditions[3] = e;
                else record.conditions[4] = e;
                checkedLandslideCondition.Remove(e);
            }
            deathToll = -1;
            injuryToll = -1;
            houseDamage = false;
            houseSelected = null;
            riverDamage = false;
            riverSelected = null;
            farmDamage = false;
            farmSelected = null;
            groundDamage = false;
            groundSelected = null;
            roadDamage = false;
            roadSelected = null;
            RaisePropertyChanged("deathToll");
            RaisePropertyChanged("injuryToll");
            RaisePropertyChanged("photoUploaded");
            RaisePropertyChanged("saveRecord");
            RaisePropertyChanged("checkedLandslideCondition");
            RaisePropertyChanged("houseDamage");
            RaisePropertyChanged("riverDamage");
            RaisePropertyChanged("farmDamage");
            RaisePropertyChanged("roadDamage");
            RaisePropertyChanged("groundDamage");
            RaisePropertyChanged("houseSelected");
            RaisePropertyChanged("riverSelected");
            RaisePropertyChanged("farmSelected");
            RaisePropertyChanged("roadSelected");
            RaisePropertyChanged("groundSelected");
            
            recordView.RaiseUserInputReadyEvent(new SaveButtonClickedEventArgs(record));

        }
        private void LoadClick(object sender)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                //imgPhoto.Source = new BitmapImage(new Uri(op.FileName));
                photoUploaded = new BitmapImage(new Uri(op.FileName));
                RaisePropertyChanged("photoUploaded");
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

        public override string Name
        {
            get
            {
                return "Other";
            }
        }
        public void ItemSelectionChanged(object sender)
        {
            houseDamage = false;
            riverDamage = false;
            roadDamage = false;
            farmDamage = false;
            groundDamage = false;

            for(int i=0; i<checkedLandslideCondition.Count(); i++)
            {
                if (checkedLandslideCondition[i] == "House was buried")
                    houseDamage = true;
                else if (checkedLandslideCondition[i] == "Farm was buried")
                    farmDamage = true;
                else if (checkedLandslideCondition[i] == "Road was buried")
                    roadDamage = true;
                else if (checkedLandslideCondition[i] == "River siltation")
                    riverDamage = true;
                else if (checkedLandslideCondition[i] == "Ground cracked")
                    groundDamage = true;
            }
            if (houseDamage == false)
            {
                houseSelected = null;
                RaisePropertyChanged("houseSelected");
            }
            if (riverDamage == false)
            {
                riverSelected = null;
                RaisePropertyChanged("riverSelected");
            }
            if (roadDamage == false)
            {
                roadSelected = null;
                RaisePropertyChanged("roadSelected");
            }
            if (farmDamage == false)
            {
                farmSelected = null;
                RaisePropertyChanged("farmSelected");
            }
            if (groundDamage == false)
            {
                groundSelected = null;
                RaisePropertyChanged("groundSelected");
            }
            RaisePropertyChanged("houseDamage");
            RaisePropertyChanged("farmDamage");
            RaisePropertyChanged("roadDamage");
            RaisePropertyChanged("riverDamage");
            RaisePropertyChanged("groundDamage");
        }
        [Serializable]
        public enum TimeType { AM, PM }
    }
}
