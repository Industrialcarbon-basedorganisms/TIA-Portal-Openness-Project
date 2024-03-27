using MyLibrary.Models;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TIA程序生成.Common;
using TIA程序生成.Common.Models;
using TIA程序生成.Extensions;

namespace TIA程序生成.ViewModels
{
    public class LogsViewModel : BindableBase
    {
        private int pageIndex = 1;
        private int pageSize = 15;
        private int totalCount;
        public DelegateCommand<string> ExecuteCommand { get; private set; }
        private readonly IDialogHostService dialogHostService;
        public LogsViewModel(IDialogHostService dialogHostService)
        {
            this.dialogHostService = dialogHostService;
            LogsModel = new LogsModel();
            ExecuteCommand = new DelegateCommand<string>(Execute);
            LogsModel.BeginDate = DateTime.Today.AddDays(-1);
            LogsModel.EndDate = LogsModel.EndTime = LogsModel.BeginTime = DateTime.Now;
            LogsModel.LogsType = "All";
        }

        private void Execute(string obj)
        {
            switch (obj)
            {
                case "DatabaseSearch": pageIndex = 1; DatabaseSearch(); break;
                case "First":
                    pageIndex = 1;
                    DatabaseSearch();
                    break;
                case "Previous":
                    if (pageIndex > 1)
                    {
                        pageIndex--;
                        DatabaseSearch();
                    }
                    break;
                case "Next":
                    int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                    if (pageIndex < totalPages)
                    {
                        pageIndex++;
                        DatabaseSearch();
                    }
                    break;
                case "Last":
                    pageIndex = (int)Math.Ceiling((double)totalCount / pageSize);
                    DatabaseSearch();
                    break;
            }
        }

        private void DatabaseSearch()
        {
            try
            {
                string t1 = $"{LogsModel.BeginDate.ToString("yyyy-MM-dd")}T{LogsModel.BeginTime.ToString("HH:mm:ss")}";
                string t2 = $"{LogsModel.EndDate.ToString("yyyy-MM-dd")}T{LogsModel.EndTime.ToString("HH:mm:ss")}";
                //连接数据库
                SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = "Data Source =" + Environment.CurrentDirectory + "/Project_Logs.db",
                    DbType = SqlSugar.DbType.Sqlite,
                    IsAutoCloseConnection = true,
                });
                var conModels = new List<IConditionalModel>();
                conModels.Add(new ConditionalModel
                {
                    FieldName = "Timestamp",
                    ConditionalType = ConditionalType.GreaterThan,
                    FieldValue = t1
                });
                conModels.Add(new ConditionalModel
                {
                    FieldName = "Timestamp",
                    ConditionalType = ConditionalType.LessThan,
                    FieldValue = t2
                });
                if (!(LogsModel.LogsType == "All") && !(string.IsNullOrWhiteSpace(LogsModel.LogsType)))
                {
                    conModels.Add(new ConditionalModel
                    {
                        FieldName = "Level",
                        ConditionalType = ConditionalType.Like,
                        FieldValue = LogsModel.LogsType
                    });
                }
                LogsModel.LogsList = db.Queryable<Project_Logs>().AS("Project_Logs").Where(conModels).ToPageList(pageIndex, pageSize, ref totalCount);
                pageIndex = (totalCount == 0) ? pageIndex = 0 : pageIndex;
                LogsModel.PageNumText = $"Page {pageIndex} of {(int)Math.Ceiling((double)totalCount / pageSize)}";
                LogsModel.TotalCount = $"共查询到 {totalCount} 条数据";
            }
            catch (Exception ex)
            {
                var dialogResult = dialogHostService.Question("致命错误", $"{ex.Message}");
                Log.Error(ex.ToString());
            }
        }

        private LogsModel logsModel;

        public LogsModel LogsModel
        {
            get { return logsModel; }
            set { logsModel = value; RaisePropertyChanged(); }
        }

    }
}
