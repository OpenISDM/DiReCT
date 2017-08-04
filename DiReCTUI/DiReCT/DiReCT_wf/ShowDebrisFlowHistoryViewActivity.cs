using DiReCT_wpf.ScreenInterface;
using DiReCT_wpf.ViewModel;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_wf
{
   
        public sealed class ShowDebrisFlowHistoryViewActivity : NativeActivity<string>
        {
        public OutArgument<string> NextWorkFlow { get; set; }

        private IView mainView;

            protected override void Execute(NativeActivityContext context)
            {
            NextWorkFlow.Set(context, "MenuWorkFlow");
            mainView = HomeScreenViewModel.GetInstance().ShowDFHistoryView();

            }

        }
    
}
