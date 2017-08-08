using System;
using System.Windows.Controls;
using DiReCT_wpf.ScreenInterface;
using System.Diagnostics;
using DiReCT_wpf.ViewModel;

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
