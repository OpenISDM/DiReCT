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

namespace DiReCT_wpf.View
{
    /// <summary>
    /// NotificationWindow.xaml 的互動邏輯
    /// </summary>
    public partial class NotificationWindow : Window
    {

        // public string Notification { get; set; }
        public NotificationWindow(string notification)
        {
            InitializeComponent();
            Debug.WriteLine("notification text=" + notification);
            notifacationTextBlock.Text = notification;
        }
    }
}
