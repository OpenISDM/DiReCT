using System;
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
