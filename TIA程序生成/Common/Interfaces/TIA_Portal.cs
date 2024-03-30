using System;
using System.Collections.Generic;
using System.IO;
using Siemens.Engineering;
using Siemens.Engineering.Cax;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.Compare;
using Siemens.Engineering.Download;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Cycle;
using Siemens.Engineering.Hmi.Communication;
using Siemens.Engineering.Hmi.Globalization;
using Siemens.Engineering.Hmi.RuntimeScripting;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.Hmi.TextGraphicList;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Extensions;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.HW.Utilities;
using Siemens.Engineering.Library;
using Siemens.Engineering.Library.MasterCopies;
using Siemens.Engineering.Library.Types;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.ExternalSources;
using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.SW.TechnologicalObjects;
using Siemens.Engineering.SW.TechnologicalObjects.Motion;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.Upload;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Globalization;
using System.Windows;
using Prism.Events;
using TIA程序生成.Extensions;
using TIA程序生成.Common;
using Prism.Services.Dialogs;
using TIA程序生成.Common.Models;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using ImTools;
using System.Collections;
using Siemens.Engineering.Umac;
using System.Windows.Documents;
using System.Xml.Linq;
using System.Security.Principal;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using DryIoc;
using System.Collections.ObjectModel;
using TIA程序生成.Properties;
using Serilog;

namespace TIA程序生成.Common.Interfaces
{
    public partial class TIA_Portal
    {
        private readonly IDialogHostService _dialogHostService;
        public static TiaPortal instTIA;
        public static Project projectTIA;
        public static Device plcDevice;
        public static PlcSoftware plcSoftware;
        public static PlcBlock dataBlock;

        public TIA_Portal(IDialogHostService dialogHostService)
        {
            this._dialogHostService = dialogHostService;
        }

        /// <summary>
        /// 打开一个新的带或者不带用户界面的TIA Portal实例。
        /// </summary>
        /// <param name="guiTIA"></param>
        public TiaPortal CreateTIAinstance(bool guiTIA)
        {
            try
            {
                //打开一个新的带用户界面的TIA Portal实例
                if (guiTIA)
                {
                    Log.Information("选择的是带用户页面的TIA Portal。");
                    instTIA = new TiaPortal(TiaPortalMode.WithUserInterface);
                }
                //打开一个新的不带用户界面的TIA Portal实例
                else
                {
                    Log.Information("选择的是不带用户页面的TIA Portal。");
                    instTIA = new TiaPortal(TiaPortalMode.WithoutUserInterface);
                }
            }
            catch (Exception ex)
            {
                var dialogResult = _dialogHostService.Question("致命错误", ex.Message);
            }
            return instTIA;
        }

        /// <summary>
        /// 在给定的项目路径下，使用给定的项目名称创建新的TIA项目。
        /// </summary>
        /// <param name="prjPath"></param>
        /// <param name="prjName"></param>
        /// <param name="prjAuthor"></param>
        /// <param name="prjComment"></param>
        public string CreateTIAprj(string prjPath, string prjName, string prjAuthor, string prjComment)
        {
            string error=string.Empty;
            try
            {
                prjAuthor = prjAuthor == null ? "" : prjAuthor;
                prjComment = prjComment == null ? "" : prjComment;
                // 创建新的目录信息
                DirectoryInfo targetDirectory = new DirectoryInfo(prjPath);
                //创建新的TIA Port 项目
                ProjectComposition projectComposition = instTIA.Projects;
                IEnumerable<KeyValuePair<string, object>> createParameters = new[] {
                new KeyValuePair<string, object>("TargetDirectory", targetDirectory),//必填
                new KeyValuePair<string, object>("Name", prjName),//必填
                new KeyValuePair<string, object>("Author", prjAuthor),//可选
                new KeyValuePair<string, object>("Comment", prjComment)}; //可选
                                                                          //创建一个包含必填和可选参数的项目
                projectTIA = (Project)((IEngineeringComposition)projectComposition).Create(typeof(Project), createParameters);
            }
            catch(Exception ex)
            {
                error = ex.Message;
            }
           return error;
        }

        /// <summary>
        /// 按照选择的TIA Portal文件打开TIA Portal项目
        /// </summary>
        /// <param name="fileName"></param>
        public Project OpenFile(string fileName)
        {
            projectTIA = instTIA.Projects.Open(new FileInfo(fileName));
            return projectTIA;
        }

