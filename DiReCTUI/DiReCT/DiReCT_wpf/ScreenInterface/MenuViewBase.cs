using System;
using System.Windows.Controls;

namespace DiReCT_wpf.ScreenInterface
{
    public class MenuViewBase : UserControl, IView
    {
        public event EventHandler UserEnteredInput;

        public event EventHandler ScreenTimedOut;

        public MenuViewBase()
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
        public virtual string WorkFlowName()
        {
            return "";
        }
        // Delegate that specify the parameter
        public delegate void SaveRecordEventHanlder(dynamic record);
        // Event Handler for Saving Record
        public static event SaveRecordEventHanlder UIRecordSavingTriggerd;

        /// <summary>
        /// Function to initiate the Saving Record event
        /// </summary>
        /// <param name="index"></param>
        public void OnSavingRecord(dynamic record)
        {
            UIRecordSavingTriggerd?.BeginInvoke(record, null, null);
        }
    }
}
