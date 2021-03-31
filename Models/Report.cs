using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSerilalProxy.Models
{
    class Report
    {
        public int ID { get; set; }
        public int RegisterAddress { get; set; }
        public int Value { get; set; }
        public DateTime DateTime { get; set; }
    }
}
