using System.ComponentModel;

namespace DiReCT_wpf.ScreenInterface
{
    public interface IRepresentationLayer : INotifyPropertyChanged
    {
        ScreenBase CurrentScreen { get; }
        MenuViewBase CurrentMenuView { get; }

        ScreenBase ShowLoginScreen();
        ScreenBase ShowMenuScreen();
        MenuViewBase ShowMainView();
        MenuViewBase ShowOtherView();
        MenuViewBase ShowRecordView();

        void ShowNotificationWindow(string notifacation);
    }
}
