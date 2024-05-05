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
using System.Reflection;
using Newtonsoft.Json;
using static TIA程序生成.Common.Models.Configuration;
using System.Xml.Serialization;
using Siemens.Engineering.CrossReference;
using Access = TIA程序生成.Common.Models.Configuration.Access;
using System.Diagnostics.Metrics;
using DryIoc;

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
        bool detectionResult = false;
        string readData = string.Empty;
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
            string readJson = File.ReadAllText("SelectPath.json");
            readData = JsonConvert.DeserializeObject<dynamic>(readJson);
            InitializationCheck();
            if (detectionResult)
            {
                GetTiaPortalProcess();
            }
        }



        private async void Execute(string obj)
        {
            if (detectionResult)
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
            else
            {
                var dialogResult1 = dialogHostService.Question("温馨提示", "缺少DLL文件，请检查TIA Portal安装路径，在系统设置页更改路径。");
            }
        }



        //private void ImportProgramsxml()
        //{

        //}


        private string ImportProgramsxml(string originalXml, int ID, string[] ConstantValue, string[] ComponentValues, string[] TextIndex, string ComponentName)
        {

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(originalXml);

            // 为命名空间定义一个前缀
            string prefix = "ns";
            int constantValueIndex = 0;
            // 为XmlNamespaceManager设置命名空间
            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace(prefix, "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4");
            // 使用带有命名空间的XPath表达式选择所有的ConstantValue节点并替换文本
            XmlNodeList constantValueNodes = xmlDoc.SelectNodes("//" + prefix + ":ConstantValue", nsManager);
            foreach (XmlNode constantValueNode in constantValueNodes)
            {
                if (constantValueNode.NodeType == XmlNodeType.Element)
                {
                    constantValueNode.InnerText = ConstantValue[constantValueIndex];
                    constantValueIndex++;
                }
            }

            // 选择所有的ID属性并替换它们的值
            XmlNodeList idAttributes = xmlDoc.SelectNodes("//@ID");
            foreach (XmlNode idAttribute in idAttributes)
            {
                if (idAttribute is XmlAttribute)
                {
                    idAttribute.Value = String.Format("{0:X}", ID);
                    ID++;
                }
            }

            int arrayIndex = 0;
            // 为命名空间创建XmlNamespaceManager
            XmlNamespaceManager nsManager1 = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager1.AddNamespace("ns", "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4");

            // 使用命名空间前缀的XPath选择所有Symbol节点
            XmlNodeList symbolNodes = xmlDoc.SelectNodes("//ns:Symbol", nsManager1);

            foreach (XmlNode symbolNode in symbolNodes)
            {
                // 在每个Symbol节点中，选择所有Component节点（本地选择，不使用//）
                XmlNodeList componentNodes = symbolNode.SelectNodes("ns:Component", nsManager1);

                foreach (XmlNode componentNode in componentNodes)
                {
                    if (componentNode.Attributes["Name"] != null)
                    {
                        componentNode.Attributes["Name"].Value = ComponentValues[arrayIndex];
                        arrayIndex++;
                    }
                }
            }

            XmlNodeList constantNodes = xmlDoc.SelectNodes("//ns:Constant", nsManager1);
            foreach (XmlNode constantNode in constantNodes)
            {
                if (constantNode.Attributes["Name"] != null)
                {
                    constantNode.Attributes["Name"].Value = ComponentName;
                    arrayIndex++;
                }
            }

            int textIndex = 0;
            XmlNodeList textNodes = xmlDoc.SelectNodes("//Text");
            foreach (XmlNode textNode in textNodes)
            {
                if (textNode.NodeType == XmlNodeType.Element)
                {
                    textNode.InnerText = TextIndex[textIndex];
                    textIndex++;
                }
            }
            return xmlDoc.OuterXml.ToString();
        }

        private void ImportProgramsxml123(string Name, int Number, string ProgrammingLanguage, string[] Items)
        {
            // 创建Document对象，并填充其属性
            Configuration.Document document = new Configuration.Document
            {
                Engineering = new Configuration.Engineering { Version = "V18" },
                SWBlocksOB = new Configuration.SWBlocksOB
                {
                    ID = 0,
                    AttributeList = new Configuration.BlockAttributeList
                    {
                        Name = Name,
                        Namespace = string.Empty, // 虽然是空的，但仍然需要一个值
                        Number = Number,
                        ProgrammingLanguage = ProgrammingLanguage,
                    },

                    ObjectList = new Configuration.BlockObjectList
                    {
                        Items = new List<object>()
                    }
                }
            };

            // 为MultilingualText创建多个条目
            int multilingualTextID = 1; // 用于唯一标识MultilingualText的ID
            int multilingualTextItemID = 2; // 用于唯一标识MultilingualTextItem的ID
            // 现在Items列表已确保被初始化，可以安全地添加新项
            document.SWBlocksOB.ObjectList.Items.Add(new Configuration.MultilingualText
            {
                ID = multilingualTextID++,
                ObjectList = new Configuration.ObjectList
                {
                    // 假设MultilingualTextItem的CompositionName是固定的
                    MultilingualTextItem = new Configuration.MultilingualTextItem
                    {
                        ID = multilingualTextItemID++,
                        CompositionName = "Items",
                        AttributeList = new Configuration.AttributeList
                        {
                            Culture = "zh-CN",
                            Text = "sd"
                        }
                    }
                }
            });

            for (int i = 0; i < 5; i++)
            {
                document.SWBlocksOB.ObjectList.Items.Add(new Configuration.SWBlocksCompileUnit
                {
                    ID = i + 1,
                    CompositionName = $"ZMX{i}",
                    AttributeList = new Configuration.SWBlocksCompileUnitAttributeList
                    {
                        NetworkSource = new SWBlocksCompileUnitNetworkSource
                        {
                            FlgNet = new SWBlocksCompileUnitFlgNet
                            {

                            }
                        }
                    },
                    ObjectList = new Configuration.SWBlocksCompileUnitObjectList
                    {
                        MultilingualTexts = new List<Configuration.MultilingualText>
                        {
                           new Configuration.MultilingualText
                           {
                                ID = multilingualTextID++,
                                ObjectList=new ObjectList
                                {
                                    MultilingualTextItem=new Configuration.MultilingualTextItem
                                    {
                                        ID= multilingualTextID++,
                                        AttributeList = new Configuration.AttributeList
                                        {
                                            Culture = "zh-CN",
                                            Text = "sd"
                                        }
                                    }
                                }
                           },
                           new Configuration.MultilingualText
                           {
                                ID = multilingualTextID++,
                                ObjectList=new ObjectList
                                {
                                    MultilingualTextItem=new Configuration.MultilingualTextItem
                                    {
                                        ID= multilingualTextID++,
                                        AttributeList = new Configuration.AttributeList
                                        {
                                            Culture = "zh-CN",
                                            Text = "sd"
                                        }
                                    }
                                }
                           }
                        }
                    }
                });
            }
            //创建我们自己的命名空间
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            //添加一个空的命名空间和空的值
            ns.Add("", "");

            // 创建XmlSerializer的实例，指定要序列化的类型
            XmlSerializer serializer = new XmlSerializer(typeof(Configuration.Document));

            // 指定要保存的XML文件路径
            string filePath = "document.xml";

            // 使用FileStream来写入文件
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                // 执行序列化，将对象转换为XML格式并写入文件
                serializer.Serialize(fs, document, ns);
                System.Windows.MessageBox.Show("XML file saved to: " + filePath);
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

            if (File.Exists($"{readData}\\Portal V{ConvertToDoubleWithNoTrailingZeros(SelectedOpennessVersion)}\\PublicAPI\\V{ConvertToDoubleWithNoTrailingZeros(SelectedOpennessVersion)}\\Siemens.Engineering.dll"))
            {

            }
            else
            {
                detectionResult = false;
                Log.Error("Siemens.Engineering.dll文件不存在。");
                var dialogResult1 = dialogHostService.Question("温馨提示", "Siemens.Engineering.dll文件不存在，请检查TIA Portal安装路径，在系统设置页更改路径。");
                return;
            }

            if (File.Exists($"{readData}\\Portal V{ConvertToDoubleWithNoTrailingZeros(SelectedOpennessVersion)}\\PublicAPI\\V{ConvertToDoubleWithNoTrailingZeros(SelectedOpennessVersion)}\\Siemens.Engineering.Hmi.dll"))
            {
                detectionResult = true;
            }
            else
            {
                detectionResult = false;
                Log.Error("Siemens.Engineering.Hmi.dll文件不存在。");
                var dialogResult1 = dialogHostService.Question("温馨提示", "Siemens.Engineering.Hmi.dll文件不存在，请检查TIA Portal安装路径，在系统设置页更改路径");
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
            if (string.IsNullOrWhiteSpace(StartTIAPortalModel.TiaPortalDevicesPlcBlockName))
            {
                var dialogResult = dialogHostService.Question("温馨提示", "请选择要导出的块。");
                Log.Warning("导出块时未选择块名称。");
                return;
            }
            Log.Information($"将'{StartTIAPortalModel.TiaPortalDevicesPlcBlockName}'块导出到路径：{StartTIAPortalModel.BlockExportPath}。");
            string message = _newTIAPortal.ExportRegularBlock(StartTIAPortalModel.BlockExportPath, StartTIAPortalModel.TiaPortalDevicesPlcBlockName);
            if (!string.IsNullOrWhiteSpace(message))
            {
                var dialogResult = dialogHostService.Question("温馨提示", message);
            }
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
            try
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
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }

        private async void GetTiaPortalProcess()
        {
            //try
            //{
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
                tiaPortalProcess.Append(i == StartTIAPortalModel.TiaPortalProcess.Count - 1 ? "。" : "，");
            }
            Log.Information($"获取TiaPortal进程成功。获取到的进程:{tiaPortalProcess}");
            //}
            //catch (Exception ex)
            //{
            //    //var dialogResult = dialogHostService.Question("致命错误", ex.Message);
            //    System.Windows.MessageBox.Show(ex.Message);
            //    Log.Error(ex.ToString());
            //}

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
                string message = string.Empty;
                await Task.Run(async () =>
                {
                    //创建TIA Portal项目，文件路径和项目名必填，项目作者和项目注释可选填。
                    message = _newTIAPortal.CreateTIAprj(@"" + StartTIAPortalModel.SelectPath, StartTIAPortalModel.ProjectName, StartTIAPortalModel.ProjectAuthor, StartTIAPortalModel.ProjectComment);
                });
                UpdateLoading(false);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    Log.Warning($"创建项目中发生错误：{message}。");
                    var dialogResult = dialogHostService.Question("温馨提示", message); return;
                }
                Log.Information($"'{StartTIAPortalModel.ProjectName}'项目创建成功。");
            }
        }



        string ModbusParity;
        private async void ImportPrograms()
        {

            bool import = true;
            if (StartTIAPortalModel.ModbusInfo.Count <= 0)
            {
                import = false;
                Log.Warning("生成Modbus程序时未插入Modbus参数。");
                var dialogResult = await dialogHostService.Question("温馨提示", $"请插入Modbus参数。");
                return;
            }
            string Namespace = float.Parse(SelectedOpennessVersion) >= 18.0 ? "<Namespace/>" : "";
            string Document = $"<Document>\r\n<Engineering version=\"V{(int)Math.Floor(Double.Parse((SelectedOpennessVersion)))}\"/>" + originalXml4 + Namespace + originalXml5;
            string DocumentDB = $"<Document>\r\n<Engineering version=\"V{(int)Math.Floor(Double.Parse((SelectedOpennessVersion)))}\"/>" + originalXml6 + $"<Member Name=\"Modbus_Master\" Datatype=\"Array[1..{StartTIAPortalModel.ModbusInfo.Count}] of Struct\" Remanence=\"NonRetain\" Accessibility=\"Public\">" + originalXml7;
            string selectedPort = $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace(" / ", "_")}ModbusMasterDB";
            int ID = 18;

            switch (StartTIAPortalModel.ModbusParity)
            {
                case "None":
                    ModbusParity = "0";
                    break;
                case "Odd":
                    ModbusParity = "1";
                    break;
                case "Even":
                    ModbusParity = "2";
                    break;
            }
            string[] ComponentValues2 = { selectedPort, "Comm_Load_DONE", "Modbus_Master_DB", "MB_DB", selectedPort, "Comm_Load_DONE", selectedPort, "Comm_Load_ERROR", selectedPort, "Comm_Load_STATUS", selectedPort, "Modbus_Master", "STATUS", "Modbus_Comm_Load_DB" };
            string[] ConstantValue2 = { $"{StartTIAPortalModel.ModbusBaud}", $"{ModbusParity}", $"0", $"0", $"0", $"1000" };
            string[] TextIndex2 = { $"Modbus主站初始化", $"在S7-1200启动的第一个扫描周期，将Modbus RTU通信的RS485端口参数初始化为波特率：{StartTIAPortalModel.ModbusBaud}，校验:{StartTIAPortalModel.ModbusParity}，无流控，响应超时1000ms（Modbus RTU默认为数据位：8位，停止位：1位） MB_DB指向\"Modbus_Master\"指令所使用的背景数据块引用" };
            string[] ComponentValues3 = { selectedPort, "Comm_Load_DONE", selectedPort, "Step" };
            string[] ConstantValue3 = { "1" };
            string[] TextIndex3 = { $"转到第1步", $"初始化完成位使能MOVE指令，对步地址\"ModbusMasterOpenness.Step\"赋值1" };
            Document = Document + ImportProgramsxml(originalXml2, 8, ConstantValue2, ComponentValues2, TextIndex2, $"Local~{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace("/", "_")}");
            Document = Document + ImportProgramsxml(originalXml3, 13, ConstantValue3, ComponentValues3, TextIndex3, "");
            
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
                string[] ComponentValues = { selectedPort, "Step", selectedPort, $"Read_DATA_{StartTIAPortalModel.ModbusInfo[i].Name}{StartTIAPortalModel.ModbusInfo[i].MB_No}", selectedPort, "Modbus_Master", "DONE", selectedPort, "Modbus_Master", "BUSY", selectedPort, "Modbus_Master", "ERROR", selectedPort, "Modbus_Master", "STATUS", "Modbus_Master_DB", selectedPort, "Step", selectedPort, "Modbus_Master", "DONE", selectedPort, "Modbus_Master", "ERROR", selectedPort, "Step" };
                string[] ConstantValue = { $"{i + 1}", $"{StartTIAPortalModel.ModbusInfo[i].MB_ADDR}", $"{StartTIAPortalModel.ModbusInfo[i].MODE}", $"{StartTIAPortalModel.ModbusInfo[i].DATA_ADDR}", $"{StartTIAPortalModel.ModbusInfo[i].DATA_LEN}", $"{i + 1}", $"{i + 1}", $"{i + 1}", $"{i + 1}" };
                string[] TextIndex = { "Modbus主站读取保持寄存器数据", $"第{i + 1}步：\"ModbusMasterOpenness.Step\"值={i + 1}时触发“Modbus_Master”指令读取 Modbus RTU 从站地址{StartTIAPortalModel.ModbusInfo[i].MB_ADDR}保持寄存器{StartTIAPortalModel.ModbusInfo[i].DATA_ADDR}地址开始的{StartTIAPortalModel.ModbusInfo[i].DATA_LEN}个字长的数据，将其存放于“\"ModbusMasterOpenness\".{StartTIAPortalModel.ModbusInfo[i].Name}”指定的地址中" };
                string[] ComponentValues1 = { selectedPort, "Step", selectedPort, "Modbus_Master", "DONE", selectedPort, "Modbus_Master", "ERROR", selectedPort, "Step" };
                string[] ConstantValue1 = { $"{i + 1}", $"{i + 1}", $"{i + 1}", $"{i + 2}" };
                string[] TextIndex1 = { $"转到第{i + 2}步", $"第{i + 1}步完成位或错误位作为条件转到第{i + 2}步，使能MOVE指令，对步地址\"ModbusMasterOpenness.Step\"赋值{i + 2}" };
                Document = Document + ImportProgramsxml(originalXml, ID + (i * 10), ConstantValue, ComponentValues, TextIndex, "");
                Document = Document + ImportProgramsxml(originalXml1, ID + (i * 10) + 5, ConstantValue1, ComponentValues1, TextIndex1, "");
                DocumentDB = DocumentDB + $"<Member Name=\"Read_DATA_{StartTIAPortalModel.ModbusInfo[i].Name}{i+1}\" Datatype=\"Array[1..{StartTIAPortalModel.ModbusInfo[i].DATA_LEN}] of {StartTIAPortalModel.ModbusInfo[i].DataType}\" Remanence=\"NonRetain\" Accessibility=\"Public\">\r\n<AttributeList>\r\n<BooleanAttribute Name=\"ExternalAccessible\" SystemDefined=\"true\">true</BooleanAttribute>\r\n<BooleanAttribute Name=\"ExternalVisible\" SystemDefined=\"true\">true</BooleanAttribute>\r\n<BooleanAttribute Name=\"ExternalWritable\" SystemDefined=\"true\">true</BooleanAttribute>\r\n<BooleanAttribute Name=\"SetPoint\" SystemDefined=\"true\">false</BooleanAttribute>\r\n</AttributeList>\r\n</Member>";
            }
            DocumentDB = DocumentDB + $"</Section>\r\n</Sections>\r\n</Interface>\r\n<IsOnlyStoredInLoadMemory>false</IsOnlyStoredInLoadMemory>\r\n<IsWriteProtectedInAS>false</IsWriteProtectedInAS>\r\n<MemoryLayout>Standard</MemoryLayout>\r\n<Name>{selectedPort}</Name>\r\n<Namespace/>\r\n<Number>3</Number>\r\n<ProgrammingLanguage>DB</ProgrammingLanguage>\r\n</AttributeList>\r\n<ObjectList>\r\n<MultilingualText ID=\"1\" CompositionName=\"Comment\">\r\n<ObjectList>\r\n<MultilingualTextItem ID=\"2\" CompositionName=\"Items\">\r\n<AttributeList>\r\n<Culture>zh-CN</Culture>\r\n<Text/>\r\n</AttributeList>\r\n</MultilingualTextItem>\r\n</ObjectList>\r\n</MultilingualText>\r\n<MultilingualText ID=\"3\" CompositionName=\"Title\">\r\n<ObjectList>\r\n<MultilingualTextItem ID=\"4\" CompositionName=\"Items\">\r\n<AttributeList>\r\n<Culture>zh-CN</Culture>\r\n<Text/>\r\n</AttributeList>\r\n</MultilingualTextItem>\r\n</ObjectList>\r\n</MultilingualText>\r\n</ObjectList>\r\n</SW.Blocks.GlobalDB>\r\n</Document>";



            if (import)
            {
                try
                {
                    UpdateLoading(true);
                    await Task.Run(async () =>
                    {
                        // 将 XML 文档保存到文件中
                        Document = Document + $"<MultilingualText ID=\"{String.Format("{0:X}", (StartTIAPortalModel.ModbusInfo.Count) * 10 + 18)}\" CompositionName=\"Title\">\r\n<ObjectList>\r\n<MultilingualTextItem ID=\"{String.Format("{0:X}", (StartTIAPortalModel.ModbusInfo.Count) * 10 + 19)}\" CompositionName=\"Items\">\r\n<AttributeList>\r\n<Culture>zh-CN</Culture>\r\n<Text/>\r\n</AttributeList>\r\n</MultilingualTextItem>\r\n</ObjectList>\r\n</MultilingualText>\r\n</ObjectList>\r\n</SW.Blocks.OB>\r\n</Document>";
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(Document);
                        xmlDoc.Save("ModbusMaster.xml");
                        XmlDocument xmlDBDoc = new XmlDocument();
                        xmlDBDoc.LoadXml(DocumentDB);
                        xmlDBDoc.Save("ModbusMasterDB.xml");
                        _newTIAPortal.ImportBlockGroups($"{Environment.CurrentDirectory}/ModbusMaster.xml", $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace("/", "_")}Group");
                        _newTIAPortal.ImportBlockGroups($"{Environment.CurrentDirectory}/ModbusMasterDB.xml", $"{StartTIAPortalModel.SelectedPort.Replace(" ", "_").Replace("/", "_")}Group");
                        _newTIAPortal.CreateInstanceDB();
                        File.Delete($"{Environment.CurrentDirectory}/ModbusMaster.xml");
                        File.Delete($"{Environment.CurrentDirectory}/ModbusMasterDB.xml");
                    });
                    UpdateLoading(false);

                }
                catch (Exception ex)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        UpdateLoading(false);
                        var dialogResult = dialogHostService.Question("致命错误", ex.Message);
                    });
                    Log.Fatal($"生成Modbus程序时发生错误：{ex.Message}。");

                }
            }
        }

        

        private async void OpenTIAPortal()
        {
            Log.Information("打开TIA Portal中。");
            try
            {
                UpdateLoading(true);
                await Task.Run(async () =>
                {
                    instTIA = _newTIAPortal.CreateTIAinstance(true);
                });
                UpdateLoading(false);
                Log.Information("TIA Portal已被打开。");
            }
            catch (Exception ex)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    UpdateLoading(false);
                    var dialogResult = dialogHostService.Question("致命错误", ex.Message);
                });
                Log.Fatal($"打开TIA Portal时发生错误：{ex.Message}。");
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

        string originalXml = @"<SW.Blocks.CompileUnit ID=""12"" CompositionName=""CompileUnits"">
