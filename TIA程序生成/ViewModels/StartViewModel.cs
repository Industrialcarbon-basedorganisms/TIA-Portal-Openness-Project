using DryIoc;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using TIA程序生成.Common;
using TIA程序生成.Common.Interfaces;
using TIA程序生成.Common.Models;
using TIA程序生成.Events;
using TIA程序生成.Extensions;
using DialogResult = Prism.Services.Dialogs.DialogResult;

namespace TIA程序生成.ViewModels
{
    public class StartViewModel : BindableBase, IDialogAware
    {
        private readonly IDialogHostService dialogHostService;
        private readonly CheckVersion _newCheckVersion;

        public DelegateCommand<string> ExecuteCommand { get; private set; }

        private readonly IEventAggregator aggregator;
        public string Title => "起始页面";

        public event Action<IDialogResult> RequestClose;

        bool _isDisposed = false;

        string readData=string.Empty;
        public StartViewModel(IEventAggregator aggregator, IDialogHostService dialogHostService, CheckVersion checkVersion)
        {
            StartModel = new StartModel();
            this.dialogHostService = dialogHostService;
            this.aggregator = aggregator;
            this._newCheckVersion = checkVersion;
            ExecuteCommand = new DelegateCommand<string>(Execute);
            StartModel.TIAPortalVersion = _newCheckVersion.GetEngineeringVersions();
            StartModel.OpennessVersion = _newCheckVersion.GetOpennessApiVersions();
            AppDomain.CurrentDomain.AssemblyResolve += MyResolver;
            CheakSelectPath();
        }

        private void CheakSelectPath()
        {
            string jsonFilePath = @"SelectPath.json";
            if (File.Exists(jsonFilePath))
            {
                string readJson = File.ReadAllText("SelectPath.json");
                readData = JsonConvert.DeserializeObject<dynamic>(readJson);
            }
            else
            {
                // 将数据转换为 JSON 字符串
                string json = JsonConvert.SerializeObject("C:\\");
                // 保存到文件
                File.WriteAllText("SelectPath.json", json);
                CheakSelectPath();
            }
        }


        private Assembly MyResolver(object sender, ResolveEventArgs args)
        {
            try
            {
                int index = args.Name.IndexOf(',');
                if (index == -1)
                {
                    return null;
                }
                string name = args.Name.Substring(0, index);

                //if (name == "Siemens.Engineering")
                //{
                //    return Assembly.LoadFrom($"{readData}Program Files\\Siemens\\Automation\\Portal V{ConvertToDoubleWithNoTrailingZeros(StartModel.SelectedOpennessVersion)}\\PublicAPI\\V{ConvertToDoubleWithNoTrailingZeros(StartModel.SelectedOpennessVersion)}\\Siemens.Engineering.dll");
                //}
                //else if (name == "Siemens.Engineering.Hmi")
                //{
                //    return Assembly.LoadFrom($"{readData}Program Files\\Siemens\\Automation\\Portal V{ConvertToDoubleWithNoTrailingZeros(StartModel.SelectedOpennessVersion)}\\PublicAPI\\V{ConvertToDoubleWithNoTrailingZeros(StartModel.SelectedOpennessVersion)}\\Siemens.Engineering.dll");
                //}

                if (name == "Siemens.Engineering")
                {
                    return Assembly.LoadFrom($"{readData}\\Portal V{ConvertToDoubleWithNoTrailingZeros(StartModel.SelectedOpennessVersion)}\\PublicAPI\\V{ConvertToDoubleWithNoTrailingZeros(StartModel.SelectedOpennessVersion)}\\Siemens.Engineering.dll");
                }
                else if (name == "Siemens.Engineering.Hmi")
                {
                    return Assembly.LoadFrom($"{readData}\\Portal V{ConvertToDoubleWithNoTrailingZeros(StartModel.SelectedOpennessVersion)}\\PublicAPI\\V{ConvertToDoubleWithNoTrailingZeros(StartModel.SelectedOpennessVersion)}\\Siemens.Engineering.dll");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                _isDisposed = true;
            }

            return null;
        }

        static double ConvertToDoubleWithNoTrailingZeros(string input)
        {
            if (double.TryParse(input, out double number))
            {
                // 检查小数点后是否有非零数字
                if (number % 1 != 0)
                {
                    // 保留原始浮点数
                    return number;
                }
                else
                {
                    // 只取整数部分
                    return Math.Floor(number);
                }
            }
            else
            {
                throw new FormatException("输入字符串不是有效的浮点数。");
            }
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {

        }

        private void Execute(string obj)
        {
            switch (obj)
            {
                case "Cancel": RequestClose?.Invoke(new DialogResult(ButtonResult.No)); break;
                case "Login": Login(); break;
            }
        }

        private async void Login()
        {
            Log.Information($"选择TIA Portal版本为V{StartModel.SelectedOpennessVersion}。");
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            OpennessVersion.Default.Version = StartModel.SelectedOpennessVersion;
        }

        private StartModel startModel;

        public StartModel StartModel
        {
            get { return startModel; }
            set { startModel = value; RaisePropertyChanged(); }
        }
    }
}
