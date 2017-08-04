using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiReCT_wpf.ServiceLocator;

namespace DiReCT_wf
{
    public sealed class ShowNotificationActivity : NativeActivity<string>
    {
        public InArgument<string> Notification { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            string notification = context.GetValue(this.Notification);
            ServiceLocator.Instance.RepresentationLayerMain.ShowNotificationWindow(notification);


        }
    }
}
