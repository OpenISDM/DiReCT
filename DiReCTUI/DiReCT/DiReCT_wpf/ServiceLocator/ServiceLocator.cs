using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiReCT_wpf.ScreenInterface;
using System.Activities;
using DiReCT_wpf.RepresentaionLayer;

namespace DiReCT_wpf.ServiceLocator
{
   
        public class ServiceLocator
        {
            private static volatile ServiceLocator instance;
            private static object syncRoot = new Object();

            private IRepresentationLayer representationLayerMain;
            private WorkflowApplication currentWorkFlow;

            private ServiceLocator() { }

            public static ServiceLocator Instance
            {
                get
                {
                    if (instance == null)
                    {
                        lock (syncRoot)
                        {
                            if (instance == null)
                            {
                                instance = new ServiceLocator();
                                Initialize();
                            }
                        }
                    }
                    return instance;
                }
            }

            private static void Initialize()
            {
                instance.representationLayerMain = new WPFUserInterface();

                //other objectes could also be defined here. like the atm card accepting machine!
            }

            public IRepresentationLayer RepresentationLayerMain
            {
                get { return representationLayerMain; }
            }

            public WorkflowApplication CurrentWorkFlow
            {
                get { return currentWorkFlow; }
                set { currentWorkFlow = value; }
            }
        }
    
}