        /// <summary>
        /// 在TIA Portal的项目里插入PLC
        /// </summary>
        /// <param name="plcArticle"></param>
        /// <param name="plcVersion"></param>
        /// <param name="plcName"></param>
        public void CreatePLC(string plcArticle, string plcVersion, string plcName)
        {
            string plcIdent = $"OrderNumber:{plcArticle}/{plcVersion}";
            string plcStation = $"Station{plcName}";
            if (projectTIA == null)
            {
                var dialogResult = _dialogHostService.Question("温馨提示", "请打开项目。");
                return;
            }
            else
            {
                plcDevice = projectTIA.Devices.CreateWithItem(plcIdent, plcName, plcStation);
            }
        }

        /// <summary>
        /// 连接TIA Portal的项目
        /// </summary>
        /// <param name="prjName"></param>
        public Project ConnectTIAprj(string prjName)
        {
            try
            {

                foreach (TiaPortalProcess tiaPortalProcess in TiaPortal.GetProcesses())
                {
                    // 使用 LINQ 查询判断字符串是否存在于 tiaPortalProcess.ProjectPath.Name 中
                    bool exists = tiaPortalProcess.ProjectPath?.Name.Contains(prjName) ?? false;

                    if (exists)
                    {
                        instTIA = tiaPortalProcess.Attach();
                        projectTIA = instTIA.Projects[0];
                        if (projectTIA.Devices.Count > 0)
                        {
                            plcDevice = projectTIA.Devices[0];
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message);
                MessageBox.Show(ex.Message);
                //var dialogResult = _dialogHostService.Question("致命错误", ex.Message);
            }
            return projectTIA;
        }

        /// <summary>
        /// 获取TIA Portal的项目中的驱动名
        /// </summary>
        /// <returns></returns>
        public List<string> GetTiaPortalDevices()
        {
            //List<string> list = projectTIA.Devices
            //     .SelectMany(item => item.Items[0].Items)
            //     .Where(rack => !string.IsNullOrEmpty(rack.Name))
            //     .Select(rack => rack.Items[0].Name)
            //     .ToList();
            List<string> list = instTIA.Projects
                   .SelectMany(project => project.Devices)
                   .Select(item => item.Items[0])
                   .Select(rack => rack.Items[0].Name)
                   .ToList();
            return list;
        }

        /// <summary>
        /// 根据名称连接PLC实例并返回PLC中的程序块
        /// </summary>
        /// <param name="plcName"></param>
        /// <returns></returns>
        public List<string> ConnectTiaPortalDevices(string plcName)
        {
            List<string> list = new List<string>();
            if (!string.IsNullOrWhiteSpace(plcName))
            {
                if (projectTIA != null)
                {
                    foreach (var item in projectTIA.Devices)
                    {
                        DeviceItem rack = item.Items[0];
                        if (rack.Items[0].Name == plcName)
                        {
                            SoftwareContainer softwareContainer =
                            ((IEngineeringServiceProvider)rack.Items[0]).GetService<SoftwareContainer>();
                            if (softwareContainer != null)
                            {
                                plcSoftware = softwareContainer.Software as PlcSoftware;
                            }
                        }

                    }
                    // 检查 plcSoftware 是否为 null
                    if (plcSoftware == null || plcSoftware.BlockGroup == null || plcSoftware.BlockGroup.Blocks == null)
                    {
                        return new List<string>(); // 返回空列表
                    }

                    // 使用 LINQ 查询来构建结果列表
                    list = plcSoftware.BlockGroup.Blocks
                        .Select(block => block != null ? $"{block.Name} [{block.ToString().Substring(block.ToString().Length - 2, 2)}{block.Number}]" : null)
                        .Where(name => name != null) // 过滤掉 null 值
                        .ToList();
                }
                else
                {
                    var dialogResult = _dialogHostService.Question("温馨提示", "请打开项目。");
                }
            }
            else
            {
                var dialogResult = _dialogHostService.Question("温馨提示", "请输入驱动名。");
            }


            return list;
        }

        /// <summary>
        /// 返回PLC中的程序块
        /// </summary>
        /// <returns></returns>
        public List<string> GetBlockName()
        {
            // 检查 plcSoftware 是否为 null
            if (plcSoftware == null || plcSoftware.BlockGroup == null || plcSoftware.BlockGroup.Blocks == null)
            {
                return new List<string>(); // 返回空列表
            }

            // 使用 LINQ 查询来构建结果列表
            var blockNames = plcSoftware.BlockGroup.Blocks
                .Select(block => block != null ? $"{block.Name} [{block.ToString().Substring(block.ToString().Length - 2, 2)}{block.Number}]" : null)
                .Where(name => name != null) // 过滤掉 null 值
                .ToList();

            return blockNames;
        }



        /// <summary>
        /// 将块导出至Xml文件
        /// </summary>
        /// <param name="blockExportPath"></param>
        /// <param name="blockName"></param>
        public string ExportRegularBlock(string blockExportPath, string blockName)
        {
            string error=string.Empty;
            int spaceIndex = blockName.IndexOf(' ');
            string blockNameTemp = blockName.Substring(0, spaceIndex);
            try
            {
                dataBlock = plcSoftware.BlockGroup.Blocks.Find(blockNameTemp);
                if (dataBlock != null)
                {
                    dataBlock.Export(new FileInfo($"{blockExportPath}\\{blockNameTemp}.xml"),
                                ExportOptions.WithDefaults);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"导出块'{blockName}'时发生错误：{ex.Message}");
                error = ex.Message;
            }
            return error;
        }

        /// <summary>
        /// 导入块
        /// </summary>
        /// <param name="blockImportPath"></param>
        public bool ImportBlocks(string blockImportPath)
        {
            bool error = false;
            if (plcSoftware != null)
            {
                PlcBlockGroup blockGroup = plcSoftware.BlockGroup;
                IList<PlcBlock> blocks = blockGroup.Blocks.Import(new FileInfo(blockImportPath),
                ImportOptions.Override);
            }
            else
            {
                error = true;
                var dialogResult = _dialogHostService.Question("温馨提示", "请连接PLC。");
            }
            return error;
        }

        private static void AccessBlockusergroup(PlcSoftware plcsoftware)
        {
            PlcBlockUserGroupComposition userGroupComposition = plcsoftware.BlockGroup.Groups;
            PlcBlockUserGroup plcBlockUserGroup = userGroupComposition.Find("MyUserfolder");
        }

        /// <summary>
        /// 导入用户组的块
        /// </summary>
        /// <param name="blockImportPath"></param>
        public bool ImportBlockGroups(string blockImportPath, string GroupName)
        {
            bool error = false;
            if (plcSoftware != null)
            {
                PlcBlockUserGroupComposition userGroupComposition = plcSoftware.BlockGroup.Groups;
                PlcBlockUserGroup plcBlockUserGroup = userGroupComposition.Find(GroupName);
                if (plcBlockUserGroup == null)
                {
                    PlcBlockUserGroup myCreatedGroup = userGroupComposition.Create(GroupName);
                    IList<PlcBlock> blocks = myCreatedGroup.Blocks.Import(new FileInfo(blockImportPath),
               ImportOptions.Override);
                }
                else
                {
                    IList<PlcBlock> blocks = plcBlockUserGroup.Blocks.Import(new FileInfo(blockImportPath),
               ImportOptions.Override);
                }
            }
            else
            {
                error = true;
                var dialogResult = _dialogHostService.Question("温馨提示", "请连接PLC。");
            }
            return error;
        }

        /// <summary>
        /// 创建背景DB块
        /// </summary>
        public void CreateInstanceDB()
        {
            if (plcSoftware != null)
            {
                PlcBlock plcBlockModbus_Comm_Load_DB = plcSoftware.BlockGroup.Blocks.Find("Modbus_Comm_Load_DB");
                PlcBlock plcBlockModbus_Master_DB = plcSoftware.BlockGroup.Blocks.Find("Modbus_Master_DB");
                if (plcBlockModbus_Comm_Load_DB == null)
                {
                    plcSoftware.BlockGroup.Blocks.CreateInstanceDB("Modbus_Comm_Load_DB", true, 2, "Modbus_Comm_Load");
                }
                else
                {

                }
                if (plcBlockModbus_Master_DB == null)
                {
                    plcSoftware.BlockGroup.Blocks.CreateInstanceDB("Modbus_Master_DB", true, 2, "Modbus_Master");
                }
                else
                {

                }
            }
            else
            {
                var dialogResult = _dialogHostService.Question("温馨提示", "请连接PLC。");
            }
        }

        /// <summary>
        /// 根据驱动的编号获取该驱动的通信模块
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public List<string> GetPortName(int number)
        {
            List<string> ports = new List<string>();

            if (number < projectTIA.Devices.Count)
            {
                ports = projectTIA.Devices[number].Items[0].Items
                      .Where(DeviceItem => DeviceItem.Name.StartsWith("CM") || DeviceItem.Name.StartsWith("CB"))
                      .Select(DeviceItem => DeviceItem.Name)
                      .ToList();
            }
            return ports;
        }

        public void GetPlcSoftware()
        {
            // 获取当前用户的 Windows 身份对象
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal currentPrincipal = new WindowsPrincipal(currentIdentity);

            // 指定要检查的用户组
            string groupName = "Siemens TIA Openness";

            // 检查当前用户是否属于指定的用户组
            bool isInGroup = currentPrincipal.IsInRole(groupName);

            if (isInGroup)
            {
            }
            else
            {
                var dialogResult = _dialogHostService.Question("温馨提示", "请将当前账户添加至Siemens TIA Openness用户组。");
            }
        }

        /// <summary>
        /// Get version info from registry key
        /// </summary>
        /// <returns></returns>
        public static List<string> GetEngineeringVersions()
        {
            RegistryKey key = GetRegistryKey("SOFTWARE\\Siemens\\Automation\\Openness\\");

            if (key != null)
            {
                try
                {
                    var names = key.GetSubKeyNames().OrderBy(x => x).ToList();

                    var result = (from item in names
                                  where Convert.ToDecimal(item.Substring(0, 4)) >= Convert.ToDecimal("V15.0".Substring(1, 4))
                                  select item.Substring(0, 4)).ToList();

                    key.Dispose();

                    return result;
                }
                finally
                {
                    key.Dispose();
                }
            }

            return new List<string>();
        }

        private static RegistryKey GetRegistryKey(string keyName)
        {
            RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey key = baseKey.OpenSubKey(keyName);
            if (key == null)
            {
                baseKey.Dispose();
                baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
                key = baseKey.OpenSubKey(keyName);
            }
            if (key == null)
            {
                baseKey.Dispose();
                baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                key = baseKey.OpenSubKey(keyName);
            }
            baseKey.Dispose();

            return key;
        }

        /// <summary>
        /// 在注册表中为TIA Portal设置白名单条目。
        /// </summary>
        /// <param name="ApplicationName"></param>
        /// <param name="ApplicationStartupPath"></param>
        /// <param name="version"></param>
        public void SetWhitelist(string ApplicationName, string ApplicationStartupPath, string version)
        {
            try
            {
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey software = null;
                try
                {
                    software = key.OpenSubKey("SOFTWARE\\Siemens\\Automation\\Openness")
                        .OpenSubKey(version)
                        .OpenSubKey("Whitelist")
                        .OpenSubKey(ApplicationName + ".exe")
                        .OpenSubKey("Entry", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }
                catch (Exception)
                {
                    software = key.CreateSubKey("SOFTWARE\\Siemens\\Automation\\Openness")
                        .CreateSubKey(version)
                        .CreateSubKey("Whitelist")
                        .CreateSubKey(ApplicationName + ".exe")
                        .CreateSubKey("Entry", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
                }

                string lastWriteTimeUtcFormatted = string.Empty;
                DateTime lastWriteTimeUtc;
                HashAlgorithm hashAlgorithm = SHA256.Create();
                FileStream stream = File.OpenRead(ApplicationStartupPath);
                byte[] hash = hashAlgorithm.ComputeHash(stream);

                string convertedHash = Convert.ToBase64String(hash);
                software.SetValue("FileHash", convertedHash);
                lastWriteTimeUtc = new FileInfo(ApplicationStartupPath).LastWriteTimeUtc;

                lastWriteTimeUtcFormatted = lastWriteTimeUtc.ToString($"yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                software.SetValue("DateModified", lastWriteTimeUtcFormatted);
                software.SetValue("Path", ApplicationStartupPath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }


        }
    }
}