<AttributeList>
<NetworkSource>
<FlgNet xmlns=""http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4"">
<Parts>
<Access Scope=""GlobalVariable"" UId=""21"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Step""/>
</Symbol>
</Access>
<Access Scope=""LiteralConstant"" UId=""22"">
<Constant>
<ConstantType>USInt</ConstantType>
<ConstantValue>1</ConstantValue>
</Constant>
</Access>
<Access Scope=""LiteralConstant"" UId=""23"">
<Constant>
<ConstantType>UInt</ConstantType>
<ConstantValue>1</ConstantValue>
</Constant>
</Access>
<Access Scope=""LiteralConstant"" UId=""24"">
<Constant>
<ConstantType>USInt</ConstantType>
<ConstantValue>0</ConstantValue>
</Constant>
</Access>
<Access Scope=""LiteralConstant"" UId=""25"">
<Constant>
<ConstantType>UDInt</ConstantType>
<ConstantValue>5247</ConstantValue>
</Constant>
</Access>
<Access Scope=""LiteralConstant"" UId=""26"">
<Constant>
<ConstantType>UInt</ConstantType>
<ConstantValue>2</ConstantValue>
</Constant>
</Access>
<Access Scope=""GlobalVariable"" UId=""27"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Read_DATA_tgreg1""/>
</Symbol>
</Access>
<Access Scope=""GlobalVariable"" UId=""28"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Modbus_Master"" AccessModifier=""Array"">
<Access Scope=""LiteralConstant"">
<Constant>
<ConstantType>DInt</ConstantType>
<ConstantValue>1</ConstantValue>
</Constant>
</Access>
</Component>
<Component Name=""DONE""/>
</Symbol>
</Access>
<Access Scope=""GlobalVariable"" UId=""29"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Modbus_Master"" AccessModifier=""Array"">
<Access Scope=""LiteralConstant"">
<Constant>
<ConstantType>DInt</ConstantType>
<ConstantValue>1</ConstantValue>
</Constant>
</Access>
</Component>
<Component Name=""BUSY""/>
</Symbol>
</Access>
<Access Scope=""GlobalVariable"" UId=""30"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Modbus_Master"" AccessModifier=""Array"">
<Access Scope=""LiteralConstant"">
<Constant>
<ConstantType>DInt</ConstantType>
<ConstantValue>1</ConstantValue>
</Constant>
</Access>
</Component>
<Component Name=""ERROR""/>
</Symbol>
</Access>
<Access Scope=""GlobalVariable"" UId=""31"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Modbus_Master"" AccessModifier=""Array"">
<Access Scope=""LiteralConstant"">
<Constant>
<ConstantType>DInt</ConstantType>
<ConstantValue>1</ConstantValue>
</Constant>
</Access>
</Component>
<Component Name=""STATUS""/>
</Symbol>
</Access>
<Part Name=""Eq"" UId=""32"">
<TemplateValue Name=""SrcType"" Type=""Type"">USInt</TemplateValue>
</Part>
<Part Name=""Modbus_Master"" Version=""5.1"" UId=""33"">
<Instance Scope=""GlobalVariable"" UId=""34"">
<Component Name=""Modbus_Master_DB""/>
</Instance>
</Part>
</Parts>
<Wires>
<Wire UId=""35"">
<Powerrail/>
<NameCon UId=""33"" Name=""en""/>
<NameCon UId=""32"" Name=""pre""/>
</Wire>
<Wire UId=""36"">
<IdentCon UId=""21""/>
<NameCon UId=""32"" Name=""in1""/>
</Wire>
<Wire UId=""37"">
<IdentCon UId=""22""/>
<NameCon UId=""32"" Name=""in2""/>
</Wire>
<Wire UId=""38"">
<NameCon UId=""32"" Name=""out""/>
<NameCon UId=""33"" Name=""REQ""/>
</Wire>
<Wire UId=""39"">
<IdentCon UId=""23""/>
<NameCon UId=""33"" Name=""MB_ADDR""/>
</Wire>
<Wire UId=""40"">
<IdentCon UId=""24""/>
<NameCon UId=""33"" Name=""MODE""/>
</Wire>
<Wire UId=""41"">
<IdentCon UId=""25""/>
<NameCon UId=""33"" Name=""DATA_ADDR""/>
</Wire>
<Wire UId=""42"">
<IdentCon UId=""26""/>
<NameCon UId=""33"" Name=""DATA_LEN""/>
</Wire>
<Wire UId=""43"">
<IdentCon UId=""27""/>
<NameCon UId=""33"" Name=""DATA_PTR""/>
</Wire>
<Wire UId=""44"">
<NameCon UId=""33"" Name=""DONE""/>
<IdentCon UId=""28""/>
</Wire>
<Wire UId=""45"">
<NameCon UId=""33"" Name=""BUSY""/>
<IdentCon UId=""29""/>
</Wire>
<Wire UId=""46"">
<NameCon UId=""33"" Name=""ERROR""/>
<IdentCon UId=""30""/>
</Wire>
<Wire UId=""47"">
<NameCon UId=""33"" Name=""STATUS""/>
<IdentCon UId=""31""/>
</Wire>
</Wires>
</FlgNet>
</NetworkSource>
<ProgrammingLanguage>LAD</ProgrammingLanguage>
</AttributeList>
<ObjectList>
<MultilingualText ID=""13"" CompositionName=""Comment"">
<ObjectList>
<MultilingualTextItem ID=""14"" CompositionName=""Items"">
<AttributeList>
<Culture>zh-CN</Culture>
<Text>Modbus主站读取保持寄存器数据</Text>
</AttributeList>
</MultilingualTextItem>
</ObjectList>
</MultilingualText>
<MultilingualText ID=""15"" CompositionName=""Title"">
<ObjectList>
<MultilingualTextItem ID=""16"" CompositionName=""Items"">
<AttributeList>
<Culture>zh-CN</Culture>
<Text>第一步：""ModbusMasterOpenness.Step""值=1时触发“Modbus_Master”指令读取 Modbus RTU 从站地址1保持寄存器5247地址开始的2个字长的数据，将其存放于“""ModbusMasterOpenness"".tgreg1”指定的地址中</Text>
</AttributeList>
</MultilingualTextItem>
</ObjectList>
</MultilingualText>
</ObjectList>
</SW.Blocks.CompileUnit>";
        string originalXml1 = @"<SW.Blocks.CompileUnit ID=""17"" CompositionName=""CompileUnits"">
