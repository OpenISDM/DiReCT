using DiReCT_wpf.ScreenInterface;
using System.Diagnostics;

namespace DiReCT_wpf.View
{
    /// <summary>
    /// MainView.xaml 的互動邏輯
    /// </summary>
    public partial class MainView : MenuViewBase
    {
        public MainView()
        {
            Debug.WriteLine("MainView Initialize");
            InitializeComponent();
        }
        public override string WorkFlowName()
        {
            return "MainWorkFlow";

        }
    }
}
