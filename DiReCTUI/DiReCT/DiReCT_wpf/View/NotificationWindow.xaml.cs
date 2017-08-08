using System.Diagnostics;
using System.Windows;

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