<AttributeList>
<NetworkSource>
<FlgNet xmlns=""http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4"">
<Parts>
<Access Scope=""GlobalVariable"" UId=""21"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Step""/>
</Symbol>
</Access>
<Access Scope=""LiteralConstant"" UId=""22"">
<Constant>
<ConstantType>USInt</ConstantType>
<ConstantValue>1</ConstantValue>
</Constant>
</Access>
<Access Scope=""GlobalVariable"" UId=""23"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Modbus_Master"" AccessModifier=""Array"">
<Access Scope=""LiteralConstant"">
<Constant>
<ConstantType>DInt</ConstantType>
<ConstantValue>1</ConstantValue>
</Constant>
</Access>
</Component>
<Component Name=""DONE""/>
</Symbol>
</Access>
<Access Scope=""GlobalVariable"" UId=""24"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Modbus_Master"" AccessModifier=""Array"">
<Access Scope=""LiteralConstant"">
<Constant>
<ConstantType>DInt</ConstantType>
<ConstantValue>1</ConstantValue>
</Constant>
</Access>
</Component>
<Component Name=""ERROR""/>
</Symbol>
</Access>
<Access Scope=""LiteralConstant"" UId=""25"">
<Constant>
<ConstantType>USInt</ConstantType>
<ConstantValue>2</ConstantValue>
</Constant>
</Access>
<Access Scope=""GlobalVariable"" UId=""26"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Step""/>
</Symbol>
</Access>
<Part Name=""Eq"" UId=""27"">
<TemplateValue Name=""SrcType"" Type=""Type"">USInt</TemplateValue>
</Part>
<Part Name=""Contact"" UId=""28""/>
<Part Name=""Contact"" UId=""29""/>
<Part Name=""O"" UId=""30"">
<TemplateValue Name=""Card"" Type=""Cardinality"">2</TemplateValue>
</Part>
<Part Name=""Move"" UId=""31"" DisabledENO=""true"">
<TemplateValue Name=""Card"" Type=""Cardinality"">1</TemplateValue>
</Part>
</Parts>
<Wires>
<Wire UId=""32"">
<Powerrail/>
<NameCon UId=""27"" Name=""pre""/>
</Wire>
<Wire UId=""33"">
<IdentCon UId=""21""/>
<NameCon UId=""27"" Name=""in1""/>
</Wire>
<Wire UId=""34"">
<IdentCon UId=""22""/>
<NameCon UId=""27"" Name=""in2""/>
</Wire>
<Wire UId=""35"">
<NameCon UId=""27"" Name=""out""/>
<NameCon UId=""28"" Name=""in""/>
<NameCon UId=""29"" Name=""in""/>
</Wire>
<Wire UId=""36"">
<IdentCon UId=""23""/>
<NameCon UId=""28"" Name=""operand""/>
</Wire>
<Wire UId=""37"">
<NameCon UId=""28"" Name=""out""/>
<NameCon UId=""30"" Name=""in1""/>
</Wire>
<Wire UId=""38"">
<IdentCon UId=""24""/>
<NameCon UId=""29"" Name=""operand""/>
</Wire>
<Wire UId=""39"">
<NameCon UId=""29"" Name=""out""/>
<NameCon UId=""30"" Name=""in2""/>
</Wire>
<Wire UId=""40"">
<NameCon UId=""30"" Name=""out""/>
<NameCon UId=""31"" Name=""en""/>
</Wire>
<Wire UId=""41"">
<IdentCon UId=""25""/>
<NameCon UId=""31"" Name=""in""/>
</Wire>
<Wire UId=""42"">
<NameCon UId=""31"" Name=""out1""/>
<IdentCon UId=""26""/>
</Wire>
</Wires>
</FlgNet>
</NetworkSource>
<ProgrammingLanguage>LAD</ProgrammingLanguage>
</AttributeList>
<ObjectList>
<MultilingualText ID=""18"" CompositionName=""Comment"">
<ObjectList>
<MultilingualTextItem ID=""19"" CompositionName=""Items"">
<AttributeList>
<Culture>zh-CN</Culture>
<Text>转到第2步</Text>
</AttributeList>
</MultilingualTextItem>
</ObjectList>
</MultilingualText>
<MultilingualText ID=""1A"" CompositionName=""Title"">
<ObjectList>
<MultilingualTextItem ID=""1B"" CompositionName=""Items"">
<AttributeList>
<Culture>zh-CN</Culture>
<Text>第1步完成位或错误位作为条件转到第2步，使能MOVE指令，对步地址""ModbusMasterOpenness.Step""赋值2</Text>
</AttributeList>
</MultilingualTextItem>
</ObjectList>
</MultilingualText>
</ObjectList>
</SW.Blocks.CompileUnit>";
        string originalXml2 = @"<SW.Blocks.CompileUnit ID=""8"" CompositionName=""CompileUnits"">
