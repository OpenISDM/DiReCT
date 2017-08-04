using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiReCT_wpf.View;
using DiReCT_wpf.ScreenInterface;
using System.ComponentModel;
using System.Diagnostics;

namespace DiReCT_wpf.RepresentaionLayer
{
    public class WPFUserInterface : IRepresentationLayer

    {

        private LoginScreen loginScreen;
        private HomeScreen menuScreen;
        private MainView mainView;
        private OtherView otherView;
        private RecordView recordView;

        public Model.Menu MyMenu { get; }

        public WPFUserInterface()
        {
            Debug.WriteLine("WPFUserInterface");
            loginScreen = new LoginScreen();
            menuScreen = new HomeScreen();
            mainView = new MainView();
            otherView = new OtherView();
            recordView = new RecordView();
            
            MyMenu = Model.Menu.Instance;

        }

        private ScreenBase _CurrentScreen;

        public ScreenBase CurrentScreen
        {
            get
            {
                return _CurrentScreen;
            }
        }


        private void ChangeScreen(ScreenBase control)
        {
            _CurrentScreen = control;
            OnPropertyChanged("CurrentScreen");
        }

        public ScreenBase ShowMenuScreen()
        {
            // menuScreen.UpdateScreen();
            Debug.WriteLine("showMenu");
             ChangeScreen(menuScreen);
            return menuScreen;
           
        }

        public ScreenBase ShowLoginScreen()
        {
            //loginScreen.UpdateScreen();
             ChangeScreen(loginScreen);
            return loginScreen;
           
        }

        private MenuViewBase _CurrentMenuView;

        public MenuViewBase CurrentMenuView
        {
            get
            {
                return _CurrentMenuView;
            }
        }

        private void ChangeMenuView(MenuViewBase control)
        {
            _CurrentMenuView = control;
            OnPropertyChanged("CurrentMenuView");
        }

        public MenuViewBase ShowMainView()
        {
            ChangeMenuView(mainView);
            return mainView;
            

        }
        public MenuViewBase ShowOtherView()
        {
            ChangeMenuView(otherView);
            return otherView;
          
        }
        public MenuViewBase ShowRecordView()
        {
             ChangeMenuView(recordView);
            return recordView;
            
        }

        public void ShowNotificationWindow(string notification)
        {
            Action action = () => {
                NotificationWindow notifacationWindow = new NotificationWindow(notification);
                notifacationWindow.Show();
            };
            System.Windows.Application.Current.Dispatcher.BeginInvoke(action);

        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
