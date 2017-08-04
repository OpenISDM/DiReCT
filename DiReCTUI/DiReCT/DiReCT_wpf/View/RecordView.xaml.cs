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

namespace DiReCT_wpf.View
{
    /// <summary>
    /// RecordView.xaml 的互動邏輯
    /// </summary>
    public partial class RecordView : MenuViewBase
    {
        public RecordViewModel recordViewModel;
        public RecordView()
        {
            InitializeComponent();
            recordViewModel = new RecordViewModel();
            this.DataContext = recordViewModel;
        }
        private void Click_On_Save_Button(object sender, EventArgs e) {
        }
        public override string WorkFlowName()
        {
            return "RecordWorkFlow";

        }
    }
}
