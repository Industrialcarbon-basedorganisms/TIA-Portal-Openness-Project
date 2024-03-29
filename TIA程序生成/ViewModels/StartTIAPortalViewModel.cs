using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using SqlSugar;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using TIA程序生成.Common;
using TIA程序生成.Common.Interfaces;
using TIA程序生成.Common.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using TIA程序生成.Extensions;
using MyLibrary.Models;
using System.Collections;
using System.Security.Cryptography;
using MyLibrary.DataTypes;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Windows.Documents;
using ImTools;
using Siemens.Engineering.Umac;
using System.IO;
using MaterialDesignThemes.Wpf.Transitions;
using Prism.Ioc;
using Prism.Regions;
using System.ComponentModel.DataAnnotations;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW;
using Prism.Events;
using TIA程序生成.Events;
using Serilog;

namespace TIA程序生成.ViewModels
{
    public class StartTIAPortalViewModel : NavigationViewModel
    {
        public DelegateCommand<string> ExecuteCommand { get; private set; }
        public DelegateCommand<object> ExecuteDelCommand { get; private set; }

        private static string SelectedOpennessVersion = string.Empty;
        private readonly CheckVersion _newCheckVersion;
        private readonly IDialogHostService dialogHostService;
        private readonly TIA_Portal _newTIAPortal;
        private static TiaPortal instTIA;
        private static Project projectTIA;
        private static Device plcDevice;
        private static PlcSoftware plcSoftware;
        private static PlcBlock dataBlock;
        ushort mb_No;

        public StartTIAPortalViewModel(IDialogHostService dialogHostService, TIA_Portal newTIAPortal, CheckVersion checkVersion, IContainerProvider provider)
            : base(provider)
        {
            this.dialogHostService = dialogHostService;
            _newCheckVersion = checkVersion;
            StartTIAPortalModel = new StartTIAPortalModel();
            StartTIAPortalModel.ModbusInfo = new ObservableCollection<ModbusInfo>();
            ExecuteCommand = new DelegateCommand<string>(Execute);
            ExecuteDelCommand = new DelegateCommand<object>(ExecuteDel);
            _newTIAPortal = newTIAPortal;
            SelectedOpennessVersion = OpennessVersion.Default.Version;
            GetTiaPortalProcess();
            InitializationCheck();

        }


        private async void Execute(string obj)
        {
            switch (obj)
            {
                case "OpenTIAPortal": OpenTIAPortal(); break;
                case "CreateProject": CreateProject(); break;
                case "SelectPath": SelectPath(); break;
                case "SelectFile": SelectFile(); break;
                case "OpenFile": OpenFile(); break;
                case "CreatePLC": CreateData(); break;
                case "GetTiaPortalProcess": GetTiaPortalProcess(); break;
                case "ConnectTIAprj": ConnectTIAprj(); break;
                case "GetTiaPortalDevices": GetTiaPortalDevices(); break;
                case "ConnectTiaPortalDevices": ConnectTiaPortalDevices(); break;
                case "ExportRegularBlock": ExportRegularBlock(); break;
                case "BlockExportPath": BlockExportPath(); break;
                case "ImportBlocks": ImportBlocks(); break;
                case "ImportPrograms": ImportPrograms1(); break;
                case "SelectImportBlockPath": SelectImportBlockPath(); break;
                case "CreateModbusData": CreateModbusData(); break;
                case "CleanModbusData": CleanModbusData(); break;
            }
        }


        private async void ImportPrograms1()
        {
            var dialogResult = await dialogHostService.Question("温馨提示", $"确定要生成Modbus程序吗？");
            if (dialogResult.Result != Prism.Services.Dialogs.ButtonResult.OK) return;
            Log.Information("生成Modbus程序。");
            ImportPrograms();
            Log.Information("生成Modbus程序成功。");
        }

        private async void CleanModbusData()
        {
            var dialogResult = await dialogHostService.Question("温馨提示", $"确定要清除通信配置吗？");
            if (dialogResult.Result != Prism.Services.Dialogs.ButtonResult.OK) return;
            Log.Information("清除了所有通信配置。");
            mb_No = 0;
            StartTIAPortalModel.ModbusInfo.Clear();
        }

        private async void InitializationCheck()
        {
            string message = string.Empty;
            await Task.Delay(1000);
            string errormessage = _newCheckVersion.SetWhitelist(System.Diagnostics.Process.GetCurrentProcess().ProcessName, System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, SelectedOpennessVersion);
            if (!string.IsNullOrEmpty(errormessage))
            {
                Log.Fatal($"为TIA Portal设置白名单时发生错误：{errormessage}。");
                var dialogResult1 = await dialogHostService.Question("为TIA Portal设置白名单失败", errormessage);
            }
            if (_newCheckVersion.GetPlcSoftware())
            {

            }
            else
            {
                Log.Information("检测到当前用户不在Siemens TIA Openness用户组");
                var dialogResult = await dialogHostService.Question("温馨提示", $"检测到当前用户不在Siemens TIA Openness用户组,需要帮您添加吗？");
                if (dialogResult.Result != Prism.Services.Dialogs.ButtonResult.OK) return;
                Log.Information("将当前用户添加至Siemens TIA Openness用户组。");
                UpdateLoading(true);
                await Task.Run(async () =>
                {
                    message = _newCheckVersion.AddUserToGroup();
                });
                UpdateLoading(false);
                if (!string.IsNullOrEmpty(message))
                {
                    string[] parts = message.Split(',');
                    Log.Information(parts[1]);
                    var dialogResult1 = await dialogHostService.Question(parts[0], parts[1]);
                    if (message == "温馨提示,用户已成功添加到用户组,请重启系统。")
                    {
                        if (dialogResult1.Result != Prism.Services.Dialogs.ButtonResult.OK) System.Windows.Application.Current.MainWindow.Close();
                    }
                }
            }
        }
        private void CreateModbusData()
        {
            Log.Information("插入了一条Modbus指令。");
            StartTIAPortalModel.ModbusInfo.Add(new ModbusInfo { MB_No = (ushort)(mb_No += 1) });
        }

        private void ExecuteDel(object obj)
        {
            // 在集合中查找元素
            ModbusInfo itemToRemove = StartTIAPortalModel.ModbusInfo.FirstOrDefault(item => item.MB_No == Convert.ToInt32(obj));

            // 如果找到了元素，就删除它
            if (itemToRemove != null)
            {
                StartTIAPortalModel.ModbusInfo.Remove(itemToRemove);
                //StartTIAPortalModel.ModbusInfo = modbusInfos; 
            }
        }

        private void SelectImportBlockPath()
        {

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            // 设置对话框的属性
            openFileDialog.Title = "打开现有项目";
            openFileDialog.Filter = "xml文件 (*.xml)|*.xml";
            openFileDialog.Multiselect = false; // 不允许多选

            // 显示对话框，并等待用户选择文件
            bool? result = openFileDialog.ShowDialog();

            // 处理用户选择的文件
            if (result == true)
            {
                // 获取用户选择的文件路径
                string[] selectedFiles = openFileDialog.FileNames;

                // 处理选中的文件
                foreach (string file in selectedFiles)
                {
                    StartTIAPortalModel.ImportBlocksPath = file;
                }
            }
        }

