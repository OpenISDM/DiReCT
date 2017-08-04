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
using DiReCT_wpf.ViewModel;
using DiReCT_wpf.RepresentaionLayer;

namespace DiReCT_wpf.View
{
    /// <summary>
    /// HomeScreen.xaml 的互動邏輯
    /// </summary>
    public partial class HomeScreen : ScreenBase
    {
        public HomeScreenViewModel homeViewModel;
       
        public HomeScreen()
        {
            InitializeComponent();
            homeViewModel = HomeScreenViewModel.GetInstance();          
            this.DataContext = homeViewModel;
        }

        private void ListBoxMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          
            Object seletedMenuItem = ListBoxMenu.SelectedItem;
            if (seletedMenuItem != null)
            {

                RaiseUserInputReadyEvent(new MenuItemSelectedEventArgs(seletedMenuItem));

            }
        }

        private void CurrentMenuView_MouseEnter(object sender, EventArgs e)
        {
            Debug.WriteLine("Mouse on currentView");
            RaiseUserInputReadyEvent(new MouseOnViewEventArgs());

        }
        private void CurrentMenuView_MouseLeave(object sender, EventArgs e)
        {
            Debug.WriteLine("Mouse on menu");
            RaiseUserInputReadyEvent(new MouseOnMenuEventArgs());

        }

        private void ListBoxMenu_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Debug.WriteLine("Mouse on menu");
            RaiseUserInputReadyEvent(new MouseOnMenuEventArgs());
        }
    }
}