<AttributeList>
<NetworkSource>
<FlgNet xmlns=""http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4"">
<Parts>
<Access Scope=""GlobalVariable"" UId=""21"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Comm_Load_DONE""/>
</Symbol>
</Access>
<Access Scope=""GlobalConstant"" UId=""22"">
<Constant Name=""Local~CM_1241_(RS422_485)_1""/>
</Access>
<Access Scope=""LiteralConstant"" UId=""23"">
<Constant>
<ConstantType>UDInt</ConstantType>
<ConstantValue>9600</ConstantValue>
</Constant>
</Access>
<Access Scope=""LiteralConstant"" UId=""24"">
<Constant>
<ConstantType>UInt</ConstantType>
<ConstantValue>0</ConstantValue>
</Constant>
</Access>
<Access Scope=""LiteralConstant"" UId=""25"">
<Constant>
<ConstantType>UInt</ConstantType>
<ConstantValue>0</ConstantValue>
</Constant>
</Access>
<Access Scope=""LiteralConstant"" UId=""26"">
<Constant>
<ConstantType>UInt</ConstantType>
<ConstantValue>0</ConstantValue>
</Constant>
</Access>
<Access Scope=""LiteralConstant"" UId=""27"">
<Constant>
<ConstantType>UInt</ConstantType>
<ConstantValue>0</ConstantValue>
</Constant>
</Access>
<Access Scope=""LiteralConstant"" UId=""28"">
<Constant>
<ConstantType>UInt</ConstantType>
<ConstantValue>1000</ConstantValue>
</Constant>
</Access>
<Access Scope=""GlobalVariable"" UId=""29"">
<Symbol>
<Component Name=""Modbus_Master_DB""/>
<Component Name=""MB_DB""/>
</Symbol>
</Access>
<Access Scope=""GlobalVariable"" UId=""30"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Comm_Load_DONE""/>
</Symbol>
</Access>
<Access Scope=""GlobalVariable"" UId=""31"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Comm_Load_ERROR""/>
</Symbol>
</Access>
<Access Scope=""GlobalVariable"" UId=""32"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Comm_Load_STATUS""/>
</Symbol>
</Access>
<Part Name=""Contact"" UId=""33"">
<Negated Name=""operand""/>
</Part>
<Part Name=""Modbus_Comm_Load"" Version=""5.0"" UId=""34"">
<Instance Scope=""GlobalVariable"" UId=""35"">
<Component Name=""Modbus_Comm_Load_DB""/>
</Instance>
</Part>
</Parts>
<Wires>
<Wire UId=""36"">
<Powerrail/>
<NameCon UId=""34"" Name=""en""/>
<NameCon UId=""33"" Name=""in""/>
</Wire>
<Wire UId=""37"">
<IdentCon UId=""21""/>
<NameCon UId=""33"" Name=""operand""/>
</Wire>
<Wire UId=""38"">
<NameCon UId=""33"" Name=""out""/>
<NameCon UId=""34"" Name=""REQ""/>
</Wire>
<Wire UId=""39"">
<IdentCon UId=""22""/>
<NameCon UId=""34"" Name=""PORT""/>
</Wire>
<Wire UId=""40"">
<IdentCon UId=""23""/>
<NameCon UId=""34"" Name=""BAUD""/>
</Wire>
<Wire UId=""41"">
<IdentCon UId=""24""/>
<NameCon UId=""34"" Name=""PARITY""/>
</Wire>
<Wire UId=""42"">
<IdentCon UId=""25""/>
<NameCon UId=""34"" Name=""FLOW_CTRL""/>
</Wire>
<Wire UId=""43"">
<IdentCon UId=""26""/>
<NameCon UId=""34"" Name=""RTS_ON_DLY""/>
</Wire>
<Wire UId=""44"">
<IdentCon UId=""27""/>
<NameCon UId=""34"" Name=""RTS_OFF_DLY""/>
</Wire>
<Wire UId=""45"">
<IdentCon UId=""28""/>
<NameCon UId=""34"" Name=""RESP_TO""/>
</Wire>
<Wire UId=""46"">
<IdentCon UId=""29""/>
<NameCon UId=""34"" Name=""MB_DB""/>
</Wire>
<Wire UId=""47"">
<NameCon UId=""34"" Name=""DONE""/>
<IdentCon UId=""30""/>
</Wire>
<Wire UId=""48"">
<NameCon UId=""34"" Name=""ERROR""/>
<IdentCon UId=""31""/>
</Wire>
<Wire UId=""49"">
<NameCon UId=""34"" Name=""STATUS""/>
<IdentCon UId=""32""/>
</Wire>
</Wires>
</FlgNet>
</NetworkSource>
<ProgrammingLanguage>LAD</ProgrammingLanguage>
</AttributeList>
<ObjectList>
<MultilingualText ID=""9"" CompositionName=""Comment"">
<ObjectList>
<MultilingualTextItem ID=""A"" CompositionName=""Items"">
<AttributeList>
<Culture>zh-CN</Culture>
<Text>Modbus主站初始化</Text>
</AttributeList>
</MultilingualTextItem>
</ObjectList>
</MultilingualText>
<MultilingualText ID=""B"" CompositionName=""Title"">
<ObjectList>
<MultilingualTextItem ID=""C"" CompositionName=""Items"">
<AttributeList>
<Culture>zh-CN</Culture>
<Text>在S7-1200启动的第一个扫描周期，将Modbus RTU通信的RS485端口参数初始化为波特率：9600，无校验，无流控，响应超时1000ms（Modbus RTU默认为数据位：8位，停止位：1位） MB_DB指向""Modbus_Master""指令所使用的背景数据块引用</Text>
</AttributeList>
</MultilingualTextItem>
</ObjectList>
</MultilingualText>
</ObjectList>
</SW.Blocks.CompileUnit>";
        string originalXml3 = @"<SW.Blocks.CompileUnit ID=""D"" CompositionName=""CompileUnits"">
