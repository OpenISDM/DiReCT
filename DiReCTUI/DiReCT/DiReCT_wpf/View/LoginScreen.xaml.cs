using DiReCT_wpf.ScreenInterface;
using DiReCT_wpf.ViewModel;
using System.Diagnostics;

namespace DiReCT_wpf.View
{
    /// <summary>
    /// UserLogin.xaml 的互動邏輯
    /// </summary>
    public partial class LoginScreen : ScreenBase
    {
        public AuthenticationViewModel AuthViewModel;
        public LoginScreen()
        {
            Debug.WriteLine("LoginScreen Initialize");
            InitializeComponent();
            AuthViewModel = AuthenticationViewModel.GetInstance();
            this.DataContext = AuthViewModel;
        }
    }
}
