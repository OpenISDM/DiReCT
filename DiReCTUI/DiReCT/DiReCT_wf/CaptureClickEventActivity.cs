using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiReCT_wpf.ServiceLocator;
using DiReCT_wpf.ScreenInterface;
using DiReCT_wpf.RepresentaionLayer;
using System.Diagnostics;
using DiReCT_wpf.ViewModel;
namespace DiReCT_wf
{
    public sealed class CaptureClickEventActivity : NativeActivity<string>
    {
        public InArgument<string> BookmarkName { get; set; }
     

        private string bookmarkName;
        public OutArgument<object> Record { get; set; }
        public OutArgument<string> NextWorkFlow { get; set; }
        private string nextWorkFlow;
        public OutArgument<string> NextState { get; set; }
        private string nextState;
        private object recordData;
        private IView recordView;
        private IView menuScreen;
        protected override void Execute(NativeActivityContext context)
        {
            Debug.WriteLine("in Record workflow");
            bookmarkName = context.GetValue(this.BookmarkName);


            string currentView = HomeScreenViewModel.GetInstance().CurrentMenuView.WorkFlowName();
            if (currentView == "RecordWorkFlow")
            {
                recordView = HomeScreenViewModel.GetInstance().ShowRecordView();
            }
            else 
            {
                recordView = HomeScreenViewModel.GetInstance().ShowOtherView();
            }
         
          
          
            menuScreen = ServiceLocator.Instance.RepresentationLayerMain.ShowMenuScreen();
            if(recordView!=null)
            recordView.UserEnteredInput += OnInputReady;
            menuScreen.UserEnteredInput += OnInputReady;

            context.CreateBookmark(bookmarkName,
                new BookmarkCallback(OnResumeBookmark));
        }
        private void OnInputReady(object sender, EventArgs e)
        {
            Debug.WriteLine("in oninputready");

            if (e.GetType() == typeof(SaveButtonClickedEventArgs))
            {
                Debug.WriteLine("SaveButtonClickedEventArgs");
                recordView.UserEnteredInput -= OnInputReady;
                menuScreen.UserEnteredInput -= OnInputReady;

                recordData = e;
                nextWorkFlow = "RecordWorkFlow";
                nextState = "Authenticate Input Data";

                ServiceLocator.Instance.CurrentWorkFlow.ResumeBookmark(bookmarkName, e);
            }
            else if (e.GetType() == typeof(MouseOnMenuEventArgs))
            {
                recordView.UserEnteredInput -= OnInputReady;
                menuScreen.UserEnteredInput -= OnInputReady;
                nextWorkFlow = "MenuWorkFlow";
                nextState = "FinalState";
                ServiceLocator.Instance.CurrentWorkFlow.ResumeBookmark(bookmarkName, e);

            }

        }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        public void OnResumeBookmark(NativeActivityContext context, Bookmark bookmark, object obj)
        {
            context.RemoveAllBookmarks();
            NextState.Set(context, nextState);
            NextWorkFlow.Set(context, nextWorkFlow);
            Record.Set(context, recordData);
        }
    }
}