<AttributeList>
<NetworkSource>
<FlgNet xmlns=""http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4"">
<Parts>
<Access Scope=""GlobalVariable"" UId=""21"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Comm_Load_DONE""/>
</Symbol>
</Access>
<Access Scope=""LiteralConstant"" UId=""22"">
<Constant>
<ConstantType>USInt</ConstantType>
<ConstantValue>1</ConstantValue>
</Constant>
</Access>
<Access Scope=""GlobalVariable"" UId=""23"">
<Symbol>
<Component Name=""CM_1241_(RS422/485)_1ModbusMasterDB""/>
<Component Name=""Step""/>
</Symbol>
</Access>
<Part Name=""Contact"" UId=""24""/>
<Part Name=""Move"" UId=""25"" DisabledENO=""true"">
<TemplateValue Name=""Card"" Type=""Cardinality"">1</TemplateValue>
</Part>
</Parts>
<Wires>
<Wire UId=""26"">
<Powerrail/>
<NameCon UId=""24"" Name=""in""/>
</Wire>
<Wire UId=""27"">
<IdentCon UId=""21""/>
<NameCon UId=""24"" Name=""operand""/>
</Wire>
<Wire UId=""28"">
<NameCon UId=""24"" Name=""out""/>
<NameCon UId=""25"" Name=""en""/>
</Wire>
<Wire UId=""29"">
<IdentCon UId=""22""/>
<NameCon UId=""25"" Name=""in""/>
</Wire>
<Wire UId=""30"">
<NameCon UId=""25"" Name=""out1""/>
<IdentCon UId=""23""/>
</Wire>
</Wires>
</FlgNet>
</NetworkSource>
<ProgrammingLanguage>LAD</ProgrammingLanguage>
</AttributeList>
<ObjectList>
<MultilingualText ID=""E"" CompositionName=""Comment"">
<ObjectList>
<MultilingualTextItem ID=""F"" CompositionName=""Items"">
<AttributeList>
<Culture>zh-CN</Culture>
<Text>转到第一步</Text>
</AttributeList>
</MultilingualTextItem>
</ObjectList>
</MultilingualText>
<MultilingualText ID=""10"" CompositionName=""Title"">
<ObjectList>
<MultilingualTextItem ID=""11"" CompositionName=""Items"">
<AttributeList>
<Culture>zh-CN</Culture>
<Text>初始化完成位使能MOVE指令，对步地址""ModbusMasterOpenness.Step""赋值1</Text>
</AttributeList>
</MultilingualTextItem>
</ObjectList>
</MultilingualText>
</ObjectList>
</SW.Blocks.CompileUnit>";
        string originalXml4 = @"
