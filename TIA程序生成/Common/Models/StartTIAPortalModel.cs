using MyLibrary.DataTypes;
using MyLibrary.Dtos;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIA程序生成.Common.Models
{
    public class StartTIAPortalModel : BaseDto
    {
        private string selectPath;

        public string SelectPath
        {
            get { return selectPath; }
            set { selectPath = value; OnPropertyChanged(); }
        }

        private string projectName;

        public string ProjectName
        {
            get { return projectName; }
            set { projectName = value; OnPropertyChanged(); }
        }

        private string projectAuthor;

        public string ProjectAuthor
        {
            get { return projectAuthor; }
            set { projectAuthor = value; OnPropertyChanged(); }
        }

        private string projectComment;

        public string ProjectComment
        {
            get { return projectComment; }
            set { projectComment = value; OnPropertyChanged(); }
        }

        private string selectFile;

        public string SelectFile
        {
            get { return selectFile; }
            set { selectFile = value; OnPropertyChanged(); }
        }

        private string plcArticle;

        public string PlcArticle
        {
            get { return plcArticle; }
            set { plcArticle = value; OnPropertyChanged(); }
        }

        private string plcVersion;

        public string PlcVersion
        {
            get { return plcVersion; }
            set { plcVersion = value; OnPropertyChanged(); }
        }

        private string plcName;

        public string PlcName
        {
            get { return plcName; }
            set { plcName = value; OnPropertyChanged(); }
        }

        private List<string> tiaPortalProcess;

        public List<string> TiaPortalProcess
        {
            get { return tiaPortalProcess; }
            set { tiaPortalProcess = value; OnPropertyChanged(); }
        }

        private string tiaPortalProcessName;

        public string TiaPortalProcessName
        {
            get { return tiaPortalProcessName; }
            set { tiaPortalProcessName = value; OnPropertyChanged(); }
        }

        private List<string> tiaPortalDevicesName;

        public List<string> TiaPortalDevicesName
        {
            get { return tiaPortalDevicesName; }
            set { tiaPortalDevicesName = value; OnPropertyChanged(); }
        }

        private string tiaPortalDevicesNameSelected;

        public string TiaPortalDevicesNameSelected
        {
            get { return tiaPortalDevicesNameSelected; }
            set { tiaPortalDevicesNameSelected = value; OnPropertyChanged(); }
        }

        private List<string> tiaPortalDevicesPlcBlock;

        public List<string> TiaPortalDevicesPlcBlock
        {
            get { return tiaPortalDevicesPlcBlock; }
            set { tiaPortalDevicesPlcBlock = value; OnPropertyChanged(); }
        }

        private string blockExportPath;

        public string BlockExportPath
        {
            get { return blockExportPath; }
            set { blockExportPath = value; OnPropertyChanged(); }
        }

        private string tiaPortalDevicesPlcBlockName;

        public string TiaPortalDevicesPlcBlockName
        {
            get { return tiaPortalDevicesPlcBlockName; }
            set { tiaPortalDevicesPlcBlockName = value; OnPropertyChanged(); }
        }

        private string importBlocksPath;

        public string ImportBlocksPath
        {
            get { return importBlocksPath; }
            set { importBlocksPath = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ModbusInfo> modbusInfo;

        public ObservableCollection<ModbusInfo> ModbusInfo
        {
            get { return modbusInfo; }
            set { modbusInfo = value; OnPropertyChanged(); }
        }

        private string modbusBaud;

        public string ModbusBaud
        {
            get { return modbusBaud; }
            set { modbusBaud = value; OnPropertyChanged(); }
        }

        private string modbusParity;

        public string ModbusParity
        {
            get { return modbusParity; }
            set { modbusParity = value; OnPropertyChanged(); }
        }

        private List<string> tiaPortalPortName;
        public List<string> TiaPortalPortName
        {
            get { return tiaPortalPortName; }
            set { tiaPortalPortName = value; OnPropertyChanged(); }
        }

        private string selectedPort;

        public string SelectedPort
        {
            get { return selectedPort; }
            set { selectedPort = value; OnPropertyChanged(); }
        }

        private int selectedIndexPort;

        public int SelectedIndexPort
        {
            get { return selectedIndexPort; }
            set { selectedIndexPort = value; OnPropertyChanged(); }
        }

        private string TransitionhexValue;

        /// <summary>
        /// 16进制
        /// </summary>
        public string TransitionHexValue
        {
            get { return TransitionhexValue; }
            set { TransitionhexValue = value; OnPropertyChanged(); }
        }

        private int TransitiondecValue;

        /// <summary>
        /// 10进制
        /// </summary>
        public int TransitionDecValue
        {
            get { return TransitiondecValue; }
            set { TransitiondecValue = value; OnPropertyChanged(); }
        }
    }
}
