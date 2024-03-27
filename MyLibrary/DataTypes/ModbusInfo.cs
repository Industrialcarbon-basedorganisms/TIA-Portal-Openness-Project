using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibrary.DataTypes
{
    public class ModbusInfo
    {
        public ushort MB_No { get; set; }
        public ushort MB_ADDR { get; set; }
        public byte MODE { get; set; }
        public byte NumberBase { get; set; }
        public int DATA_ADDR { get; set; }
        public ushort DATA_LEN { get; set; }
        public string DataType { get; set; }
        public string Name { get; set; }
       
    }
}