<SW.Blocks.OB ID=""0"">
<AttributeList>
<AutoNumber>true</AutoNumber>
<HeaderAuthor/>
<HeaderFamily/>
<HeaderName/>
<HeaderVersion>0.1</HeaderVersion>
<Interface>
<Sections xmlns=""http://www.siemens.com/automation/Openness/SW/Interface/v5"">
<Section Name=""Input"">
<Member Name=""Initial_Call"" Datatype=""Bool"" Accessibility=""Public"" Informative=""true"">
<Comment>
<MultiLanguageText Lang=""en-US"">Initial call of this OB</MultiLanguageText>
</Comment>
</Member>
<Member Name=""Remanence"" Datatype=""Bool"" Accessibility=""Public"" Informative=""true"">
<Comment>
<MultiLanguageText Lang=""en-US"">=True, if remanent data are available</MultiLanguageText>
</Comment>
</Member>
</Section>
<Section Name=""Temp""/>
<Section Name=""Constant""/>
</Sections>
</Interface>
<IsIECCheckEnabled>false</IsIECCheckEnabled>
<MemoryLayout>Optimized</MemoryLayout>
<Name>CM_1241_(RS422_485)_1ModbusMaster</Name>";
        string originalXml5 = @"<Number>124</Number>
<ProgrammingLanguage>LAD</ProgrammingLanguage>
<SecondaryType>ProgramCycle</SecondaryType>
<SetENOAutomatically>false</SetENOAutomatically>
</AttributeList>
<ObjectList>
<MultilingualText CompositionName=""Comment"" ID=""1"">
<ObjectList>
<MultilingualTextItem ID=""2"" CompositionName=""Items"">
<AttributeList>
<Culture>zh-CN</Culture>
<Text>1.免责声明 1.软件使用 本软件是为了提供便利和效率而设计的。开发者已尽力确保软件的功能性和稳定性，但无法保证软件在所有情况下都能无误运行。用户应自行承担使用软件的风险。 2.免责声明 开发者不对因使用或无法使用本软件而产生的任何直接、间接、偶然、特殊或惩罚性的损害承担责任，包括但不限于数据损失、设备损坏、业务中断、利润损失等。 3.软件限制 本软件可能存在已知或未知的缺陷和漏洞。开发者保留对软件进行修改、更新或停止支持的权利，且不保证提供任何形式的技术支持或更新服务。</Text>
</AttributeList>
</MultilingualTextItem>
</ObjectList>
</MultilingualText>
<SW.Blocks.CompileUnit ID=""3"" CompositionName=""CompileUnits"">
<AttributeList>
<NetworkSource>
<FlgNet xmlns=""http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4"">
<Parts>
<Access Scope=""GlobalVariable"" UId=""21"">
<Symbol>
<Component Name=""FirstScan""/>
</Symbol>
</Access>
<Access Scope=""LiteralConstant"" UId=""22"">
<Constant>
<ConstantType>USInt</ConstantType>
<ConstantValue>4</ConstantValue>
</Constant>
</Access>
<Access Scope=""GlobalVariable"" UId=""23"">
<Symbol>
<Component Name=""Modbus_Comm_Load_DB""/>
<Component Name=""MODE""/>
</Symbol>
</Access>
<Part Name=""Contact"" UId=""24""/>
<Part Name=""Move"" UId=""25"" DisabledENO=""true"">
<TemplateValue Name=""Card"" Type=""Cardinality"">1</TemplateValue>
</Part>
</Parts>
<Wires>
<Wire UId=""26"">
<Powerrail/>
<NameCon UId=""24"" Name=""in""/>
</Wire>
<Wire UId=""27"">
<IdentCon UId=""21""/>
<NameCon UId=""24"" Name=""operand""/>
</Wire>
<Wire UId=""28"">
<NameCon UId=""24"" Name=""out""/>
<NameCon UId=""25"" Name=""en""/>
</Wire>
<Wire UId=""29"">
<IdentCon UId=""22""/>
<NameCon UId=""25"" Name=""in""/>
</Wire>
<Wire UId=""30"">
<NameCon UId=""25"" Name=""out1""/>
<IdentCon UId=""23""/>
</Wire>
</Wires>
</FlgNet>
</NetworkSource>
<ProgrammingLanguage>LAD</ProgrammingLanguage>
</AttributeList>
<ObjectList>
<MultilingualText ID=""4"" CompositionName=""Comment"">
<ObjectList>
<MultilingualTextItem ID=""5"" CompositionName=""Items"">
<AttributeList>
<Culture>zh-CN</Culture>
<Text>设置通信端口模式=4 Modbus通信</Text>
</AttributeList>
</MultilingualTextItem>
</ObjectList>
</MultilingualText>
<MultilingualText ID=""6"" CompositionName=""Title"">
<ObjectList>
<MultilingualTextItem ID=""7"" CompositionName=""Items"">
<AttributeList>
<Culture>zh-CN</Culture>
<Text>在S7-1200启动的第一个扫描周期，将数值4传送到在“Modbus_Comm_Load.DB”MODE，将工作模式设置为半双工 RS485两线模式</Text>
</AttributeList>
</MultilingualTextItem>
</ObjectList>
</MultilingualText>
</ObjectList>
</SW.Blocks.CompileUnit>";
        string originalXml6 = @"
