using System;

namespace DiReCT_wpf.ScreenInterface
{
    public interface IView
    {

        event EventHandler UserEnteredInput;
        event EventHandler ScreenTimedOut;

    }
    public class MenuItemSelectedEventArgs : EventArgs
    {
        public MenuItemSelectedEventArgs(Object item)
        {
            SelectedItem = item;
        }
        public Object SelectedItem;
    }
    public class LoginButtonClickedEventArgs : EventArgs
    {
    }

    public class MouseOnViewEventArgs : EventArgs
    {
    }
    public class MouseOnMenuEventArgs : EventArgs
    {
    }
    public class SaveButtonClickedEventArgs : EventArgs
    {
        public SaveButtonClickedEventArgs(Object record)
        {
            SavedRecord = record;

        }
        public Object SavedRecord;
    }
}
