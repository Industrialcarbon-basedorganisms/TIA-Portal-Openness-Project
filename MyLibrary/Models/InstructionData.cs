using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibrary.Models
{
    public class InstructionData
    {
        [SugarColumn(IsPrimaryKey = false)]
        public string Name { get; set; }
        public string Version { get; set; }
        public string Wire { get; set; }
        public string TemplateValueName { get; set; }
        public string TemplateValueType { get; set; }
        public string TemplateValue { get; set; }
        public string Scope { get; set; }
        public string AddressType { get; set; }
        public string InputOrOutput { get; set; }
    }
}
