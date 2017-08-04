using DiReCT_wpf.Helpers;
using DiReCT_wpf.ScreenInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_wpf.ViewModel
{
    public class AuthenticationViewModel : ViewModelBase
    {
        public override string Name
        {
            get
            {
                return "Login";
            }
        }
        public RelayCommand LoginCommand { get; set; }
        public RelayCommand LogoutCommand { get; set; }

        public AuthenticationViewModel()
        {
            LoginCommand = new RelayCommand(DoLogin);
            LogoutCommand = new RelayCommand(DoLogout, CanDoLogout);
        }
        public bool isAuthenticated = false;
        public bool IsAuthenticated
        {
            get { return isAuthenticated; }
            set
            {
                if (value != isAuthenticated)
                {
                    isAuthenticated = value;
                    RaisePropertyChanged("IsAuthenticated");
                    RaisePropertyChanged("IsNotAuthenticated");
                }
            }
        }
        public bool IsNotAuthenticated
        {
            get
            {
                return !IsAuthenticated;
            }
        }
        private bool CanDoLogout(object obj)
        {
            return isAuthenticated;
        }

        private void DoLogout(object obj)
        {
            Debug.WriteLine("Logout");
            IsAuthenticated = false;
        }

        private void DoLogin(object obj)
        {
            ScreenBase loginView = ServiceLocator.ServiceLocator.Instance.RepresentationLayerMain.ShowLoginScreen();
            loginView.RaiseUserInputReadyEvent(new LoginButtonClickedEventArgs());
            IsAuthenticated = true;
        }
        static AuthenticationViewModel _instance;

        public static AuthenticationViewModel GetInstance()
        {
            if (_instance == null)
                _instance = new AuthenticationViewModel();
            return _instance;
        }
    }
}
