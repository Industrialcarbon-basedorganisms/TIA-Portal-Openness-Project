using MyLibrary.Dtos;
using MyLibrary.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIA程序生成.Common.Models
{
    public class LogsModel : BaseDto
    {
        private DataTable logs;

        public DataTable Logs
        {
            get { return logs; }
            set { logs = value; OnPropertyChanged(); }
        }

        private List<Project_Logs> logsList;


        public List<Project_Logs> LogsList
        {
            get { return logsList; }
            set { logsList = value; OnPropertyChanged(); }
        }

        private string pageNumText;

        public string PageNumText
        {
            get { return pageNumText; }
            set { pageNumText = value; OnPropertyChanged(); }
        }

        private string totalCount;

        public string TotalCount
        {
            get { return totalCount; }
            set { totalCount = value; OnPropertyChanged(); }
        }

        private DateTime beginDate;

        public DateTime BeginDate
        {
            get { return beginDate; }
            set { beginDate = value; OnPropertyChanged(); }
        }

        private DateTime beginTime;

        public DateTime BeginTime
        {
            get { return beginTime; }
            set { beginTime = value; OnPropertyChanged(); }
        }

        private DateTime endDate;

        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; OnPropertyChanged(); }
        }

        private DateTime endTime;

        public DateTime EndTime
        {
            get { return endTime; }
            set { endTime = value; OnPropertyChanged(); }
        }

        private string logsType;

        public string LogsType
        {
            get { return logsType; }
            set { logsType = value; OnPropertyChanged(); }
        }
    }
}
