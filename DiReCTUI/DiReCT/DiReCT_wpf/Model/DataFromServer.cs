using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiReCT_wpf.Model
{
    public class DataFromServer
    {
        public DataFromServer()
        {
            data = new FakeData();
            data.addLocation(25.068133, 121.605766);
            data.addLocation(25.046907, 121.557357);
            data.addLocation(25.027038, 121.565511);
            data.addLocation(25.045391, 121.611817);
        }
        public FakeData data { get; set; }
    }
}
