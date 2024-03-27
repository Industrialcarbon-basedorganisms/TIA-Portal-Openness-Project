using MyLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIA程序生成.Common.DataType
{
    public class ConvertType : BaseDto
    {
        private string dataType;

        public string DataType
        {
            get { return dataType; }
            set { dataType = value; OnPropertyChanged(); }
        }

        private string data;

        public string Data
        {
            get { return data; }
            set { data = value; OnPropertyChanged(); }
        }
    }
}
