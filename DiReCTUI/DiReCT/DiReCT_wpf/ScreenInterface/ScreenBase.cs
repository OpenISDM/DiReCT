using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DiReCT_wpf.ScreenInterface
{
    public class ScreenBase : UserControl, IView
    {
        public event EventHandler UserEnteredInput;

        public event EventHandler ScreenTimedOut;

        public ScreenBase()
        {
        }


        public void RaiseUserInputReadyEvent(EventArgs e)
        {
            EventHandler handler = UserEnteredInput;
            if (handler != null)
            {
                handler(this, e);
            }
        }


    }
}