<SW.Blocks.GlobalDB ID=""0"">
<AttributeList>
<AutoNumber>true</AutoNumber>
<HeaderAuthor/>
<HeaderFamily/>
<HeaderName/>
<HeaderVersion>0.1</HeaderVersion>
<Interface>
<Sections xmlns=""http://www.siemens.com/automation/Openness/SW/Interface/v5"">
<Section Name=""Static"">
<Member Name=""Step"" Datatype=""USInt"" Remanence=""NonRetain"" Accessibility=""Public"">
<AttributeList>
<BooleanAttribute Name=""ExternalAccessible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalVisible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalWritable"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""SetPoint"" SystemDefined=""true"">false</BooleanAttribute>
</AttributeList>
</Member>
<Member Name=""Comm_Load_DONE"" Datatype=""Bool"" Remanence=""NonRetain"" Accessibility=""Public"">
<AttributeList>
<BooleanAttribute Name=""ExternalAccessible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalVisible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalWritable"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""SetPoint"" SystemDefined=""true"">false</BooleanAttribute>
</AttributeList>
</Member>
<Member Name=""Comm_Load_ERROR"" Datatype=""Bool"" Remanence=""NonRetain"" Accessibility=""Public"">
<AttributeList>
<BooleanAttribute Name=""ExternalAccessible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalVisible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalWritable"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""SetPoint"" SystemDefined=""true"">false</BooleanAttribute>
</AttributeList>
</Member>
<Member Name=""Comm_Load_STATUS"" Datatype=""Word"" Remanence=""NonRetain"" Accessibility=""Public"">
<AttributeList>
<BooleanAttribute Name=""ExternalAccessible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalVisible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalWritable"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""SetPoint"" SystemDefined=""true"">false</BooleanAttribute>
</AttributeList>
</Member>";
        string originalXml7 = @"<AttributeList>
