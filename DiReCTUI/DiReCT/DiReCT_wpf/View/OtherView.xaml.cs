using System;
using System.Collections.Generic;
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
using DiReCT_wpf.ScreenInterface;
using System.Diagnostics;
using DiReCT_wpf.Model;
using System.Collections.ObjectModel;
using System.Device.Location;
using Microsoft.Maps.MapControl.WPF;
using System.Windows.Threading;
using Microsoft.Win32;
using DiReCT_wpf.ViewModel;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace DiReCT_wpf.View
{
    /// <summary>
    /// OtherView.xaml 的互動邏輯
    /// </summary>
    public partial class OtherView : MenuViewBase
    {
        public override string WorkFlowName()
        {
            return "OtherWorkFlow";

        }
        public OtherViewModel otherViewModel;
        private Microsoft.Maps.MapControl.WPF.MapLayer layer;
        public OtherView()
        {
            otherViewModel = new OtherViewModel();

            InitializeComponent();


            this.DataContext = otherViewModel;
            otherViewModel.PropertyChanged += Save_PropertyChanged;
            isFirstPress = true;
            layer = otherViewModel.Layer;
            mapView.Children.Add(otherViewModel.Layer);

        }
      
        void Save_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "saveRecord":
                    Debug.WriteLine("clear layer");
                    mapView.Children.Remove(layer);
                    mapView.Children.Add(otherViewModel.Layer);
                    layer = otherViewModel.Layer;
                break;
            }
        }


        private Point currentPoint;
        private bool isFirstPress;

        void mapViewMouseUp(Object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                currentPoint = e.GetPosition(this);
            Debug.WriteLine("mapViewmouseDown");
            isFirstPress = true;

        }
        private void mapViewMouseMove(object sender, MouseEventArgs e)
        {   //isFirstPress = true;
            if (e.RightButton == MouseButtonState.Pressed)
            {
                //e.Handled = true;
                if (isFirstPress == false)
                {
                    Debug.WriteLine("draw");
                    Microsoft.Maps.MapControl.WPF.MapPolyline line = new Microsoft.Maps.MapControl.WPF.MapPolyline();
                    line.Locations = new Microsoft.Maps.MapControl.WPF.LocationCollection();
                    line.Locations.Add(mapView.ViewportPointToLocation(currentPoint));
                    line.Locations.Add(mapView.ViewportPointToLocation(e.GetPosition(this)));
                    SolidColorBrush redBrush = new SolidColorBrush();
                    redBrush.Color = Colors.Red;
                    line.Stroke = redBrush;
                    line.Opacity = 0.5;
                    line.Visibility = Visibility.Visible;
                    line.StrokeThickness = 4;
                    otherViewModel.Layer.Children.Add(line);
                }
                currentPoint = e.GetPosition(this);
                isFirstPress = false;

            }
        }

    }
    
}
