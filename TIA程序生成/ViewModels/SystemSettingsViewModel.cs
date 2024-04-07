
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Ioc;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TIA程序生成.Common;
using TIA程序生成.Common.Models;
using TIA程序生成.Extensions;

namespace TIA程序生成.ViewModels
{
    public class SystemSettingsViewModel : NavigationViewModel
    {
        public DelegateCommand<string> ExecuteCommand { get; private set; }
        private readonly IDialogHostService dialogHostService;
        public SystemSettingsViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider)
        {
            SystemSettingsModel=new SystemSettingsModel();
            this.dialogHostService = dialogHostService;
            ExecuteCommand = new DelegateCommand<string>(Execute);
            SystemSettingsModel.SelectedOpennessVersion = ConvertToDoubleWithNoTrailingZeros(OpennessVersion.Default.Version).ToString();
        }

        private void Execute(string obj)
        {
            switch (obj)
            {
                case "SelectPath": SelectPath(); break;
                case "CheakPath":CheakPath();break;
            }
        }

        private void CheakPath()
        {
            //if (File.Exists($"{SystemSettingsModel.SelectPath}Program Files\\Siemens\\Automation\\Portal V{ConvertToDoubleWithNoTrailingZeros(SystemSettingsModel.SelectedOpennessVersion)}\\PublicAPI\\V{ConvertToDoubleWithNoTrailingZeros(SystemSettingsModel.SelectedOpennessVersion)}\\Siemens.Engineering.dll"))
            //{

            //}
            //else
            //{
            //    Log.Error("Siemens.Engineering.dll文件不存在。");
            //    var dialogResult1 = dialogHostService.Question("温馨提示", "Siemens.Engineering.dll文件不存在，请检查TIA Portal安装路径，在系统设置页更改路径。");
            //    return;
            //}

            //if (File.Exists($"{SystemSettingsModel.SelectPath}Program Files\\Siemens\\Automation\\Portal V{ConvertToDoubleWithNoTrailingZeros(SystemSettingsModel.SelectedOpennessVersion)}\\PublicAPI\\V{ConvertToDoubleWithNoTrailingZeros(SystemSettingsModel.SelectedOpennessVersion)}\\Siemens.Engineering.Hmi.dll"))
            //{
            //    var dialogResult1 = dialogHostService.Question("温馨提示", "Siemens.Engineering.dll和Siemens.Engineering.Hmi.dll存在，路径正确。");
            //    // 将数据转换为 JSON 字符串
            //    string json = JsonConvert.SerializeObject(SystemSettingsModel.SelectPath);
            //    // 保存到文件
            //    File.WriteAllText("SelectPath.json", json);
            //    Log.Error($"Siemens.Engineering.dll和Siemens.Engineering.Hmi.dll路径{SystemSettingsModel.SelectPath}修改成功。");
            //}
            //else
            //{
            //    Log.Error("Siemens.Engineering.Hmi.dll文件不存在。");
            //    var dialogResult1 = dialogHostService.Question("温馨提示", "Siemens.Engineering.Hmi.dll文件不存在，请检查TIA Portal安装路径，在系统设置页更改路径");
            //}

            if (File.Exists($"{SystemSettingsModel.SelectPath}\\Portal V{ConvertToDoubleWithNoTrailingZeros(SystemSettingsModel.SelectedOpennessVersion)}\\PublicAPI\\V{ConvertToDoubleWithNoTrailingZeros(SystemSettingsModel.SelectedOpennessVersion)}\\Siemens.Engineering.dll"))
            {

            }
            else
            {
                Log.Error("Siemens.Engineering.dll文件不存在。");
                var dialogResult1 = dialogHostService.Question("温馨提示", "Siemens.Engineering.dll文件不存在，请检查TIA Portal安装路径，在系统设置页更改路径。");
                return;
            }

            if (File.Exists($"{SystemSettingsModel.SelectPath}\\Portal V{ConvertToDoubleWithNoTrailingZeros(SystemSettingsModel.SelectedOpennessVersion)}\\PublicAPI\\V{ConvertToDoubleWithNoTrailingZeros(SystemSettingsModel.SelectedOpennessVersion)}\\Siemens.Engineering.Hmi.dll"))
            {
                var dialogResult1 = dialogHostService.Question("温馨提示", "Siemens.Engineering.dll和Siemens.Engineering.Hmi.dll存在，路径正确，请重新打开软件。");
                // 将数据转换为 JSON 字符串
                string json = JsonConvert.SerializeObject(SystemSettingsModel.SelectPath);
                // 保存到文件
                File.WriteAllText("SelectPath.json", json);
                Log.Error($"Siemens.Engineering.dll和Siemens.Engineering.Hmi.dll路径{SystemSettingsModel.SelectPath}修改成功。");
            }
            else
            {
                Log.Error("Siemens.Engineering.Hmi.dll文件不存在。");
                var dialogResult1 = dialogHostService.Question("温馨提示", "Siemens.Engineering.Hmi.dll文件不存在，请检查TIA Portal安装路径，在系统设置页更改路径");
            }
        }

        private void SelectPath()
        {
            // 创建一个文件夹选择对话框实例
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            // 显示文件夹选择对话框，并等待用户选择文件夹
            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();

            // 如果用户点击了"确定"按钮，则获取用户选择的文件夹路径
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedFolderPath = folderBrowserDialog.SelectedPath;
                SystemSettingsModel.SelectPath = selectedFolderPath;
            }
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

        private SystemSettingsModel systemSettingsModel;

        public SystemSettingsModel SystemSettingsModel
        {
            get { return systemSettingsModel; }
            set { systemSettingsModel = value; RaisePropertyChanged(); }
        }

    }
}
