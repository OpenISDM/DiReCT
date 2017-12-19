using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiReCT_Network;

namespace DiReCT_Server
{
    class Program
    {
        private static NetworkServer NetworkServer;
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += 
                new EventHandler(CurrentDomain_ProcessExit);

            NetworkServer = new NetworkServer(4444);
            NetworkServer.Event.ServerReceiveEventHandler += 
                new EventHandler(SaveReceiveData);
            NetworkServer.Event.MessageOutputEventHandler +=
                new EventHandler(OutputMessage);
            NetworkServer.Start();
        }

        static void SaveReceiveData(object sender, EventArgs e)
        {
            //Save record or file code
        }

        static void OutputMessage(object sender, EventArgs e)
        {
            MessageOutputEventArgs message = 
                (sender as MessageOutputEventArgs);
            Console.WriteLine(message.MessageOutTime.ToString() + " :" +
                message.Message);
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            NetworkServer.Dispose();
            Console.WriteLine("exit");
        }
    }
}
