using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT.Network
{
    public class ServerReceiveEventArgs : EventArgs
    {
        public ReceiveDataType ReceiveDataType { get; set; }
        public byte[] Data { get; set; }
        public string Record { get; set; }
    }

    public class MessageOutputEventArgs : EventArgs
    {
        public DateTime MessageOutTime { get { return DateTime.Now; } }
        public string Message { get; set; }
    }

    public class ServerEvent
    {
        public event EventHandler ServerReceiveEventHandler;
        public event EventHandler MessageOutputEventHandler;

        public void ReceiveEventCall(ServerReceiveEventArgs e)
        {
            ServerReceiveEventHandler(this, e);
        }

        public void MessageOutputCall(MessageOutputEventArgs e)
        {
            MessageOutputEventHandler(this, e);
        }
    }

    public class ReceiveEventArgs
    {
        public class ControlSignalEventArgs : EventArgs
        {
            public string ControlSignal { get; set; }
        }

        public class DataFlowEventArgs : EventArgs
        {
            public byte[] Data { get; set; }
        }
    }

    public class ReceiveEvent
    {
        public event EventHandler ControlSignalEventHandler;
        public event EventHandler DataFlowEventHandler;

        public void ControlSignalEventCall(
            ReceiveEventArgs.ControlSignalEventArgs e)
        {
            ControlSignalEventHandler(this, e);
        }

        public void DataFlowEventCall(ReceiveEventArgs.DataFlowEventArgs e)
        {
            DataFlowEventHandler(this, e);
        }
    }
}
