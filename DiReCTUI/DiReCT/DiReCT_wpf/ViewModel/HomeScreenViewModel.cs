using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiReCT_wpf.ScreenInterface;
using DiReCT_wpf.Helpers;
using System.Diagnostics;
using DiReCT_wpf.View;

namespace DiReCT_wpf.ViewModel
{
    public class HomeScreenViewModel : ViewModelBase
    {
     
        public override string Name
        {
            get
            {
                return "HomeScreen";
            }
        }
        
        public Model.Menu MyMenu { get; }
        private MainView mainView;
        private OtherView otherView;
        private RecordView recordView;
        private DebrisFlowHistoryView dHistoryView;
        private MenuViewBase currentMenuView;
       
        public MenuViewBase CurrentMenuView
        {
            get
            {
                return currentMenuView;
            }
        }
        public HomeScreenViewModel()
        {
            Debug.WriteLine("HomeScreenViewModel");
            mainView = new MainView();
            otherView = new OtherView();
            recordView = new RecordView();
            dHistoryView = new DebrisFlowHistoryView();
            MyMenu = Model.Menu.Instance;
            LogoutCommand = new RelayCommand(DoLogout);
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
        public MenuViewBase ShowDFHistoryView()
        {
            ChangeMenuView(dHistoryView);
            return dHistoryView;

        }

        private void ChangeMenuView(MenuViewBase control)
        {
            currentMenuView = control;
            RaisePropertyChanged("CurrentMenuView");
        }

        public RelayCommand LogoutCommand { get; set; }
        public RelayCommand ListBoxMenuSelectionChangedCommand { get; set; }
       
        private void DoLogout(object obj)
        {
            Debug.WriteLine("click logout");
            ScreenBase homeScreenView = ServiceLocator.ServiceLocator.Instance.RepresentationLayerMain.ShowMenuScreen();
            homeScreenView.RaiseUserInputReadyEvent(new LoginButtonClickedEventArgs());
        }
        static HomeScreenViewModel _instance;

        public static HomeScreenViewModel GetInstance()
        {
            if (_instance == null)
                _instance = new HomeScreenViewModel();
            return _instance;
        }
       
    }
}
