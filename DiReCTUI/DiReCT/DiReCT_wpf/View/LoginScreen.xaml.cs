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
