using DiReCT_wpf.ViewModel;
using System.Activities;
using System.Diagnostics;

namespace DiReCT_wf
{
    public sealed class ReceiveDisasterName : NativeActivity<string>
    {
        public OutArgument<string> NextState { get; set; }
        private string nextState;

        protected override void Execute(NativeActivityContext context)
        {
            Debug.WriteLine("in Record Workflow");

            string currentView = HomeScreenViewModel.GetInstance().CurrentMenuView.WorkFlowName();
            if (currentView == "OtherWorkFlow")  // Landslide Observation's Record
            {
                nextState = "Landslide Record";
            }
            else if (currentView == "RecordWorkFlow")  // Flood Observation's Record
            {
                nextState = "Flood Record";
            }
            
            NextState.Set(context, nextState);

        }
        
    }
}
