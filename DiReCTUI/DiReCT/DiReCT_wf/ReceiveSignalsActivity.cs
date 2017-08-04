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
using DiReCT_wpf.Model;
using DiReCT_wpf.ViewModel;
namespace DiReCT_wf
{
    public sealed class ReceiveSignalsActivity : NativeActivity<string>
    {
        [RequiredArgument]

        public InArgument<string> BookmarkName { get; set; }

        private string bookmarkName;
        public OutArgument<string> NextWorkFlow { get; set; }

        private string nextWorkFlow;
        public OutArgument<string> NextState { get; set; }
        private string nextState;

        private IView menuScreen;

        protected override void Execute(NativeActivityContext context)
        {
            Debug.WriteLine("Menu WorkFLow");
            menuScreen = ServiceLocator.Instance.RepresentationLayerMain.ShowMenuScreen();
            menuScreen.UserEnteredInput += OnInputReady;

              if (HomeScreenViewModel.GetInstance().CurrentMenuView == null)
              {
                  HomeScreenViewModel.GetInstance().ShowMainView();
              }

            nextWorkFlow = HomeScreenViewModel.GetInstance().CurrentMenuView.WorkFlowName();
            if (nextWorkFlow == "OtherWorkFlow") {
                nextWorkFlow = "RecordWorkFlow";
            }          
               
            bookmarkName = context.GetValue(this.BookmarkName);
            context.CreateBookmark(bookmarkName,
                new BookmarkCallback(OnResumeBookmark));

        }
        private void OnInputReady(object sender, EventArgs e)
        {
            Debug.WriteLine("homeScreen onInputReady");

            if (e.GetType() == typeof(MenuItemSelectedEventArgs))
            {
                menuScreen.UserEnteredInput -= OnInputReady;

                MenuItemSelectedEventArgs menuEventArgs = e as MenuItemSelectedEventArgs;
                MenuItem menuItem = menuEventArgs.SelectedItem as MenuItem;
                nextState = menuItem.Lable;
               
                ServiceLocator.Instance.CurrentWorkFlow.ResumeBookmark(bookmarkName, null);
            }
            else if (e.GetType() == typeof(MouseOnViewEventArgs))
            {
                menuScreen.UserEnteredInput -= OnInputReady;
                nextState = "FinalState";
                ServiceLocator.Instance.CurrentWorkFlow.ResumeBookmark(bookmarkName, null);
            }
            else if (e.GetType() == typeof(LoginButtonClickedEventArgs)) {
                menuScreen.UserEnteredInput -= OnInputReady;
                Debug.WriteLine("loginButtonClickedEvent");
                nextState = "FinalState";
                nextWorkFlow = "LoginWorkFlow";
                ServiceLocator.Instance.CurrentWorkFlow.ResumeBookmark(bookmarkName, null);
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
        }
    }
}
