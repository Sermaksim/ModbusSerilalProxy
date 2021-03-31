using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSerilalProxy.Services
{
    class DeviceEventArgs: EventArgs
    {
        public DeviceEventArgs(int registerAddress, int value, DateTime dateTime)
        {
            RegisterAddress = registerAddress;
            Value = value;
            DateTime = dateTime;
        }
        public int RegisterAddress { get; set; }
        public int Value { get; set; }
        public DateTime DateTime { get; set; }
    }
}
