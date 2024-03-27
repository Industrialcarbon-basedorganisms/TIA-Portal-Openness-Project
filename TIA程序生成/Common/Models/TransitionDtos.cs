using MyLibrary.Dtos;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIA程序生成.Common.Models
{
    public class TransitionDtos : BaseDto
    {
        private string textoutputstring;

        /// <summary>
        /// 输入字符串
        /// </summary>
        public string textOutputString
        {
            get { return textoutputstring; }
            set { textoutputstring = value; OnPropertyChanged(); }
        }

        private string textinputstring;

        /// <summary>
        /// 输出字符串
        /// </summary>
        public string textInputString
        {
            get { return textinputstring; }
            set { textinputstring = value; OnPropertyChanged(); }
        }

        private string textcrc16check;

        /// <summary>
        /// CRC16校验结果
        /// </summary>
        public string textCRC16Check
        {
            get { return textcrc16check; }
            set { textcrc16check = value; OnPropertyChanged(); }
        }

        private string textxorcheck;

        /// <summary>
        /// XOR校验结果
        /// </summary>
        public string textXORCheck
        {
            get { return textxorcheck; }
            set { textxorcheck = value; OnPropertyChanged(); }
        }

        private string textfcscheck;

        /// <summary>
        /// FCS校验结果
        /// </summary>
        public string textFCSCheck
        {
            get { return textfcscheck; }
            set { textfcscheck = value; OnPropertyChanged(); }
        }

        private string textchecksumcheck;

        /// <summary>
        /// LRC校验结果
        /// </summary>
        public string textCheckSumCheck
        {
            get { return textchecksumcheck; }
            set { textchecksumcheck = value; OnPropertyChanged(); }
        }

        private string textlrccheck;

        /// <summary>
        /// LRC校验结果
        /// </summary>
        public string textLRCCheck
        {
            get { return textlrccheck; }
            set { textlrccheck = value; OnPropertyChanged(); }
        }

        private string hexValue;

        /// <summary>
        /// 16进制
        /// </summary>
        public string HexValue
        {
            get { return hexValue; }
            set { hexValue = value; OnPropertyChanged(); }
        }

        private string decValue;

        /// <summary>
        /// 10进制
        /// </summary>
        public string DecValue
        {
            get { return decValue; }
            set { decValue = value; OnPropertyChanged(); }
        }

        private string octValue;

        /// <summary>
        /// 8进制
        /// </summary>
        public string OctValue
        {
            get { return octValue; }
            set { octValue = value; OnPropertyChanged(); }
        }

        private string binValue;

        /// <summary>
        /// 2进制
        /// </summary>
        public string BinValue
        {
            get { return binValue; }
            set { binValue = value; OnPropertyChanged(); }
        }

        private string floatValue;

        /// <summary>
        /// 浮点数
        /// </summary>
        public string FloatValue
        {
            get { return floatValue; }
            set { floatValue = value; OnPropertyChanged(); }
        }

        private int selectedIndex;

        /// <summary>
        /// 浮点数
        /// </summary>
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { selectedIndex = value; OnPropertyChanged(); }
        }

        private int caretIndex;

        /// <summary>
        /// 浮点数
        /// </summary>
        public int CaretIndex
        {
            get { return caretIndex; }
            set { caretIndex = value; OnPropertyChanged(); }
        }
    }
}
