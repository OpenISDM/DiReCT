using System;
using System.Net.Sockets;

namespace DiReCT.Network
{
    public class ReceiveEventArgs
    {
        public class ControlSignalEventArgs : EventArgs
        {
            public string ControlSignal { get; set; }
            public Socket Socket { get; set; }
        }

        public class DataFlowEventArgs : EventArgs
        {
            public byte[] Data { get; set; }
            public Socket Socket { get; set; }
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