<BooleanAttribute Name=""ExternalAccessible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalVisible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalWritable"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""SetPoint"" SystemDefined=""true"">false</BooleanAttribute>
</AttributeList>
<Member Name=""DONE"" Datatype=""Bool"">
<AttributeList>
<BooleanAttribute Name=""ExternalAccessible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalVisible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalWritable"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""SetPoint"" SystemDefined=""true"">false</BooleanAttribute>
</AttributeList>
</Member>
<Member Name=""BUSY"" Datatype=""Bool"">
<AttributeList>
<BooleanAttribute Name=""ExternalAccessible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalVisible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalWritable"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""SetPoint"" SystemDefined=""true"">false</BooleanAttribute>
</AttributeList>
</Member>
<Member Name=""ERROR"" Datatype=""Bool"">
<AttributeList>
<BooleanAttribute Name=""ExternalAccessible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalVisible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalWritable"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""SetPoint"" SystemDefined=""true"">false</BooleanAttribute>
</AttributeList>
</Member>
<Member Name=""STATUS"" Datatype=""Word"">
<AttributeList>
<BooleanAttribute Name=""ExternalAccessible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalVisible"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""ExternalWritable"" SystemDefined=""true"">true</BooleanAttribute>
<BooleanAttribute Name=""SetPoint"" SystemDefined=""true"">false</BooleanAttribute>
</AttributeList>
</Member>
</Member>";
    } }
