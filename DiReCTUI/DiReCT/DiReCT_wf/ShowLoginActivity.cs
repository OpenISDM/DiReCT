using System;
using System.Activities;
using DiReCT_wpf.ServiceLocator;
using DiReCT_wpf.ScreenInterface;
using System.Diagnostics;

namespace DiReCT_wf
{
    public sealed class ShowLoginActivity : NativeActivity<string>
    {
        public InArgument<string> BookmarkName { get; set; }
        private string bookmarkName;
        public OutArgument<string> NextWorkFlow { get; set; }
        private string nextWorkFlow;

        private IView loginScreen;

        protected override void Execute(NativeActivityContext context)
        {
            loginScreen = ServiceLocator.Instance.RepresentationLayerMain.ShowLoginScreen();
            loginScreen.UserEnteredInput += OnInputReady;
            bookmarkName = BookmarkName.Get(context);

            context.CreateBookmark(bookmarkName,
                new BookmarkCallback(OnResumeBookmark));

        }

        private void OnInputReady(object sender, EventArgs e)
        {
            if (e.GetType() == typeof(LoginButtonClickedEventArgs))
            {
                loginScreen.UserEnteredInput -= OnInputReady;
                nextWorkFlow = "MenuWorkFlow";
                ServiceLocator.Instance.CurrentWorkFlow.ResumeBookmark(bookmarkName, null);
            }

        }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        public void OnResumeBookmark(NativeActivityContext context, Bookmark bookmark, object obj)
        {
            NextWorkFlow.Set(context, nextWorkFlow);  // in LoginWF, vNextWorkFlow is NextWorkFlow
            context.RemoveAllBookmarks();
        }

    }
}