        private async void ImportBlocks()
        {
            Log.Information($"将{StartTIAPortalModel.ImportBlocksPath}文件导入到'{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}Group'块组。");
            bool? result = false;
            UpdateLoading(true);
            await Task.Run(async () =>
            {
                result = _newTIAPortal.ImportBlockGroups(StartTIAPortalModel.ImportBlocksPath, $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace("/", "_")}Group");
                if (App.Current != null)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {

                    });
                }
            });
            UpdateLoading(false);
            if (result == true)
            {
                Log.Fatal($"导入{StartTIAPortalModel.ImportBlocksPath}文件时未连接到PLC。");
                var dialogResult = dialogHostService.Question("温馨提示", "请连接PLC。");
            }
            else
            {
                Log.Information($"将{StartTIAPortalModel.ImportBlocksPath}文件导入到'{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}Group'块组成功。");
            }
        }

        private void BlockExportPath()
        {
            // 创建一个文件夹选择对话框实例
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            // 显示文件夹选择对话框，并等待用户选择文件夹
            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();

            // 如果用户点击了"确定"按钮，则获取用户选择的文件夹路径
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedFolderPath = folderBrowserDialog.SelectedPath;
                StartTIAPortalModel.BlockExportPath = selectedFolderPath;
            }
        }

        private void ExportRegularBlock()
        {
            Log.Information($"将'{StartTIAPortalModel.TiaPortalDevicesPlcBlockName}'块导出到路径：{StartTIAPortalModel.BlockExportPath}。");
            _newTIAPortal.ExportRegularBlock(StartTIAPortalModel.BlockExportPath, StartTIAPortalModel.TiaPortalDevicesPlcBlockName);
            Log.Information($"将'{StartTIAPortalModel.TiaPortalDevicesPlcBlockName}'块导出到路径：{StartTIAPortalModel.BlockExportPath}成功。");
        }

        private async void ConnectTiaPortalDevices()
        {
            if (string.IsNullOrWhiteSpace(StartTIAPortalModel.TiaPortalDevicesNameSelected))
            {
                Log.Warning("连接驱动之前未输入驱动名。");
                var dialogResult = dialogHostService.Question("温馨提示", "请输入驱动名。");
                return;
            }
            if (projectTIA == null)
            {
                Log.Warning("连接驱动之前未打开项目。");
                var dialogResult = dialogHostService.Question("温馨提示", "请打开项目。");
                return;
            }
            Log.Information($"连接'{StartTIAPortalModel.TiaPortalDevicesNameSelected}'驱动。");
            UpdateLoading(true);
            await Task.Run(async () =>
            {
                StartTIAPortalModel.TiaPortalDevicesPlcBlock = _newTIAPortal.ConnectTiaPortalDevices(StartTIAPortalModel.TiaPortalDevicesNameSelected);
                StartTIAPortalModel.TiaPortalPortName = _newTIAPortal.GetPortName(StartTIAPortalModel.SelectedIndexPort);
            });
            UpdateLoading(false);
            Log.Information($"连接'{StartTIAPortalModel.TiaPortalDevicesNameSelected}'驱动成功。");
        }

        private async void GetTiaPortalDevices()
        {

            if (projectTIA == null)
            {
                Log.Warning("获取驱动前未打开项目。");
                var dialogResult = dialogHostService.Question("温馨提示", "请打开项目。");
                return;
            }
            Log.Information($"获取'{StartTIAPortalModel.SelectedIndexPort}'驱动。");
            UpdateLoading(true);
            await Task.Run(async () =>
            {
                //接收返回的项目中驱动的名字
                StartTIAPortalModel.TiaPortalDevicesName = _newTIAPortal.GetTiaPortalDevices();
                StartTIAPortalModel.TiaPortalPortName = _newTIAPortal.GetPortName(StartTIAPortalModel.SelectedIndexPort);
            });
            UpdateLoading(false);
            Log.Information($"获取'{StartTIAPortalModel.SelectedIndexPort}'驱动成功。");
        }

        private async void ConnectTIAprj()
        {
            if (string.IsNullOrWhiteSpace(StartTIAPortalModel.TiaPortalProcessName))
            {
                Log.Warning("连接进程之前未输入已打开的项目名。");
                var dialogResult = dialogHostService.Question("温馨提示", "请输入已打开的项目名。"); return;
            }
            Log.Information($"连接TIA Portal中'{StartTIAPortalModel.TiaPortalProcessName}'进程。");
            UpdateLoading(true);
            await Task.Run(async () =>
            {
                //连接博途项目，并接收返回的项目中驱动的名字
                projectTIA = _newTIAPortal.ConnectTIAprj(StartTIAPortalModel.TiaPortalProcessName);
                StartTIAPortalModel.TiaPortalDevicesName = _newTIAPortal.GetTiaPortalDevices();
                StartTIAPortalModel.TiaPortalPortName = _newTIAPortal.GetPortName(StartTIAPortalModel.SelectedIndexPort);
            });
            UpdateLoading(false);
            Log.Information($"连接TIA Portal中'{StartTIAPortalModel.TiaPortalProcessName}'进程成功。");
        }

        private async void GetTiaPortalProcess()
        {
            Log.Information("获取TiaPortal进程。");
            UpdateLoading(true);
            await Task.Run(async () =>
            {
                //获取TiaPortal进程
                IEnumerable<TiaPortalProcess> processes = TiaPortal.GetProcesses();
                StartTIAPortalModel.TiaPortalProcess = processes
                   .Select(process => process.ProjectPath?.Name)
                   .Where(name => !string.IsNullOrEmpty(name))
                   .ToList();
            });
            UpdateLoading(false);
            StringBuilder tiaPortalProcess = new StringBuilder(50);
            for (int i = 0; i < StartTIAPortalModel.TiaPortalProcess.Count; i++)
            {
                tiaPortalProcess.Append(StartTIAPortalModel.TiaPortalProcess[i]);
                tiaPortalProcess.Append(i == StartTIAPortalModel.TiaPortalProcess.Count-1 ? "。" : "，");
            }
            Log.Information($"获取TiaPortal进程成功。获取到的进程:{tiaPortalProcess}");
        }

        private async void CreateData()
        {
            if (projectTIA == null)
            {
                var dialogResult = dialogHostService.Question("温馨提示", "请打开项目。");
                return;
            }
            UpdateLoading(true);
            await Task.Run(async () =>
            {
                _newTIAPortal.CreatePLC(StartTIAPortalModel.PlcArticle, StartTIAPortalModel.PlcVersion, StartTIAPortalModel.PlcName);
            });
            UpdateLoading(false);
        }

        private async void OpenFile()
        {
            if (instTIA == null)
            {
                Log.Warning($"打开项目之前未打开TIA Portal。");
                var dialogResult = dialogHostService.Question("温馨提示", "请先打开TIA Portal。");
                return;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(StartTIAPortalModel.SelectFile))
                {
                    Log.Warning($"打开项目之前未选择项目文件。");
                    var dialogResult = dialogHostService.Question("温馨提示", "请选择项目文件。");
                    return;
                }
                Log.Information($"打开'{StartTIAPortalModel.SelectFile}'项目。");
                UpdateLoading(true);
                await Task.Run(async () =>
                {
                    projectTIA = _newTIAPortal.OpenFile(StartTIAPortalModel.SelectFile);
                });
                UpdateLoading(false);
                Log.Information($"打开'{StartTIAPortalModel.SelectFile}'项目成功。");
            }
        }

        private void SelectFile()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            // 设置对话框的属性
            openFileDialog.Title = "打开现有项目";
            openFileDialog.Filter = $"TIA Portal项目 (*.ap{ConvertToDoubleWithNoTrailingZeros(SelectedOpennessVersion)})|*.ap{ConvertToDoubleWithNoTrailingZeros(SelectedOpennessVersion)}";
            openFileDialog.Multiselect = false; // 不允许多选

            // 显示对话框，并等待用户选择文件
            bool? result = openFileDialog.ShowDialog();

            // 处理用户选择的文件
            if (result == true)
            {
                // 获取用户选择的文件路径
                string[] selectedFiles = openFileDialog.FileNames;

                // 处理选中的文件
                foreach (string file in selectedFiles)
                {
                    StartTIAPortalModel.SelectFile = file;
                }
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
                StartTIAPortalModel.SelectPath = selectedFolderPath;
            }
        }

        private async void CreateProject()
        {
            if (instTIA == null)
            {
                Log.Warning($"创建项目之前未打开TIA Portal。");
                var dialogResult = dialogHostService.Question("温馨提示", "请先打开TIA Portal。");
                return;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(StartTIAPortalModel.SelectPath))
                {
                    Log.Warning($"创建项目之前未选择路径。");
                    var dialogResult = dialogHostService.Question("温馨提示", "请选择路径。");
                    return;
                }
                if (string.IsNullOrWhiteSpace(StartTIAPortalModel.ProjectName))
                {
                    Log.Warning($"创建项目之前未输入项目名。");
                    var dialogResult = dialogHostService.Question("温馨提示", "请输入项目名。");
                    return;
                }
                Log.Information($"在{StartTIAPortalModel.SelectPath}路径中创建'{StartTIAPortalModel.ProjectName}'项目。");
                UpdateLoading(true);
                await Task.Run(async () =>
                {
                    //创建TIA Portal项目，文件路径和项目名必填，项目作者和项目注释可选填。
                    _newTIAPortal.CreateTIAprj(@"" + StartTIAPortalModel.SelectPath, StartTIAPortalModel.ProjectName, StartTIAPortalModel.ProjectAuthor, StartTIAPortalModel.ProjectComment);
                });
                UpdateLoading(false);
                Log.Information($"'{StartTIAPortalModel.ProjectName}'项目创建成功。");
            }
        }

        static void CreateCompileUnitTypeAElement(XmlDocument doc, XmlElement parentElement, int id, string programmingLanguage, string titleText, string commentText)
        {
            // 创建 <SW.Blocks.CompileUnit> 元素
            XmlElement compileUnitElement = doc.CreateElement("SW.Blocks.CompileUnit");
            compileUnitElement.SetAttribute("ID", id.ToString("X"));
            compileUnitElement.SetAttribute("CompositionName", "CompileUnits");

            // 创建 <AttributeList> 元素
            XmlElement attributeListElement = doc.CreateElement("AttributeList");

            // 创建 NetworkSource 元素
            XmlElement networkSourceElement = doc.CreateElement("NetworkSource");

            // 创建 FlgNet 元素
            XmlElement flgNetElement = doc.CreateElement("FlgNet");
            flgNetElement.SetAttribute("xmlns", "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4");

            // 添加 <FlgNet> 元素的子元素
            XmlElement partsElement = doc.CreateElement("Parts");

            AddSymbolElement(doc, partsElement, 21, true, "FirstScan");
            AddConstantElement(doc, partsElement, 22, "USInt.4");
            AddSymbolElement(doc, partsElement, 23, true, "Modbus_Comm_Load_DB.MODE");
            // 添加 <Part> 元素
            XmlElement partAElement = doc.CreateElement("Part");
            partAElement.SetAttribute("Name", "Contact");
            partAElement.SetAttribute("UId", "24");
            partsElement.AppendChild(partAElement);

            // 添加 <Part> 元素
            XmlElement partElement = doc.CreateElement("Part");
            partElement.SetAttribute("Name", "Move");
            partElement.SetAttribute("UId", "25");
            partElement.SetAttribute("DisabledENO", "true");

            XmlElement templateValueElement = doc.CreateElement("TemplateValue");
            templateValueElement.SetAttribute("Name", "Card");
            templateValueElement.SetAttribute("Type", "Cardinality");
            templateValueElement.InnerText = "1";
            partElement.AppendChild(templateValueElement);

            partsElement.AppendChild(partElement);
            flgNetElement.AppendChild(partsElement);

            // 将 Wires 元素添加到 FlgNet 元素中
            XmlElement wiresElement = doc.CreateElement("Wires");

            // 添加 <Wire> 元素
            XmlElement wireElement1 = doc.CreateElement("Wire");
            wireElement1.SetAttribute("UId", "26");
            wiresElement.AppendChild(wireElement1);
            XmlElement powerrailElement = doc.CreateElement("Powerrail");
            wireElement1.AppendChild(powerrailElement);
            XmlElement nameConElement1 = doc.CreateElement("NameCon");
            nameConElement1.SetAttribute("UId", "24");
            nameConElement1.SetAttribute("Name", "in");
            wireElement1.AppendChild(nameConElement1);
            wiresElement.AppendChild(wireElement1);

            // 创建 <Wire> 元素，并添加子元素和属性
            XmlElement wire27 = doc.CreateElement("Wire");
            wire27.SetAttribute("UId", "27");
            wire27.AppendChild(CreateIdentCon(doc, "21"));
            wire27.AppendChild(CreateNameCon(doc, "24", "operand"));
            wiresElement.AppendChild(wire27);

            XmlElement wire28 = doc.CreateElement("Wire");
            wire28.SetAttribute("UId", "28");
            wire28.AppendChild(CreateNameCon(doc, "24", "out"));
            wire28.AppendChild(CreateNameCon(doc, "25", "en"));
            wiresElement.AppendChild(wire28);

            XmlElement wire29 = doc.CreateElement("Wire");
            wire29.SetAttribute("UId", "29");
            wire29.AppendChild(CreateIdentCon(doc, "22"));
            wire29.AppendChild(CreateNameCon(doc, "25", "in"));
            wiresElement.AppendChild(wire29);

            XmlElement wire30 = doc.CreateElement("Wire");
            wire30.SetAttribute("UId", "30");
            wire30.AppendChild(CreateNameCon(doc, "25", "out1"));
            wire30.AppendChild(CreateIdentCon(doc, "23"));
            wiresElement.AppendChild(wire30);
            flgNetElement.AppendChild(wiresElement);

            // 将 FlgNet 元素添加到 NetworkSource 元素中
            networkSourceElement.AppendChild(flgNetElement);

            // 将 NetworkSource 元素添加到 AttributeList 元素中
            attributeListElement.AppendChild(networkSourceElement);

            // 创建 ProgrammingLanguage 元素
            XmlElement programmingLanguageElement = doc.CreateElement("ProgrammingLanguage");
            programmingLanguageElement.InnerText = programmingLanguage;

            // 将 ProgrammingLanguage 元素添加到 AttributeList 元素中
            attributeListElement.AppendChild(programmingLanguageElement);

            // 将 AttributeList 元素添加到 SW.Blocks.CompileUnit 元素中
            compileUnitElement.AppendChild(attributeListElement);

            // 创建 ObjectList 元素
            XmlElement objectListElement = doc.CreateElement("ObjectList");

            // 创建 Comment MultilingualText
            CreateMultilingualTextElement(doc, objectListElement, id + 1, "Comment", "zh-CN", commentText);

            // 创建 Title MultilingualText
            CreateMultilingualTextElement(doc, objectListElement, id + 3, "Title", "zh-CN", titleText);

            // 将 ObjectList 元素添加到 SW.Blocks.CompileUnit 元素中
            compileUnitElement.AppendChild(objectListElement);

            // 将 SW.Blocks.CompileUnit 元素添加到父元素中
            parentElement.AppendChild(compileUnitElement);
        }

        private void CreateCompileUnitTypeCElement(XmlDocument doc, XmlElement parentElement, int id, string programmingLanguage, string titleText, string commentText)
        {
            // 创建 <SW.Blocks.CompileUnit> 元素
            XmlElement compileUnitElement = doc.CreateElement("SW.Blocks.CompileUnit");
            compileUnitElement.SetAttribute("ID", id.ToString("X"));
            compileUnitElement.SetAttribute("CompositionName", "CompileUnits");

            // 创建 <AttributeList> 元素
            XmlElement attributeListElement = doc.CreateElement("AttributeList");

            // 创建 NetworkSource 元素
            XmlElement networkSourceElement = doc.CreateElement("NetworkSource");

            // 创建 FlgNet 元素
            XmlElement flgNetElement = doc.CreateElement("FlgNet");
            flgNetElement.SetAttribute("xmlns", "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4");

            // 添加 <FlgNet> 元素的子元素
            XmlElement partsElement = doc.CreateElement("Parts");

            AddSymbolElement(doc, partsElement, 21, true, $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB.Comm_Load_DONE");
            AddConstantElement(doc, partsElement, 22, "USInt.1");
            AddSymbolElement(doc, partsElement, 23, true, $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB.Step");
            // 添加 <Part> 元素
            XmlElement partAElement = doc.CreateElement("Part");
            partAElement.SetAttribute("Name", "Contact");
            partAElement.SetAttribute("UId", "24");
            partsElement.AppendChild(partAElement);

            // 添加 <Part> 元素
            XmlElement partElement = doc.CreateElement("Part");
            partElement.SetAttribute("Name", "Move");
            partElement.SetAttribute("UId", "25");
            partElement.SetAttribute("DisabledENO", "true");

            XmlElement templateValueElement = doc.CreateElement("TemplateValue");
            templateValueElement.SetAttribute("Name", "Card");
            templateValueElement.SetAttribute("Type", "Cardinality");
            templateValueElement.InnerText = "1";
            partElement.AppendChild(templateValueElement);

            partsElement.AppendChild(partElement);
            flgNetElement.AppendChild(partsElement);

            // 将 Wires 元素添加到 FlgNet 元素中
            XmlElement wiresElement = doc.CreateElement("Wires");

            // 添加 <Wire> 元素
            XmlElement wireElement1 = doc.CreateElement("Wire");
            wireElement1.SetAttribute("UId", "26");
            wiresElement.AppendChild(wireElement1);
            XmlElement powerrailElement = doc.CreateElement("Powerrail");
            wireElement1.AppendChild(powerrailElement);
            XmlElement nameConElement1 = doc.CreateElement("NameCon");
            nameConElement1.SetAttribute("UId", "24");
            nameConElement1.SetAttribute("Name", "in");
            wireElement1.AppendChild(nameConElement1);
            wiresElement.AppendChild(wireElement1);

            // 创建 <Wire> 元素，并添加子元素和属性
            XmlElement wire27 = doc.CreateElement("Wire");
            wire27.SetAttribute("UId", "27");
            wire27.AppendChild(CreateIdentCon(doc, "21"));
            wire27.AppendChild(CreateNameCon(doc, "24", "operand"));
            wiresElement.AppendChild(wire27);

            XmlElement wire28 = doc.CreateElement("Wire");
            wire28.SetAttribute("UId", "28");
            wire28.AppendChild(CreateNameCon(doc, "24", "out"));
            wire28.AppendChild(CreateNameCon(doc, "25", "en"));
            wiresElement.AppendChild(wire28);

            XmlElement wire29 = doc.CreateElement("Wire");
            wire29.SetAttribute("UId", "29");
            wire29.AppendChild(CreateIdentCon(doc, "22"));
            wire29.AppendChild(CreateNameCon(doc, "25", "in"));
            wiresElement.AppendChild(wire29);

            XmlElement wire30 = doc.CreateElement("Wire");
            wire30.SetAttribute("UId", "30");
            wire30.AppendChild(CreateNameCon(doc, "25", "out1"));
            wire30.AppendChild(CreateIdentCon(doc, "23"));
            wiresElement.AppendChild(wire30);
            flgNetElement.AppendChild(wiresElement);

            // 将 FlgNet 元素添加到 NetworkSource 元素中
            networkSourceElement.AppendChild(flgNetElement);

            // 将 NetworkSource 元素添加到 AttributeList 元素中
            attributeListElement.AppendChild(networkSourceElement);

            // 创建 ProgrammingLanguage 元素
            XmlElement programmingLanguageElement = doc.CreateElement("ProgrammingLanguage");
            programmingLanguageElement.InnerText = programmingLanguage;

            // 将 ProgrammingLanguage 元素添加到 AttributeList 元素中
            attributeListElement.AppendChild(programmingLanguageElement);

            // 将 AttributeList 元素添加到 SW.Blocks.CompileUnit 元素中
            compileUnitElement.AppendChild(attributeListElement);

            // 创建 ObjectList 元素
            XmlElement objectListElement = doc.CreateElement("ObjectList");

            // 创建 Comment MultilingualText
            CreateMultilingualTextElement(doc, objectListElement, id + 1, "Comment", "zh-CN", commentText);

            // 创建 Title MultilingualText
            CreateMultilingualTextElement(doc, objectListElement, id + 3, "Title", "zh-CN", titleText);

            // 将 ObjectList 元素添加到 SW.Blocks.CompileUnit 元素中
            compileUnitElement.AppendChild(objectListElement);

            // 将 SW.Blocks.CompileUnit 元素添加到父元素中
            parentElement.AppendChild(compileUnitElement);
        }

        static void CreateCompileUnitTypeBElement(XmlDocument doc, XmlElement parentElement, int id, List<InstructionData> list, List<string> listComponentName, string programmingLanguage, string titleText, string commentText)
        {
            // 创建 <SW.Blocks.CompileUnit> 元素
            XmlElement compileUnitElement = doc.CreateElement("SW.Blocks.CompileUnit");
            compileUnitElement.SetAttribute("ID", id.ToString("X"));
            compileUnitElement.SetAttribute("CompositionName", "CompileUnits");

            // 创建 <AttributeList> 元素
            XmlElement attributeListElement = doc.CreateElement("AttributeList");

            // 创建 NetworkSource 元素
            XmlElement networkSourceElement = doc.CreateElement("NetworkSource");

            // 创建 FlgNet 元素
            XmlElement flgNetElement = doc.CreateElement("FlgNet");
            flgNetElement.SetAttribute("xmlns", "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4");

            // 将 Parts 元素添加到 FlgNet 元素中
            XmlElement partsElement = doc.CreateElement("Parts");
            int idAdd = 20;
            int idAdd1 = 20;
            for (int i = 0; i < list.Count; i++)
            {
                switch (list[i].Scope)
                {
                    case "GlobalVariable":
                        AddSymbolElement(doc, partsElement, idAdd += 1, list[i].AddressType == "Constant" ? false : true, listComponentName[i]);
                        break;
                    case "LiteralConstant":
                        AddConstantElement(doc, partsElement, idAdd += 1, listComponentName[i]);
                        break;
                    case "GlobalConstant":
                        AddSymbolGlobalConstantElement(doc, partsElement, idAdd += 1, list[i].AddressType == "Constant" ? false : true, listComponentName[i]);
                        break;
                    default:
                        //System.Windows.MessageBox.Show("Scope is not GlobalVariable or LiteralConstant.");
                        break;
                }
            }

            XmlElement partAElement = doc.CreateElement("Part");
            partAElement.SetAttribute("Name", "Contact");
            partAElement.SetAttribute("UId", $"{idAdd += 1}");

            // 添加 <Part> 元素
            XmlElement NegatedElement = doc.CreateElement("Negated");
            NegatedElement.SetAttribute("Name", "operand");
            partAElement.AppendChild(NegatedElement);

            partsElement.AppendChild(partAElement);

            // 添加 <Part> 元素
            XmlElement partElement = doc.CreateElement("Part");
            partElement.SetAttribute("Name", list[0].Name);
            partElement.SetAttribute("Version", list[0].Version);
            partElement.SetAttribute("UId", $"{idAdd += 1}");
            int noTemp = idAdd;

            XmlElement instanceElement = doc.CreateElement("Instance");
            instanceElement.SetAttribute("Scope", "GlobalVariable");
            instanceElement.SetAttribute("UId", $"{idAdd += 1}");
            partElement.AppendChild(instanceElement);

            XmlElement componentElement = doc.CreateElement("Component");
            componentElement.SetAttribute("Name", "Modbus_Comm_Load_DB");
            instanceElement.AppendChild(componentElement);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].TemplateValueName != null)
                {
                    XmlElement templateValueElement = doc.CreateElement("TemplateValue");
                    templateValueElement.SetAttribute("Name", list[i].TemplateValueName);
                    templateValueElement.SetAttribute("Type", list[i].TemplateValueType);
                    templateValueElement.InnerText = list[i].TemplateValue;
                    partElement.AppendChild(templateValueElement);
                }
            }
            partsElement.AppendChild(partElement);
            flgNetElement.AppendChild(partsElement);

            // 将 Wires 元素添加到 FlgNet 元素中
            XmlElement wiresElement = doc.CreateElement("Wires");

            // 添加 <Wire> 元素
            XmlElement wireElementD1 = doc.CreateElement("Wire");
            wireElementD1.SetAttribute("UId", "36");
            wiresElement.AppendChild(wireElementD1);
            XmlElement powerrailElementD1 = doc.CreateElement("Powerrail");
            wireElementD1.AppendChild(powerrailElementD1);
            XmlElement nameConElementD1 = doc.CreateElement("NameCon");
            nameConElementD1.SetAttribute("UId", "34");
            nameConElementD1.SetAttribute("Name", "en");
            wireElementD1.AppendChild(nameConElementD1);
            XmlElement nameConElementD2 = doc.CreateElement("NameCon");
            nameConElementD2.SetAttribute("UId", "33");
            nameConElementD2.SetAttribute("Name", "in");
            wireElementD1.AppendChild(nameConElementD2);
            wiresElement.AppendChild(wireElementD1);

            AddWireElement(doc, wiresElement, "37", "21", "33", "operand", false);

            // 添加 <Wire> 元素
            XmlElement wireElementD2 = doc.CreateElement("Wire");
            wireElementD2.SetAttribute("UId", "38");
            wiresElement.AppendChild(wireElementD2);
            XmlElement nameConElementD21 = doc.CreateElement("NameCon");
            nameConElementD21.SetAttribute("UId", "33");
            nameConElementD21.SetAttribute("Name", "out");
            wireElementD2.AppendChild(nameConElementD21);
            XmlElement nameConElementD31 = doc.CreateElement("NameCon");
            nameConElementD31.SetAttribute("UId", "34");
            nameConElementD31.SetAttribute("Name", "REQ");
            wireElementD2.AppendChild(nameConElementD31);
            wiresElement.AppendChild(wireElementD2);

            idAdd = 38;
            idAdd1 = 21;
            for (int i = 2; i < list.Count; i++)
            {
                XmlElement wireElement = doc.CreateElement("Wire");
                wireElement.SetAttribute("UId", $"{idAdd += 1}");
                XmlElement identConElement = doc.CreateElement("IdentCon"); ;
                XmlElement powerrailElement = doc.CreateElement("Powerrail");
                XmlElement nameConElement = doc.CreateElement("NameCon");
                nameConElement.SetAttribute("UId", $"{noTemp}");
                nameConElement.SetAttribute("Name", list[i].Wire);
                if (list[i].InputOrOutput == "Input")
                {
                    if (i == 0)
                    {
                        wireElement.AppendChild(powerrailElement);
                    }
                    else
                    {
                        identConElement.SetAttribute("UId", $"{idAdd1 += 1}");
                        wireElement.AppendChild(identConElement);
                    }
                    wireElement.AppendChild(nameConElement);
                }
                else
                {
                    identConElement.SetAttribute("UId", $"{idAdd1 += 1}");
                    wireElement.AppendChild(nameConElement);
                    wireElement.AppendChild(identConElement);
                }
                wiresElement.AppendChild(wireElement);
            }

            flgNetElement.AppendChild(wiresElement);

            // 将 FlgNet 元素添加到 NetworkSource 元素中
            networkSourceElement.AppendChild(flgNetElement);

            // 将 NetworkSource 元素添加到 AttributeList 元素中
            attributeListElement.AppendChild(networkSourceElement);

            // 创建 ProgrammingLanguage 元素
            XmlElement programmingLanguageElement = doc.CreateElement("ProgrammingLanguage");
            programmingLanguageElement.InnerText = programmingLanguage;

            // 将 ProgrammingLanguage 元素添加到 AttributeList 元素中
            attributeListElement.AppendChild(programmingLanguageElement);

            // 将 AttributeList 元素添加到 SW.Blocks.CompileUnit 元素中
            compileUnitElement.AppendChild(attributeListElement);

            // 创建 ObjectList 元素
            XmlElement objectListElement = doc.CreateElement("ObjectList");

            // 创建 Comment MultilingualText
            CreateMultilingualTextElement(doc, objectListElement, id + 1, "Comment", "zh-CN", commentText);

            // 创建 Title MultilingualText
            CreateMultilingualTextElement(doc, objectListElement, id + 3, "Title", "zh-CN", titleText);

            // 将 ObjectList 元素添加到 SW.Blocks.CompileUnit 元素中
            compileUnitElement.AppendChild(objectListElement);

            // 将 SW.Blocks.CompileUnit 元素添加到父元素中
            parentElement.AppendChild(compileUnitElement);
        }

        private void CreateCompileUnitElementONE(XmlDocument doc, XmlElement parentElement, int id, ModbusInfo modbusInfoONE, string programmingLanguage, string titleText, string commentText)
        {
            // 创建 <SW.Blocks.CompileUnit> 元素
            XmlElement compileUnitElement = doc.CreateElement("SW.Blocks.CompileUnit");
            compileUnitElement.SetAttribute("ID", id.ToString("X"));
            compileUnitElement.SetAttribute("CompositionName", "CompileUnits");

            // 创建 <AttributeList> 元素
            XmlElement attributeListElement = doc.CreateElement("AttributeList");

            // 创建 NetworkSource 元素
            XmlElement networkSourceElement = doc.CreateElement("NetworkSource");

            // 创建 FlgNet 元素
            XmlElement flgNetElement = doc.CreateElement("FlgNet");
            flgNetElement.SetAttribute("xmlns", "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4");

            // 将 Parts 元素添加到 FlgNet 元素中
            // 添加 <FlgNet> 元素的子元素
            XmlElement partsElement = doc.CreateElement("Parts");

            // 添加 <Access> 元素
            AddSymbolElement(doc, partsElement, 21, true, $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB.Step");
            //AddComponenElement(doc, partsElement8, "22", "Local~CM_1241_(RS422_485)_1");
            AddConstantElement(doc, partsElement, 22, $"USInt.{(id - 18) / 10 + 1}");
            AddConstantElement(doc, partsElement, 23, $"UInt.{modbusInfoONE.MB_ADDR}");
            AddConstantElement(doc, partsElement, 24, $"USInt.{modbusInfoONE.MODE}");
            AddConstantElement(doc, partsElement, 25, $"UDInt.{modbusInfoONE.DATA_ADDR}");
            AddConstantElement(doc, partsElement, 26, $"UInt.{modbusInfoONE.DATA_LEN}");
            AddSymbolElement(doc, partsElement, 27, true, $"{(modbusInfoONE.MODE == 0 ? $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB.Read_DATA_{modbusInfoONE.Name}{modbusInfoONE.MB_No}" : $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB.Write_DATA_{modbusInfoONE.Name}{modbusInfoONE.MB_No}")}");
            AddSymbolArrayElement(doc, partsElement, 28, (id - 18) / 10 + 1, "DONE");
            AddSymbolArrayElement(doc, partsElement, 29, (id - 18) / 10 + 1, "BUSY");
            AddSymbolArrayElement(doc, partsElement, 30, (id - 18) / 10 + 1, "ERROR");
            AddSymbolArrayElement(doc, partsElement, 31, (id - 18) / 10 + 1, "STATUS");

            // 添加 <Part> 元素
            XmlElement partElement2 = doc.CreateElement("Part");
            partElement2.SetAttribute("Name", "Eq");
            partElement2.SetAttribute("UId", "32");
            XmlElement templateValueElement = doc.CreateElement("TemplateValue");
            templateValueElement.SetAttribute("Name", "SrcType");
            templateValueElement.SetAttribute("Type", "Type");
            templateValueElement.InnerText = "USInt";
            partElement2.AppendChild(templateValueElement);
            partsElement.AppendChild(partElement2);

            // 添加 <Part> 元素
            XmlElement partElement8 = doc.CreateElement("Part");
            partElement8.SetAttribute("Name", "Modbus_Master");
            partElement8.SetAttribute("Version", "2.4");
            partElement8.SetAttribute("UId", "33");

            XmlElement instanceElement8 = doc.CreateElement("Instance");
            instanceElement8.SetAttribute("Scope", "GlobalVariable");
            instanceElement8.SetAttribute("UId", "34");
            partElement8.AppendChild(instanceElement8);

            XmlElement componentElement8 = doc.CreateElement("Component");
            componentElement8.SetAttribute("Name", "Modbus_Master_DB");
            instanceElement8.AppendChild(componentElement8);

            XmlElement templateValueElement81 = doc.CreateElement("TemplateValue");
            templateValueElement81.SetAttribute("Name", "card");
            templateValueElement81.SetAttribute("Type", "Cardinality");
            templateValueElement81.InnerText = "0";
            partElement8.AppendChild(templateValueElement81);

            XmlElement templateValueElement83 = doc.CreateElement("TemplateValue");
            templateValueElement83.SetAttribute("Name", "word_type");
            templateValueElement83.SetAttribute("Type", "Type");
            templateValueElement83.InnerText = "UInt";
            partElement8.AppendChild(templateValueElement83);

            XmlElement templateValueElement84 = doc.CreateElement("TemplateValue");
            templateValueElement84.SetAttribute("Name", "dword_type");
            templateValueElement84.SetAttribute("Type", "Type");
            templateValueElement84.InnerText = "UDInt";
            partElement8.AppendChild(templateValueElement84);

            XmlElement templateValueElement85 = doc.CreateElement("TemplateValue");
            templateValueElement85.SetAttribute("Name", "byte_type");
            templateValueElement85.SetAttribute("Type", "Type");
            templateValueElement85.InnerText = "USInt";
            partElement8.AppendChild(templateValueElement85);

            XmlElement templateValueElement86 = doc.CreateElement("TemplateValue");
            templateValueElement86.SetAttribute("Name", "any_type");
            templateValueElement86.SetAttribute("Type", "Type");
            templateValueElement86.InnerText = "Variant";
            partElement8.AppendChild(templateValueElement86);
            partsElement.AppendChild(partElement8);
            flgNetElement.AppendChild(partsElement);

            // 将 Wires 元素添加到 FlgNet 元素中
            XmlElement wiresElementD = doc.CreateElement("Wires");
            // 添加 <Wire> 元素
            XmlElement wireElementD1 = doc.CreateElement("Wire");
            wireElementD1.SetAttribute("UId", "35");
            wiresElementD.AppendChild(wireElementD1);
            XmlElement powerrailElementD1 = doc.CreateElement("Powerrail");
            wireElementD1.AppendChild(powerrailElementD1);
            XmlElement nameConElementD1 = doc.CreateElement("NameCon");
            nameConElementD1.SetAttribute("UId", "33");
            nameConElementD1.SetAttribute("Name", "en");
            wireElementD1.AppendChild(nameConElementD1);
            XmlElement nameConElementD2 = doc.CreateElement("NameCon");
            nameConElementD2.SetAttribute("UId", "32");
            nameConElementD2.SetAttribute("Name", "pre");
            wireElementD1.AppendChild(nameConElementD2);
            wiresElementD.AppendChild(wireElementD1);

            AddWireElement(doc, wiresElementD, "36", "21", "32", "in1", false);
            AddWireElement(doc, wiresElementD, "37", "22", "32", "in2", false);

            XmlElement wireElementD2 = doc.CreateElement("Wire");
            wireElementD2.SetAttribute("UId", "38");
            wiresElementD.AppendChild(wireElementD2);
            XmlElement nameConElementD21 = doc.CreateElement("NameCon");
            nameConElementD21.SetAttribute("UId", "32");
            nameConElementD21.SetAttribute("Name", "out");
            wireElementD2.AppendChild(nameConElementD21);
            XmlElement nameConElementD3 = doc.CreateElement("NameCon");
            nameConElementD3.SetAttribute("UId", "33");
            nameConElementD3.SetAttribute("Name", "REQ");
            wireElementD2.AppendChild(nameConElementD3);
            wiresElementD.AppendChild(wireElementD2);

            AddWireElement(doc, wiresElementD, "39", "23", "33", "MB_ADDR", false);
            AddWireElement(doc, wiresElementD, "40", "24", "33", "MODE", false);
            AddWireElement(doc, wiresElementD, "41", "25", "33", "DATA_ADDR", false);
            AddWireElement(doc, wiresElementD, "42", "26", "33", "DATA_LEN", false);
            AddWireElement(doc, wiresElementD, "43", "27", "33", "DATA_PTR", false);
            AddWireElement(doc, wiresElementD, "44", "28", "33", "DONE", true);
            AddWireElement(doc, wiresElementD, "45", "29", "33", "BUSY", true);
            AddWireElement(doc, wiresElementD, "46", "30", "33", "ERROR", true);
            AddWireElement(doc, wiresElementD, "47", "31", "33", "STATUS", true);

            flgNetElement.AppendChild(wiresElementD);

            // 将 FlgNet 元素添加到 NetworkSource 元素中
            networkSourceElement.AppendChild(flgNetElement);

            // 将 NetworkSource 元素添加到 AttributeList 元素中
            attributeListElement.AppendChild(networkSourceElement);

            // 创建 ProgrammingLanguage 元素
            XmlElement programmingLanguageElement = doc.CreateElement("ProgrammingLanguage");
            programmingLanguageElement.InnerText = programmingLanguage;

            // 将 ProgrammingLanguage 元素添加到 AttributeList 元素中
            attributeListElement.AppendChild(programmingLanguageElement);

            // 将 AttributeList 元素添加到 SW.Blocks.CompileUnit 元素中
            compileUnitElement.AppendChild(attributeListElement);

            // 创建 ObjectList 元素
            XmlElement objectListElement = doc.CreateElement("ObjectList");

            // 创建 Comment MultilingualText
            CreateMultilingualTextElement(doc, objectListElement, id + 1, "Comment", "zh-CN", commentText);

            // 创建 Title MultilingualText
            CreateMultilingualTextElement(doc, objectListElement, id + 3, "Title", "zh-CN", titleText);

            // 将 ObjectList 元素添加到 SW.Blocks.CompileUnit 元素中
            compileUnitElement.AppendChild(objectListElement);

            // 将 SW.Blocks.CompileUnit 元素添加到父元素中
            parentElement.AppendChild(compileUnitElement);
        }
        private void CreateCompileUnitElementOTWO(XmlDocument doc, XmlElement parentElement, int id, bool end, string programmingLanguage, string titleText, string commentText)
        {
            // 创建 <SW.Blocks.CompileUnit> 元素
            XmlElement compileUnitElement = doc.CreateElement("SW.Blocks.CompileUnit");
            compileUnitElement.SetAttribute("ID", id.ToString("X"));
            compileUnitElement.SetAttribute("CompositionName", "CompileUnits");

            // 创建 <AttributeList> 元素
            XmlElement attributeListElement = doc.CreateElement("AttributeList");

            // 创建 NetworkSource 元素
            XmlElement networkSourceElement = doc.CreateElement("NetworkSource");

            // 创建 FlgNet 元素
            XmlElement flgNetElement = doc.CreateElement("FlgNet");
            flgNetElement.SetAttribute("xmlns", "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4");

            // 将 Parts 元素添加到 FlgNet 元素中
            // 添加 <FlgNet> 元素的子元素
            XmlElement partsElement = doc.CreateElement("Parts");

            // 添加 <Access> 元素
            AddSymbolElement(doc, partsElement, 21, true, $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB.Step");
            AddConstantElement(doc, partsElement, 22, $"USInt.{(id - 18) / 10 + 1}");
            AddSymbolArrayElement(doc, partsElement, 23, (id - 18) / 10 + 1, "DONE");
            AddSymbolArrayElement(doc, partsElement, 24, (id - 18) / 10 + 1, "ERROR");
            if (end)
            {
                AddConstantElement(doc, partsElement, 25, $"USInt.1");
            }
            else
            {
                AddConstantElement(doc, partsElement, 25, $"USInt.{(id - 18) / 10 + 2}");
            }
            AddSymbolElement(doc, partsElement, 26, true, $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB.Step");

            // 添加 <Part> 元素
            XmlElement partElement2 = doc.CreateElement("Part");
            partElement2.SetAttribute("Name", "Eq");
            partElement2.SetAttribute("UId", "27");
            XmlElement templateValueElement = doc.CreateElement("TemplateValue");
            templateValueElement.SetAttribute("Name", "SrcType");
            templateValueElement.SetAttribute("Type", "Type");
            templateValueElement.InnerText = "USInt";
            partElement2.AppendChild(templateValueElement);
            partsElement.AppendChild(partElement2);

            // 添加 <Part> 元素
            XmlElement partElement8 = doc.CreateElement("Part");
            partElement8.SetAttribute("Name", "Contact");
            partElement8.SetAttribute("UId", "28");
            partsElement.AppendChild(partElement8);

            XmlElement partElement9 = doc.CreateElement("Part");
            partElement9.SetAttribute("Name", "Contact");
            partElement9.SetAttribute("UId", "29");
            partsElement.AppendChild(partElement9);

            // 添加 <Part> 元素
            XmlElement partElement30 = doc.CreateElement("Part");
            partElement30.SetAttribute("Name", "O");
            partElement30.SetAttribute("UId", "30");
            XmlElement templateValueElement30 = doc.CreateElement("TemplateValue");
            templateValueElement30.SetAttribute("Name", "Card");
            templateValueElement30.SetAttribute("Type", "Cardinality");
            templateValueElement30.InnerText = "2";
            partElement30.AppendChild(templateValueElement30);
            partsElement.AppendChild(partElement30);

            // 添加 <Part> 元素
            XmlElement partElement31 = doc.CreateElement("Part");
            partElement31.SetAttribute("Name", "Move");
            partElement31.SetAttribute("UId", "31");
            partElement31.SetAttribute("DisabledENO", "true");
            XmlElement templateValueElement31 = doc.CreateElement("TemplateValue");
            templateValueElement31.SetAttribute("Name", "Card");
            templateValueElement31.SetAttribute("Type", "Cardinality");
            templateValueElement31.InnerText = "1";
            partElement31.AppendChild(templateValueElement31);
            partsElement.AppendChild(partElement31);

            flgNetElement.AppendChild(partsElement);

            // 将 Wires 元素添加到 FlgNet 元素中
            XmlElement wiresElementD = doc.CreateElement("Wires");
            // 添加 <Wire> 元素
            XmlElement wireElementD1 = doc.CreateElement("Wire");
            wireElementD1.SetAttribute("UId", "32");
            wiresElementD.AppendChild(wireElementD1);
            XmlElement powerrailElementD1 = doc.CreateElement("Powerrail");
            wireElementD1.AppendChild(powerrailElementD1);
            XmlElement nameConElementD1 = doc.CreateElement("NameCon");
            nameConElementD1.SetAttribute("UId", "27");
            nameConElementD1.SetAttribute("Name", "pre");
            wireElementD1.AppendChild(nameConElementD1);
            wiresElementD.AppendChild(wireElementD1);

            AddWireElement(doc, wiresElementD, "33", "21", "27", "in1", false);
            AddWireElement(doc, wiresElementD, "34", "22", "27", "in2", false);

            XmlElement wireElementD2 = doc.CreateElement("Wire");
            wireElementD2.SetAttribute("UId", "35");
            wiresElementD.AppendChild(wireElementD2);
            XmlElement nameConElementD21 = doc.CreateElement("NameCon");
            nameConElementD21.SetAttribute("UId", "27");
            nameConElementD21.SetAttribute("Name", "out");
            wireElementD2.AppendChild(nameConElementD21);
            XmlElement nameConElementD3 = doc.CreateElement("NameCon");
            nameConElementD3.SetAttribute("UId", "28");
            nameConElementD3.SetAttribute("Name", "in");
            wireElementD2.AppendChild(nameConElementD3);
            XmlElement nameConElementD4 = doc.CreateElement("NameCon");
            nameConElementD4.SetAttribute("UId", "29");
            nameConElementD4.SetAttribute("Name", "in");
            wireElementD2.AppendChild(nameConElementD4);
            wiresElementD.AppendChild(wireElementD2);

            AddWireElement(doc, wiresElementD, "36", "23", "28", "operand", false);

            XmlElement wireElementD5 = doc.CreateElement("Wire");
            wireElementD5.SetAttribute("UId", "37");
            wiresElementD.AppendChild(wireElementD5);
            XmlElement nameConElementD5 = doc.CreateElement("NameCon");
            nameConElementD5.SetAttribute("UId", "28");
            nameConElementD5.SetAttribute("Name", "out");
            wireElementD5.AppendChild(nameConElementD5);
            XmlElement nameConElementD6 = doc.CreateElement("NameCon");
            nameConElementD6.SetAttribute("UId", "30");
            nameConElementD6.SetAttribute("Name", "in1");
            wireElementD5.AppendChild(nameConElementD6);
            wiresElementD.AppendChild(wireElementD5);

            AddWireElement(doc, wiresElementD, "38", "24", "29", "operand", false);

            XmlElement wireElementD6 = doc.CreateElement("Wire");
            wireElementD6.SetAttribute("UId", "39");
            wiresElementD.AppendChild(wireElementD6);
            XmlElement nameConElementD7 = doc.CreateElement("NameCon");
            nameConElementD7.SetAttribute("UId", "29");
            nameConElementD7.SetAttribute("Name", "out");
            wireElementD6.AppendChild(nameConElementD7);
            XmlElement nameConElementD8 = doc.CreateElement("NameCon");
            nameConElementD8.SetAttribute("UId", "30");
            nameConElementD8.SetAttribute("Name", "in2");
            wireElementD6.AppendChild(nameConElementD8);
            wiresElementD.AppendChild(wireElementD6);

            XmlElement wireElementD7 = doc.CreateElement("Wire");
            wireElementD7.SetAttribute("UId", "40");
            wiresElementD.AppendChild(wireElementD7);
            XmlElement nameConElementD9 = doc.CreateElement("NameCon");
            nameConElementD9.SetAttribute("UId", "30");
            nameConElementD9.SetAttribute("Name", "out");
            wireElementD7.AppendChild(nameConElementD9);
            XmlElement nameConElementD10 = doc.CreateElement("NameCon");
            nameConElementD10.SetAttribute("UId", "31");
            nameConElementD10.SetAttribute("Name", "en");
            wireElementD7.AppendChild(nameConElementD10);
            wiresElementD.AppendChild(wireElementD7);


            AddWireElement(doc, wiresElementD, "41", "25", "31", "in", false);
            AddWireElement(doc, wiresElementD, "42", "26", "31", "out1", true);


            flgNetElement.AppendChild(wiresElementD);

            // 将 FlgNet 元素添加到 NetworkSource 元素中
            networkSourceElement.AppendChild(flgNetElement);

            // 将 NetworkSource 元素添加到 AttributeList 元素中
            attributeListElement.AppendChild(networkSourceElement);

            // 创建 ProgrammingLanguage 元素
            XmlElement programmingLanguageElement = doc.CreateElement("ProgrammingLanguage");
            programmingLanguageElement.InnerText = programmingLanguage;

            // 将 ProgrammingLanguage 元素添加到 AttributeList 元素中
            attributeListElement.AppendChild(programmingLanguageElement);

            // 将 AttributeList 元素添加到 SW.Blocks.CompileUnit 元素中
            compileUnitElement.AppendChild(attributeListElement);

            // 创建 ObjectList 元素
            XmlElement objectListElement = doc.CreateElement("ObjectList");
            // 创建 Comment MultilingualText
            CreateMultilingualTextElement(doc, objectListElement, id + 1, "Comment", "zh-CN", commentText);

            // 创建 Title MultilingualText
            CreateMultilingualTextElement(doc, objectListElement, id + 3, "Title", "zh-CN", titleText);

            // 将 ObjectList 元素添加到 SW.Blocks.CompileUnit 元素中
            compileUnitElement.AppendChild(objectListElement);

            // 将 SW.Blocks.CompileUnit 元素添加到父元素中
            parentElement.AppendChild(compileUnitElement);
        }

        static void CreateMultilingualTextElement(XmlDocument doc, XmlElement parentElement, int id, string compositionName, string culture, string text)
        {
            // 创建 MultilingualText 元素
            XmlElement multilingualTextElement = doc.CreateElement("MultilingualText");
            multilingualTextElement.SetAttribute("ID", id.ToString("X"));
            multilingualTextElement.SetAttribute("CompositionName", compositionName);

            // 创建 ObjectList 元素
            XmlElement objectListElement = doc.CreateElement("ObjectList");

            // 创建 MultilingualTextItem 元素
            XmlElement multilingualTextItemElement = doc.CreateElement("MultilingualTextItem");
            multilingualTextItemElement.SetAttribute("ID", (id + 1).ToString("X"));
            multilingualTextItemElement.SetAttribute("CompositionName", "Items");

            // 创建 AttributeList 元素
            XmlElement attributeListElement = doc.CreateElement("AttributeList");

            // 创建 Culture 元素
            XmlElement cultureElement = doc.CreateElement("Culture");
            cultureElement.InnerText = culture;

            // 创建 Text 元素
            XmlElement textElement = doc.CreateElement("Text");
            textElement.InnerText = text;

            // 将 Culture 和 Text 元素添加到 AttributeList 元素中
            attributeListElement.AppendChild(cultureElement);
            attributeListElement.AppendChild(textElement);

            // 将 AttributeList 元素添加到 MultilingualTextItem 元素中
            multilingualTextItemElement.AppendChild(attributeListElement);

            // 将 MultilingualTextItem 元素添加到 ObjectList 元素中
            objectListElement.AppendChild(multilingualTextItemElement);

            // 将 ObjectList 元素添加到 MultilingualText 元素中
            multilingualTextElement.AppendChild(objectListElement);

            // 将 MultilingualText 元素添加到父元素中
            parentElement.AppendChild(multilingualTextElement);
        }

        static XmlElement CreateIdentCon(XmlDocument doc, string uId)
        {
            XmlElement identCon = doc.CreateElement("IdentCon");
            identCon.SetAttribute("UId", uId);
            return identCon;
        }

        // 创建 <NameCon> 元素
        static XmlElement CreateNameCon(XmlDocument doc, string uId, string name)
        {
            XmlElement nameCon = doc.CreateElement("NameCon");
            nameCon.SetAttribute("UId", uId);
            nameCon.SetAttribute("Name", name);
            return nameCon;
        }



        public List<InstructionData> list;
        List<string> modbusCommLoad = new List<string>();



        private async void ImportPrograms()
        {
            modbusCommLoad.Clear();
            modbusCommLoad.Add("0");
            modbusCommLoad.Add($"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB.Comm_Load_DONE");
            modbusCommLoad.Add($"Local~{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace("/", "_")}");
            modbusCommLoad.Add($"UDInt.{StartTIAPortalModel.ModbusBaud}");
            switch (StartTIAPortalModel.ModbusParity)
            {
                case "None":
                    modbusCommLoad.Add($"UInt.0");
                    break;
                case "Odd":
                    modbusCommLoad.Add($"UInt.1");
                    break;
                case "Even":
                    modbusCommLoad.Add($"UInt.2");
                    break;
            }
            modbusCommLoad.Add("UInt.0");
            modbusCommLoad.Add("UInt.0");
            modbusCommLoad.Add("UInt.0");
            modbusCommLoad.Add("UInt.1000");
            modbusCommLoad.Add("Modbus_Master_DB.MB_DB");
            modbusCommLoad.Add($"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB.Comm_Load_DONE");
            modbusCommLoad.Add($"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB.Comm_Load_ERROR");
            modbusCommLoad.Add($"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB.Comm_Load_STATUS");

            QueryInstructionData("Modbus_Comm_Load", "3.0");
            // 创建XmlDocument对象
            XmlDocument doc = new XmlDocument();

            // 创建xml声明节点
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(xmlDeclaration);

            // 创建根节点<Document>
            XmlElement documentElement = doc.CreateElement("Document");
            doc.AppendChild(documentElement);

            // 创建子节点<Engineering>
            XmlElement engineeringElement = doc.CreateElement("Engineering");
            engineeringElement.SetAttribute("version", $"{ConvertToDoubleWithNoTrailingZeros(SelectedOpennessVersion)}");
            documentElement.AppendChild(engineeringElement);

            // 创建子节点<SW.Blocks.OB>
            XmlElement swBlocksObElement = doc.CreateElement("SW.Blocks.OB");
            swBlocksObElement.SetAttribute("ID", "0");
            documentElement.AppendChild(swBlocksObElement);

            // 创建子节点<AttributeList>
            XmlElement attributeListElement = doc.CreateElement("AttributeList");
            swBlocksObElement.AppendChild(attributeListElement);
            // 创建子节点<AutoNumber>
            XmlElement autoNumberElement = doc.CreateElement("AutoNumber");
            autoNumberElement.InnerText = "true";
            attributeListElement.AppendChild(autoNumberElement);

            // 创建子节点<HeaderAuthor>
            XmlElement headerAuthorElement = doc.CreateElement("HeaderAuthor");
            attributeListElement.AppendChild(headerAuthorElement);

            // 创建子节点<HeaderFamily>
            XmlElement headerFamilyElement = doc.CreateElement("HeaderFamily");
            attributeListElement.AppendChild(headerFamilyElement);

            // 创建子节点<HeaderName>
            XmlElement headerNameElement = doc.CreateElement("HeaderName");
            attributeListElement.AppendChild(headerNameElement);

            // 创建子节点<HeaderVersion>
            XmlElement headerVersionElement = doc.CreateElement("HeaderVersion");
            headerVersionElement.InnerText = "0.1";
            attributeListElement.AppendChild(headerVersionElement);

            // 创建子节点<Interface>
            XmlElement interfaceElement = doc.CreateElement("Interface");
            attributeListElement.AppendChild(interfaceElement);

            // 创建子节点<Sections>
            XmlElement sectionsElement = doc.CreateElement("Sections", "http://www.siemens.com/automation/Openness/SW/Interface/v5");
            interfaceElement.AppendChild(sectionsElement);

            // 创建子节点<Section>，设置属性Name="Input"
            XmlElement inputSectionElement = doc.CreateElement("Section");
            inputSectionElement.SetAttribute("Name", "Input");
            sectionsElement.AppendChild(inputSectionElement);

            // 创建子节点<Member>，设置属性Name="Initial_Call"、Datatype="Bool"、Accessibility="Public"、Informative="true"
            XmlElement initialCallMemberElement = doc.CreateElement("Member");
            initialCallMemberElement.SetAttribute("Name", "Initial_Call");
            initialCallMemberElement.SetAttribute("Datatype", "Bool");
            initialCallMemberElement.SetAttribute("Accessibility", "Public");
            initialCallMemberElement.SetAttribute("Informative", "true");
            inputSectionElement.AppendChild(initialCallMemberElement);

            // 在<Member>下创建<Comment>
            XmlElement initialCallCommentElement = doc.CreateElement("Comment");
            initialCallMemberElement.AppendChild(initialCallCommentElement);

            // 在<Comment>下创建<MultiLanguageText>，设置属性Lang="en-US"，并设置文本内容
            XmlElement initialCallMultiLanguageTextElement = doc.CreateElement("MultiLanguageText");
            initialCallMultiLanguageTextElement.SetAttribute("Lang", "en-US");
            initialCallMultiLanguageTextElement.InnerText = "Initial call of this OB";
            initialCallCommentElement.AppendChild(initialCallMultiLanguageTextElement);

            // 创建子节点<Member>，设置属性Name="Remanence"、Datatype="Bool"、Accessibility="Public"、Informative="true"
            XmlElement remanenceMemberElement = doc.CreateElement("Member");
            remanenceMemberElement.SetAttribute("Name", "Remanence");
            remanenceMemberElement.SetAttribute("Datatype", "Bool");
            remanenceMemberElement.SetAttribute("Accessibility", "Public");
            remanenceMemberElement.SetAttribute("Informative", "true");
            inputSectionElement.AppendChild(remanenceMemberElement);

            // 在<Member>下创建<Comment>
            XmlElement remanenceCommentElement = doc.CreateElement("Comment");
            remanenceMemberElement.AppendChild(remanenceCommentElement);

            // 在<Comment>下创建<MultiLanguageText>，设置属性Lang="en-US"，并设置文本内容
            XmlElement remanenceMultiLanguageTextElement = doc.CreateElement("MultiLanguageText");
            remanenceMultiLanguageTextElement.SetAttribute("Lang", "en-US");
            remanenceMultiLanguageTextElement.InnerText = "=True, if remanent data are available";
            remanenceCommentElement.AppendChild(remanenceMultiLanguageTextElement);

            // 创建子节点<Section>，设置属性Name="Temp"
            XmlElement inputTempSectionElement = doc.CreateElement("Section");
            inputTempSectionElement.SetAttribute("Name", "Temp");
            sectionsElement.AppendChild(inputTempSectionElement);

            // 创建子节点<Section>，设置属性Name="Temp"
            XmlElement inputConstantSectionElement = doc.CreateElement("Section");
            inputConstantSectionElement.SetAttribute("Name", "Constant");
            sectionsElement.AppendChild(inputConstantSectionElement);

            // 创建子节点<IsIECCheckEnabled>，并设置其值为"false"
            XmlElement isIECCheckEnabledElement = doc.CreateElement("IsIECCheckEnabled");
            isIECCheckEnabledElement.InnerText = "false";
            attributeListElement.AppendChild(isIECCheckEnabledElement);

            // 创建子节点<MemoryLayout>，并设置其值为"Optimized"
            XmlElement memoryLayoutElement = doc.CreateElement("MemoryLayout");
            memoryLayoutElement.InnerText = "Optimized";
            attributeListElement.AppendChild(memoryLayoutElement);

            // 创建子节点<Name>，并设置其值为"ModbusProject"
            XmlElement nameElement = doc.CreateElement("Name");
            nameElement.InnerText = $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace("/", "_")}ModbusMaster";
            attributeListElement.AppendChild(nameElement);

            //创建子节点 < Namespace >，并设置其值为空字符串
            XmlElement namespaceElement = doc.CreateElement("Namespace");
            attributeListElement.AppendChild(namespaceElement);

            // 创建子节点<Number>，并设置其值为"1"
            XmlElement numberElement = doc.CreateElement("Number");
            numberElement.InnerText = "1";
            attributeListElement.AppendChild(numberElement);

            // 创建子节点<ProgrammingLanguage>，并设置其值为"LAD"
            XmlElement programmingLanguageElement = doc.CreateElement("ProgrammingLanguage");
            programmingLanguageElement.InnerText = "LAD";
            attributeListElement.AppendChild(programmingLanguageElement);

            // 创建子节点<SecondaryType>，并设置其值为"ProgramCycle"
            XmlElement secondaryTypeElement = doc.CreateElement("SecondaryType");
            secondaryTypeElement.InnerText = "ProgramCycle";
            attributeListElement.AppendChild(secondaryTypeElement);

            // 创建子节点<SetENOAutomatically>，并设置其值为"false"
            XmlElement setENOAutomaticallyElement = doc.CreateElement("SetENOAutomatically");
            setENOAutomaticallyElement.InnerText = "false";
            attributeListElement.AppendChild(setENOAutomaticallyElement);





            // 创建 <ObjectList> 元素
            XmlElement objectListElement = doc.CreateElement("ObjectList");
            swBlocksObElement.AppendChild(objectListElement);

            // 创建第一个 <MultilingualText> 元素
            XmlElement multilingualTextElement1 = doc.CreateElement("MultilingualText");
            multilingualTextElement1.SetAttribute("ID", "1");
            multilingualTextElement1.SetAttribute("CompositionName", "Comment");

            // 在第一个 <MultilingualText> 元素下创建 <ObjectList> 元素
            XmlElement objectListElement1 = doc.CreateElement("ObjectList");

            // 创建 <MultilingualTextItem> 元素
            XmlElement multilingualTextItemElement1 = doc.CreateElement("MultilingualTextItem");
            multilingualTextItemElement1.SetAttribute("ID", "2");
            multilingualTextItemElement1.SetAttribute("CompositionName", "Items");

            // 在第一个 <MultilingualTextItem> 元素下创建 <AttributeList> 元素
            XmlElement attributeListElement1 = doc.CreateElement("AttributeList");

            // 创建 <Culture> 元素并设置文本内容为 "zh-CN"
            XmlElement cultureElement1 = doc.CreateElement("Culture");
            cultureElement1.InnerText = "zh-CN";

            // 创建 <Text> 元素并设置文本内容为具体内容
            XmlElement textElement1 = doc.CreateElement("Text");
            textElement1.InnerText = @"1.免责声明 
1.软件使用

本软件是为了提供便利和效率而设计的。开发者已尽力确保软件的功能性和稳定性，但无法保证软件在所有情况下都能无误运行。用户应自行承担使用软件的风险。

2.免责声明

开发者不对因使用或无法使用本软件而产生的任何直接、间接、偶然、特殊或惩罚性的损害承担责任，包括但不限于数据损失、设备损坏、业务中断、利润损失等。

3.软件限制

本软件可能存在已知或未知的缺陷和漏洞。开发者保留对软件进行修改、更新或停止支持的权利，且不保证提供任何形式的技术支持或更新服务。"; 

            // 将 <Culture> 元素和 <Text> 元素添加到 <AttributeList> 元素下
            attributeListElement1.AppendChild(cultureElement1);
            attributeListElement1.AppendChild(textElement1);

            // 将 <AttributeList> 元素添加到 <MultilingualTextItem> 元素下
            multilingualTextItemElement1.AppendChild(attributeListElement1);

            // 将 <MultilingualTextItem> 元素添加到 <ObjectList> 元素下
            objectListElement1.AppendChild(multilingualTextItemElement1);

            // 将 <ObjectList> 元素添加到第一个 <MultilingualText> 元素下
            multilingualTextElement1.AppendChild(objectListElement1);

            objectListElement.AppendChild(multilingualTextElement1);
            CreateCompileUnitTypeAElement(doc, objectListElement, 3, "LAD", "在S7-1200启动的第一个扫描周期，将数值4传送到在“Modbus_Comm_Load.DB”MODE，将工作模式设置为半双工 RS485两线模式", "设置通信端口模式=4 Modbus通信");
            CreateCompileUnitTypeBElement(doc, objectListElement, 8, list, modbusCommLoad, "LAD", "在S7-1200启动的第一个扫描周期，将Modbus RTU通信的RS485端口参数初始化为波特率：9600，无校验，无流控，响应超时1000ms（Modbus RTU默认为数据位：8位，停止位：1位） MB_DB指向\"Modbus_Master\"指令所使用的背景数据块引用", "Modbus主站初始化");
            CreateCompileUnitTypeCElement(doc, objectListElement, 13, "LAD", "初始化完成位使能MOVE指令，对步地址\"ModbusMasterOpenness.Step\"赋值1", "转到第一步");
            bool import = true;
            if (StartTIAPortalModel.ModbusInfo.Count <= 0)
            {
                import = false;
                Log.Warning("生成Modbus程序时未插入Modbus参数。");
                var dialogResult = await dialogHostService.Question("温馨提示", $"请插入Modbus参数。");
                return;
            }
            for (int i = 0; i < StartTIAPortalModel.ModbusInfo.Count; i++)
            {
                if (StartTIAPortalModel.ModbusInfo[i].DataType == null)
                {
                    import = false;
                    Log.Warning($"生成Modbus程序时序号为'{StartTIAPortalModel.ModbusInfo[i].MB_No}'的条目数据类型为null。");
                    var dialogResult = dialogHostService.Question("温馨提示", $"序号为'{StartTIAPortalModel.ModbusInfo[i].MB_No}'的条目数据类型不能为null。");
                    return;
                }
                if (StartTIAPortalModel.ModbusInfo[i].DATA_LEN == 0)
                {
                    import = false;
                    Log.Warning($"生成Modbus程序时序号为'{StartTIAPortalModel.ModbusInfo[i].MB_No}'的条目数据长度不能为0。");
                    var dialogResult = dialogHostService.Question("温馨提示", $"序号为'{StartTIAPortalModel.ModbusInfo[i].MB_No}'的条目数据长度不能为0。");
                    return;
                }
                if (StartTIAPortalModel.ModbusInfo[i].DATA_ADDR == 0)
                {
                    import = false;
                    Log.Warning($"生成Modbus程序时序号为'{StartTIAPortalModel.ModbusInfo[i].MB_No}'的条目数据地址不能为0。");
                    var dialogResult = dialogHostService.Question("温馨提示", $"序号为'{StartTIAPortalModel.ModbusInfo[i].MB_No}'的条目数据地址不能为0。");
                    return;
                }
                CreateCompileUnitElementONE(doc, objectListElement, 18 + i * 10, StartTIAPortalModel.ModbusInfo[i], "LAD", $"第一步：\"ModbusMasterOpenness.Step\"值={i + 1}时触发“Modbus_Master”指令读取 Modbus RTU 从站地址{StartTIAPortalModel.ModbusInfo[i].MB_ADDR}保持寄存器{StartTIAPortalModel.ModbusInfo[i].DATA_ADDR}地址开始的{StartTIAPortalModel.ModbusInfo[i].DATA_LEN}个字长的数据，将其存放于“\"ModbusMasterOpenness\".{StartTIAPortalModel.ModbusInfo[i].Name}{StartTIAPortalModel.ModbusInfo[i].MB_No}”指定的地址中", "Modbus主站读取保持寄存器数据");

                CreateCompileUnitElementOTWO(doc, objectListElement, 23 + i * 10, i == StartTIAPortalModel.ModbusInfo.Count - 1 ? true : false, "LAD", $"第{i + 1}步完成位或错误位作为条件转到第{i + 2}步，使能MOVE指令，对步地址\"ModbusMasterOpenness.Step\"赋值{i + 2}", $"转到第{i + 2}步");

            }

            if (import)
            {
                try
                {
                    UpdateLoading(true);
                    await Task.Run(async () =>
                    {
                        // 将 XML 文档保存到文件中
                        doc.Save($"ModbusMaster.xml");
                        ImportGlobalDB();
                        _newTIAPortal.ImportBlockGroups($"{Environment.CurrentDirectory}/ModbusMaster.xml",$"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace("/", "_")}Group");
                        _newTIAPortal.ImportBlockGroups($"{Environment.CurrentDirectory}/ModbusMasterDB.xml", $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace("/", "_")}Group");
                        _newTIAPortal.CreateInstanceDB();
                        File.Delete($"{Environment.CurrentDirectory}/ModbusMaster.xml");
                        File.Delete($"{Environment.CurrentDirectory}/ModbusMasterDB.xml");
                    });
                    UpdateLoading(false);

                }
                catch (Exception ex)
                {
                    Log.Fatal($"生成Modbus程序时发生错误：{ex.Message}。");
                    var dialogResult = dialogHostService.Question("致命错误", ex.Message);
                }

            }

        }

        private void ImportGlobalDB()
        {

            // 创建XmlDocument对象
            XmlDocument doc = new XmlDocument();

            // 创建xml声明节点
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(xmlDeclaration);

            // 创建根节点<Document>
            XmlElement documentElement = doc.CreateElement("Document");
            doc.AppendChild(documentElement);

            // 创建子节点<Engineering>
            XmlElement engineeringElement = doc.CreateElement("Engineering");
            engineeringElement.SetAttribute("version", $"{ConvertToDoubleWithNoTrailingZeros(SelectedOpennessVersion)}");
            documentElement.AppendChild(engineeringElement);

            // 创建子节点<SW.Blocks.OB>
            XmlElement swBlocksObElement = doc.CreateElement("SW.Blocks.GlobalDB");
            swBlocksObElement.SetAttribute("ID", "0");
            documentElement.AppendChild(swBlocksObElement);

            // 创建子节点<AttributeList>
            XmlElement attributeListElement = doc.CreateElement("AttributeList");
            swBlocksObElement.AppendChild(attributeListElement);
            // 创建子节点<AutoNumber>
            XmlElement autoNumberElement = doc.CreateElement("AutoNumber");
            autoNumberElement.InnerText = "true";
            attributeListElement.AppendChild(autoNumberElement);

            // 创建子节点<HeaderAuthor>
            XmlElement headerAuthorElement = doc.CreateElement("HeaderAuthor");
            attributeListElement.AppendChild(headerAuthorElement);

            // 创建子节点<HeaderFamily>
            XmlElement headerFamilyElement = doc.CreateElement("HeaderFamily");
            attributeListElement.AppendChild(headerFamilyElement);

            // 创建子节点<HeaderName>
            XmlElement headerNameElement = doc.CreateElement("HeaderName");
            attributeListElement.AppendChild(headerNameElement);

            // 创建子节点<HeaderVersion>
            XmlElement headerVersionElement = doc.CreateElement("HeaderVersion");
            headerVersionElement.InnerText = "0.1";
            attributeListElement.AppendChild(headerVersionElement);

            // 创建子节点<Interface>
            XmlElement interfaceElement = doc.CreateElement("Interface");
            attributeListElement.AppendChild(interfaceElement);

            // 创建子节点<Sections>
            XmlElement sectionsElement = doc.CreateElement("Sections", "http://www.siemens.com/automation/Openness/SW/Interface/v5");
            interfaceElement.AppendChild(sectionsElement);

            // 创建子节点<Section>，设置属性Name="Input"
            XmlElement inputSectionElement = doc.CreateElement("Section");
            inputSectionElement.SetAttribute("Name", "Static");
            sectionsElement.AppendChild(inputSectionElement);

            XmlElement initialCallMemberStepElement = doc.CreateElement("Member");
            initialCallMemberStepElement.SetAttribute("Name", "Step");
            initialCallMemberStepElement.SetAttribute("Datatype", "USInt");
            initialCallMemberStepElement.SetAttribute("Remanence", "NonRetain");
            initialCallMemberStepElement.SetAttribute("Accessibility", "Public");
            AddAttributeListElement(doc, initialCallMemberStepElement);
            inputSectionElement.AppendChild(initialCallMemberStepElement);

            XmlElement Comm_Load_DONEElement = doc.CreateElement("Member");
            Comm_Load_DONEElement.SetAttribute("Name", "Comm_Load_DONE");
            Comm_Load_DONEElement.SetAttribute("Datatype", "bool");
            Comm_Load_DONEElement.SetAttribute("Remanence", "NonRetain");
            Comm_Load_DONEElement.SetAttribute("Accessibility", "Public");
            AddAttributeListElement(doc, Comm_Load_DONEElement);
            inputSectionElement.AppendChild(Comm_Load_DONEElement);

            XmlElement Comm_Load_ERRORElement = doc.CreateElement("Member");
            Comm_Load_ERRORElement.SetAttribute("Name", "Comm_Load_ERROR");
            Comm_Load_ERRORElement.SetAttribute("Datatype", "bool");
            Comm_Load_ERRORElement.SetAttribute("Remanence", "NonRetain");
            Comm_Load_ERRORElement.SetAttribute("Accessibility", "Public");
            AddAttributeListElement(doc, Comm_Load_ERRORElement);
            inputSectionElement.AppendChild(Comm_Load_ERRORElement);

            XmlElement Comm_Load_STATUSElement = doc.CreateElement("Member");
            Comm_Load_STATUSElement.SetAttribute("Name", "Comm_Load_STATUS");
            Comm_Load_STATUSElement.SetAttribute("Datatype", "Word");
            Comm_Load_STATUSElement.SetAttribute("Remanence", "NonRetain");
            Comm_Load_STATUSElement.SetAttribute("Accessibility", "Public");
            AddAttributeListElement(doc, Comm_Load_STATUSElement);
            inputSectionElement.AppendChild(Comm_Load_STATUSElement);

            XmlElement initialCallMemberModbus_MasterElement = doc.CreateElement("Member");
            initialCallMemberModbus_MasterElement.SetAttribute("Name", "Modbus_Master");
            initialCallMemberModbus_MasterElement.SetAttribute("Datatype", $"Array[1..{StartTIAPortalModel.ModbusInfo.Count}] of Struct");
            initialCallMemberModbus_MasterElement.SetAttribute("Remanence", "NonRetain");
            initialCallMemberModbus_MasterElement.SetAttribute("Accessibility", "Public");
            AddAttributeListElement(doc, initialCallMemberModbus_MasterElement);

            XmlElement initialCallMemberDONEElement = doc.CreateElement("Member");
            initialCallMemberDONEElement.SetAttribute("Name", "DONE");
            initialCallMemberDONEElement.SetAttribute("Datatype", "Bool");
            AddAttributeListElement(doc, initialCallMemberDONEElement);
            initialCallMemberModbus_MasterElement.AppendChild(initialCallMemberDONEElement);

            XmlElement initialCallMemberBUSYElement = doc.CreateElement("Member");
            initialCallMemberBUSYElement.SetAttribute("Name", "BUSY");
            initialCallMemberBUSYElement.SetAttribute("Datatype", "Bool");
            AddAttributeListElement(doc, initialCallMemberBUSYElement);
            initialCallMemberModbus_MasterElement.AppendChild(initialCallMemberBUSYElement);

            XmlElement initialCallMemberERRORElement = doc.CreateElement("Member");
            initialCallMemberERRORElement.SetAttribute("Name", "ERROR");
            initialCallMemberERRORElement.SetAttribute("Datatype", "Bool");
            AddAttributeListElement(doc, initialCallMemberERRORElement);
            initialCallMemberModbus_MasterElement.AppendChild(initialCallMemberERRORElement);

            XmlElement initialCallMemberSTATUSElement = doc.CreateElement("Member");
            initialCallMemberSTATUSElement.SetAttribute("Name", "STATUS");
            initialCallMemberSTATUSElement.SetAttribute("Datatype", "Word");
            AddAttributeListElement(doc, initialCallMemberSTATUSElement);
            initialCallMemberModbus_MasterElement.AppendChild(initialCallMemberSTATUSElement);

            inputSectionElement.AppendChild(initialCallMemberModbus_MasterElement);


            for (int i = 0; i < StartTIAPortalModel.ModbusInfo.Count; i++)
            {
                XmlElement initialCallMemberElement = doc.CreateElement("Member");
                initialCallMemberElement.SetAttribute("Name", $"{(StartTIAPortalModel.ModbusInfo[i].MODE == 0 ? $"Read_DATA_{StartTIAPortalModel.ModbusInfo[i].Name}{StartTIAPortalModel.ModbusInfo[i].MB_No}" : $"Write_DATA_{StartTIAPortalModel.ModbusInfo[i].Name}{StartTIAPortalModel.ModbusInfo[i].MB_No}")}");
                initialCallMemberElement.SetAttribute("Datatype", $"Array[1..{StartTIAPortalModel.ModbusInfo[i].DATA_LEN}] of {StartTIAPortalModel.ModbusInfo[i].DataType}");
                initialCallMemberElement.SetAttribute("Remanence", "NonRetain");
                initialCallMemberElement.SetAttribute("Accessibility", "Public");
                AddAttributeListElement(doc, initialCallMemberElement);
                inputSectionElement.AppendChild(initialCallMemberElement);

            }

            // 创建 IsOnlyStoredInLoadMemory 元素
            XmlElement isOnlyStoredInLoadMemory = doc.CreateElement("IsOnlyStoredInLoadMemory");
            isOnlyStoredInLoadMemory.InnerText = "false";
            attributeListElement.AppendChild(isOnlyStoredInLoadMemory);

            // 创建 IsWriteProtectedInAS 元素
            XmlElement isWriteProtectedInAS = doc.CreateElement("IsWriteProtectedInAS");
            isWriteProtectedInAS.InnerText = "false";
            attributeListElement.AppendChild(isWriteProtectedInAS);

            // 创建 MemoryLayout 元素
            XmlElement memoryLayout = doc.CreateElement("MemoryLayout");
            memoryLayout.InnerText = "Standard";
            attributeListElement.AppendChild(memoryLayout);

            // 创建 Name 元素
            XmlElement name = doc.CreateElement("Name");
            name.InnerText = $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB";
            attributeListElement.AppendChild(name);

            // 创建 Namespace 元素（这里没有内容，所以创建一个空元素）
            XmlElement namespaceElement = doc.CreateElement("Namespace");
            attributeListElement.AppendChild(namespaceElement);

            // 创建 Number 元素
            XmlElement number = doc.CreateElement("Number");
            number.InnerText = "3";
            attributeListElement.AppendChild(number);

            // 创建 ProgrammingLanguage 元素
            XmlElement programmingLanguage = doc.CreateElement("ProgrammingLanguage");
            programmingLanguage.InnerText = "DB";
            attributeListElement.AppendChild(programmingLanguage);

            // 创建根元素 ObjectList
            XmlElement root = doc.CreateElement("ObjectList");
            swBlocksObElement.AppendChild(root);

            // 创建第一个 MultilingualText 元素
            XmlElement multilingualText1 = doc.CreateElement("MultilingualText");
            root.AppendChild(multilingualText1);
            multilingualText1.SetAttribute("ID", "1");
            multilingualText1.SetAttribute("CompositionName", "Comment");

            // 创建 MultilingualText 的 ObjectList 子元素
            XmlElement multilingualTextObjectList1 = doc.CreateElement("ObjectList");
            multilingualText1.AppendChild(multilingualTextObjectList1);

            // 创建 MultilingualTextItem 元素
            XmlElement multilingualTextItem1 = doc.CreateElement("MultilingualTextItem");
            multilingualTextObjectList1.AppendChild(multilingualTextItem1);
            multilingualTextItem1.SetAttribute("ID", "2");
            multilingualTextItem1.SetAttribute("CompositionName", "Items");

            // 创建 AttributeList 元素
            XmlElement attributeList1 = doc.CreateElement("AttributeList");
            multilingualTextItem1.AppendChild(attributeList1);

            // 创建 Culture 元素
            XmlElement culture1 = doc.CreateElement("Culture");
            culture1.InnerText = "zh-CN";
            attributeList1.AppendChild(culture1);

            // 创建 Text 元素（这里没有文本内容，所以只创建空元素）
            XmlElement text1 = doc.CreateElement("Text");
            attributeList1.AppendChild(text1);

            // 创建第一个 MultilingualText 元素
            XmlElement multilingualText2 = doc.CreateElement("MultilingualText");
            root.AppendChild(multilingualText2);
            multilingualText2.SetAttribute("ID", "3");
            multilingualText2.SetAttribute("CompositionName", "Title");

            // 创建 MultilingualText 的 ObjectList 子元素
            XmlElement multilingualTextObjectList2 = doc.CreateElement("ObjectList");
            multilingualText2.AppendChild(multilingualTextObjectList2);

            // 创建 MultilingualTextItem 元素
            XmlElement multilingualTextItem2 = doc.CreateElement("MultilingualTextItem");
            multilingualTextObjectList2.AppendChild(multilingualTextItem2);
            multilingualTextItem2.SetAttribute("ID", "4");
            multilingualTextItem2.SetAttribute("CompositionName", "Items");

            // 创建 AttributeList 元素
            XmlElement attributeList2 = doc.CreateElement("AttributeList");
            multilingualTextItem2.AppendChild(attributeList2);

            // 创建 Culture 元素
            XmlElement culture2 = doc.CreateElement("Culture");
            culture2.InnerText = "zh-CN";
            attributeList2.AppendChild(culture2);

            // 创建 Text 元素（这里没有文本内容，所以只创建空元素）
            XmlElement text2 = doc.CreateElement("Text");
            attributeList2.AppendChild(text2);

            // 将 XML 文档保存到文件中
            doc.Save($"ModbusMasterDB.xml");

        }

        private void QueryInstructionData(string name, string Version)
        {
            try
            {
                //连接数据库
                SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = "Data Source =" + Environment.CurrentDirectory + "/TIAPortalInstructionData.db",
                    DbType = SqlSugar.DbType.Sqlite,
                    IsAutoCloseConnection = true,
                });
                var conModels = new List<IConditionalModel>();
                conModels.Add(new ConditionalModel
                {
                    FieldName = "Name",
                    ConditionalType = ConditionalType.Equal,
                    FieldValue = name
                });
                conModels.Add(new ConditionalModel
                {
                    FieldName = "Version",
                    ConditionalType = ConditionalType.Equal,
                    FieldValue = Version
                });
                list = db.Queryable<InstructionData>().AS("InstructionData").Where(conModels).ToList();

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
        }


        static void AddSymbolElement(XmlDocument doc, XmlElement parentElement, int uid, bool symbol, string componentName)
        {
            XmlElement accessElement = doc.CreateElement("Access");
            accessElement.SetAttribute("Scope", "GlobalVariable");
            accessElement.SetAttribute("UId", $"{uid}");

            string[] parts = componentName.Split('.');
            if (symbol)
            {
                XmlElement symbolElement = doc.CreateElement("Symbol");
                accessElement.AppendChild(symbolElement);
                for (int i = 0; i < parts.Length; i++)
                {
                    XmlElement componentElement = doc.CreateElement("Component");
                    componentElement.SetAttribute("Name", parts[i]);
                    symbolElement.AppendChild(componentElement);
                }
            }
            else
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    XmlElement componentElement = doc.CreateElement("Constant");
                    componentElement.SetAttribute("Name", parts[i]);
                    accessElement.AppendChild(componentElement);
                }
            }

            parentElement.AppendChild(accessElement);
        }

        static void AddSymbolGlobalConstantElement(XmlDocument doc, XmlElement parentElement, int uid, bool symbol, string componentName)
        {
            XmlElement accessElement = doc.CreateElement("Access");
            accessElement.SetAttribute("Scope", "GlobalConstant");
            accessElement.SetAttribute("UId", $"{uid}");

            string[] parts = componentName.Split('.');
            if (symbol)
            {
                XmlElement symbolElement = doc.CreateElement("Symbol");
                accessElement.AppendChild(symbolElement);
                for (int i = 0; i < parts.Length; i++)
                {
                    XmlElement componentElement = doc.CreateElement("Component");
                    componentElement.SetAttribute("Name", parts[i]);
                    symbolElement.AppendChild(componentElement);
                }
            }
            else
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    XmlElement componentElement = doc.CreateElement("Constant");
                    componentElement.SetAttribute("Name", parts[i]);
                    accessElement.AppendChild(componentElement);
                }
            }

            parentElement.AppendChild(accessElement);
        }

        private void AddSymbolArrayElement(XmlDocument doc, XmlElement parentElement, int uid, int number, string name)
        {
            XmlElement accessElement = doc.CreateElement("Access");
            accessElement.SetAttribute("Scope", "GlobalVariable");
            accessElement.SetAttribute("UId", $"{uid}");


            XmlElement symbolElement = doc.CreateElement("Symbol");
            accessElement.AppendChild(symbolElement);

            XmlElement componentElement = doc.CreateElement("Component");
            componentElement.SetAttribute("Name", $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB");
            symbolElement.AppendChild(componentElement);


            XmlElement componentElementArray = doc.CreateElement("Component");
            componentElementArray.SetAttribute("Name", "Modbus_Master");
            componentElementArray.SetAttribute("AccessModifier", "Array");

            XmlElement accessElementArray = doc.CreateElement("Access");
            accessElementArray.SetAttribute("Scope", "LiteralConstant");
            componentElementArray.AppendChild(accessElementArray);

            XmlElement constantElement = doc.CreateElement("Constant");
            accessElementArray.AppendChild(constantElement);

            XmlElement constantTypeElement = doc.CreateElement("ConstantType");
            constantTypeElement.InnerText = "DInt";
            constantElement.AppendChild(constantTypeElement);

            XmlElement constantValueElement = doc.CreateElement("ConstantValue");
            constantValueElement.InnerText = $"{number}";
            constantElement.AppendChild(constantValueElement);

            symbolElement.AppendChild(componentElementArray);

            XmlElement componentElement3 = doc.CreateElement("Component");
            componentElement3.SetAttribute("Name", name);
            symbolElement.AppendChild(componentElement3);

            parentElement.AppendChild(accessElement);
        }

        static void AddAttributeListElement(XmlDocument doc, XmlElement parentElement)
        {
            // 创建根元素 AttributeList
            XmlElement attributeListElement = doc.CreateElement("AttributeList");
            parentElement.AppendChild(attributeListElement);

            // 创建并设置第一个 BooleanAttribute 元素
            XmlElement externalAccessibleElement = doc.CreateElement("BooleanAttribute");
            externalAccessibleElement.SetAttribute("Name", "ExternalAccessible");
            externalAccessibleElement.SetAttribute("SystemDefined", "true");
            externalAccessibleElement.InnerText = "true";
            attributeListElement.AppendChild(externalAccessibleElement);

            // 创建并设置第二个 BooleanAttribute 元素
            XmlElement externalVisibleElement = doc.CreateElement("BooleanAttribute");
            externalVisibleElement.SetAttribute("Name", "ExternalVisible");
            externalVisibleElement.SetAttribute("SystemDefined", "true");
            externalVisibleElement.InnerText = "true";
            attributeListElement.AppendChild(externalVisibleElement);

            // 创建并设置第三个 BooleanAttribute 元素
            XmlElement externalWritableElement = doc.CreateElement("BooleanAttribute");
            externalWritableElement.SetAttribute("Name", "ExternalWritable");
            externalWritableElement.SetAttribute("SystemDefined", "true");
            externalWritableElement.InnerText = "true";
            attributeListElement.AppendChild(externalWritableElement);

            // 创建并设置第四个 BooleanAttribute 元素
            XmlElement setPointElement = doc.CreateElement("BooleanAttribute");
            setPointElement.SetAttribute("Name", "SetPoint");
            setPointElement.SetAttribute("SystemDefined", "true");
            setPointElement.InnerText = "false";
            attributeListElement.AppendChild(setPointElement);

        }

        static void AddConstantElement(XmlDocument doc, XmlElement parentElement, int uid, string componentName)
        {
            XmlElement accessElement = doc.CreateElement("Access");
            accessElement.SetAttribute("Scope", "LiteralConstant");
            accessElement.SetAttribute("UId", $"{uid}");
            string[] parts = componentName.Split('.');
            XmlElement constantElement = doc.CreateElement("Constant");
            XmlElement constantTypeElement = doc.CreateElement("ConstantType");
            constantTypeElement.InnerText = parts[0];
            constantElement.AppendChild(constantTypeElement);
            XmlElement constantValueElement = doc.CreateElement("ConstantValue");
            constantValueElement.InnerText = parts[1];
            constantElement.AppendChild(constantValueElement);
            accessElement.AppendChild(constantElement);
            parentElement.AppendChild(accessElement);

        }
        static void AddComponenElement(XmlDocument doc, XmlElement parentElement, string uid, string componentName)
        {
            XmlElement accessElement = doc.CreateElement("Access");
            accessElement.SetAttribute("Scope", "GlobalVariable");
            accessElement.SetAttribute("UId", uid);

            XmlElement componentElement = doc.CreateElement("Constant");
            componentElement.SetAttribute("Name", componentName);
            accessElement.AppendChild(componentElement);

            parentElement.AppendChild(accessElement);
        }

        static void AddWireElement(XmlDocument doc, XmlElement parentElement, string uid, string identUid, string nameConUid, string name, bool reversal)
        {
            XmlElement wireElement = doc.CreateElement("Wire");
            wireElement.SetAttribute("UId", uid);

            XmlElement identConElement = doc.CreateElement("IdentCon");
            identConElement.SetAttribute("UId", identUid);
            XmlElement nameConElement = doc.CreateElement("NameCon");
            nameConElement.SetAttribute("UId", nameConUid);
            nameConElement.SetAttribute("Name", name);
            if (reversal)
            {
                wireElement.AppendChild(nameConElement);
                wireElement.AppendChild(identConElement);
            }
            else
            {
                wireElement.AppendChild(identConElement);
                wireElement.AppendChild(nameConElement);
            }
            parentElement.AppendChild(wireElement);

        }

        private async void OpenTIAPortal()
        {
            Log.Information("打开TIA Portal中。");
            UpdateLoading(true);
            await Task.Run(async () =>
            {
                instTIA = _newTIAPortal.CreateTIAinstance(true);
            });
            UpdateLoading(false);
            Log.Information("TIA Portal已被打开。");
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

        private StartTIAPortalModel startTIAPortalModel;

        public StartTIAPortalModel StartTIAPortalModel
        {
            get { return startTIAPortalModel; }
            set { startTIAPortalModel = value; RaisePropertyChanged(); }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
        }
    }
}
