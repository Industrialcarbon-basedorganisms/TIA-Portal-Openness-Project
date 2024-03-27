using MyLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIA程序生成.Common.Models
{
    public class StartModel : BaseDto
    {
        private List<string> tiaPortalVersion;

        public List<string> TIAPortalVersion
        {
            get { return tiaPortalVersion; }
            set { tiaPortalVersion = value; OnPropertyChanged(); }
        }

        private List<string> opennessVersion;

        public List<string> OpennessVersion
        {
            get { return opennessVersion; }
            set { opennessVersion = value; OnPropertyChanged(); }
        }

        private string selectedOpennessVersion;

        public string SelectedOpennessVersion
        {
            get { return selectedOpennessVersion; }
            set { selectedOpennessVersion = value; OnPropertyChanged(); }
        }
    }
}
