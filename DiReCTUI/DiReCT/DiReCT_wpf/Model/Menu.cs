using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_wpf.Model
{
    public class Menu
    {
        public Menu()
        {
            MenuItemList = new List<MenuItem>();
            MenuItem mainView = new MenuItem("Today's Task");
            MenuItemList.Add(mainView);
            MenuItem otherView = new MenuItem("Landslide Observation");
            MenuItemList.Add(otherView);
            MenuItem recordView = new MenuItem("Flood Observation");
            MenuItemList.Add(recordView);
            MenuItem debrisFlowHistoryView = new MenuItem("History");
            MenuItemList.Add(debrisFlowHistoryView);

        }

        public List<MenuItem> MenuItemList { get; }
        static Menu instance;
        public static Menu Instance
        {
            get
            {
                if (instance == null)
                    instance = new Menu();
                return instance;
            }

        }
    }
}
