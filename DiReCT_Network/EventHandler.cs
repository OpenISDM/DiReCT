using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_Network
{
    public class ServerReceiveEventArgs : EventArgs
    {
        public ReceiveDataType receiveDataType { get; set; }
        public byte[] Data { get; set; }
        public string Record { get; set; }
    }

    public class ServerReceiveEvent
    {
        public event EventHandler ServerReceiveEventHandler;

        public void ReceiveEventCall(ServerReceiveEventArgs e)
        {
            ServerReceiveEventHandler(this, e);
        }
    }
}
