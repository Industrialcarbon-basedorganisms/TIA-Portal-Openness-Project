using MyLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIA程序生成.Common.Models
{
    public class SystemSettingsModel : BaseDto
    {
        private string selectPath;

        public string SelectPath
        {
            get { return selectPath; }
            set { selectPath = value; OnPropertyChanged(); }
        }

        private string selectedOpennessVersion;

        public string SelectedOpennessVersion
        {
            get { return selectedOpennessVersion; }
            set { selectedOpennessVersion = value; OnPropertyChanged(); }
        }

    }
}
