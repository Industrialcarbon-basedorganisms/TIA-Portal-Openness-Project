using MyLibrary.Function;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TIA程序生成.Common.DataType;
using TIA程序生成.Common.Models;

namespace TIA程序生成.ViewModels
{
    public class TransitionViewModel : BindableBase
    {
        //创建一个委托命令
        public DelegateCommand<string> ExecuteCommand { get; private set; }
        public DelegateCommand<string[]> ConvertCommand { get; private set; }
        public DelegateCommand<string> GotFocusCommand { get; private set; }
        public TransitionViewModel()
        {
            TransitionDtos = new TransitionDtos();
            ExecuteCommand = new DelegateCommand<string>(Execute);
            GotFocusCommand = new DelegateCommand<string>(Execute1);
            MenuBars = new ObservableCollection<ConvertType>();
            CreateMenuBar();
            ConvertCommand = new DelegateCommand<string[]>(ConvertTo);
            TransitionDtos.SelectedIndex = 1;
        }

        private void Execute1(string obj)
        {
            switch (obj)
            {
                case "HEX":
                    TransitionDtos.SelectedIndex = 0;
                    break;

                case "DEC":
                    TransitionDtos.SelectedIndex = 1;
                    break;
                case "OCT":
                    TransitionDtos.SelectedIndex = 2;
                    break;
                case "BIN":
                    TransitionDtos.SelectedIndex = 3;
                    break;
                case "Float":
                    TransitionDtos.SelectedIndex = 4;
                    break;
            }
        }

        private void ConvertTo(string[] obj)
        {
            try
            {
                string inputData;
                if (!string.IsNullOrWhiteSpace(obj[1]))
                {
                    inputData = obj[1].Replace(" ", "");
                }
                else
                {
                    menuBars[0].Data = "0";
                    menuBars[1].Data = "0";
                    menuBars[2].Data = "0";
                    menuBars[3].Data = "0";
                    menuBars[4].Data = "0";
                    return;
                }
                switch (obj[0])
                {
                    case "HEX":
                        if (TransitionDtos.SelectedIndex == 0)
                        {
                            Int64 decimalValue = Convert.ToInt64(inputData, 16);
                            menuBars[0].Data = InsertFromEnd(inputData.ToString().ToUpper(), 4, ' ');
                            menuBars[1].Data = InsertFromEnd(decimalValue.ToString(), 3, ',');
                            menuBars[2].Data = InsertFromEnd(Convert.ToString(decimalValue, 8), 3, ' ');
                            menuBars[3].Data = InsertFromEnd(Convert.ToString(decimalValue, 2).PadLeft(32, '0'), 4, ' ');
                            menuBars[4].Data = BinaryToFloat(menuBars[3].Data.Replace(" ", "")).ToString("F" + 2);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    case "DEC":
                        if (TransitionDtos.SelectedIndex == 1)
                        {
                            Int64 decimalValue = Convert.ToInt64(inputData.Replace(",", ""));
                            menuBars[0].Data = InsertFromEnd(Convert.ToString(decimalValue, 16).ToUpper(), 4, ' ');
                            menuBars[1].Data = InsertFromEnd(decimalValue.ToString(), 3, ',');
                            menuBars[2].Data = InsertFromEnd(Convert.ToString(decimalValue, 8), 3, ' ');
                            menuBars[3].Data = InsertFromEnd(Convert.ToString(decimalValue, 2).PadLeft(32, '0'), 4, ' ');
                            menuBars[4].Data = BinaryToFloat(menuBars[3].Data.Replace(" ", "")).ToString("F" + 2);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    case "OCT":
                        if (TransitionDtos.SelectedIndex == 2)
                        {
                            Int64 decimalValue = Convert.ToInt64(inputData, 8);
                            menuBars[0].Data = InsertFromEnd(Convert.ToString(decimalValue, 16).ToUpper(), 4, ' ');
                            menuBars[1].Data = InsertFromEnd(decimalValue.ToString(), 3, ',');
                            menuBars[2].Data = InsertFromEnd(inputData, 3, ' ');
                            menuBars[3].Data = InsertFromEnd(Convert.ToString(decimalValue, 2).PadLeft(32, '0'), 4, ' ');
                            menuBars[4].Data = BinaryToFloat(menuBars[3].Data.Replace(" ", "")).ToString("F" + 2);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    case "BIN":
                        if (TransitionDtos.SelectedIndex == 3)
                        {
                            Int64 decimalValue = Convert.ToInt64(inputData, 2);
                            menuBars[0].Data = InsertFromEnd(Convert.ToString(decimalValue, 16).ToUpper(), 4, ' ');
                            menuBars[1].Data = InsertFromEnd(decimalValue.ToString(), 3, ',');
                            menuBars[2].Data = InsertFromEnd(Convert.ToString(decimalValue, 8), 3, ' ');
                            menuBars[3].Data = InsertFromEnd(inputData, 4, ' ');
                            menuBars[4].Data = BinaryToFloat(menuBars[3].Data.Replace(" ", "").PadLeft(32, '0')).ToString("F" + 2);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    case "Float":
                        if (TransitionDtos.SelectedIndex == 4)
                        {

                            // 将浮点数转换为字节数组，并反转字节数组
                            byte[] byteArray = BitConverter.GetBytes(float.Parse(inputData));
                            Array.Reverse(byteArray);

                            // 将字节数组转换为二进制字符串
                            menuBars[3].Data = InsertFromEnd(string.Join("", byteArray.Select(b => Convert.ToString(b, 2).PadLeft(8, '0'))).PadLeft(32, '0'), 4, ' ');
                            Int64 decimalValue = Convert.ToInt64(menuBars[3].Data.Replace(" ", ""), 2);
                            menuBars[0].Data = InsertFromEnd(Convert.ToString(decimalValue, 16).ToUpper(), 4, ' ');
                            menuBars[1].Data = InsertFromEnd(decimalValue.ToString(), 3, ',');
                            menuBars[2].Data = InsertFromEnd(Convert.ToString(decimalValue, 8), 3, ' ');
                            break;
                        }
                        else
                        {
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        static float BinaryToFloat(string binaryString)
        {
            // 将二进制字符串转换为字节数组
            byte[] byteArray = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                byteArray[3 - i] = Convert.ToByte(binaryString.Substring(i * 8, 8), 2);
            }

            // 使用BitConverter将字节数组转换为浮点数
            float result = BitConverter.ToSingle(byteArray, 0);

            return result;
        }

        //委托命令执行事件
        private void Execute(string obj)
        {
            switch (obj)
            {
                case "CRC16Check": CRC16Check(); break;
                case "XORCheck": XORCheck(); break;
                case "FCSCheck": FCSCheck(); break;
                case "CheckSumCheck": CheckSumCheck(); break;
                case "LRCCheck": LRCCheck(); break;
            }
        }

        private TransitionDtos transitionDtos;

        public TransitionDtos TransitionDtos
        {
            get { return transitionDtos; }
            set { transitionDtos = value; RaisePropertyChanged(); }
        }

        public void CRC16Check()
        {
            TransitionDtos.textCRC16Check = CommunicationCheck.CRC16ModbusCheck(TransitionDtos.textInputString);
            TransitionDtos.textOutputString = DataConvert.ToHexByteString(TransitionDtos.textInputString) + TransitionDtos.textCRC16Check;
        }

        public void XORCheck()
        {
            TransitionDtos.textXORCheck = CommunicationCheck.XORCheck(TransitionDtos.textInputString);
            TransitionDtos.textOutputString = DataConvert.ToHexByteString(TransitionDtos.textInputString) + TransitionDtos.textXORCheck;
        }

        public void FCSCheck()
        {
            TransitionDtos.textFCSCheck = CommunicationCheck.FCSCheck(TransitionDtos.textInputString);
            TransitionDtos.textOutputString = DataConvert.ToHexByteString(TransitionDtos.textInputString) + TransitionDtos.textFCSCheck;
        }

        public void CheckSumCheck()
        {
            TransitionDtos.textCheckSumCheck = CommunicationCheck.CheckSumCheck(TransitionDtos.textInputString);
            transitionDtos.textOutputString = DataConvert.ToHexByteString(TransitionDtos.textInputString) + TransitionDtos.textCheckSumCheck;
        }

        public void LRCCheck()
        {
            TransitionDtos.textLRCCheck = CommunicationCheck.LRCCheck(TransitionDtos.textInputString);
            transitionDtos.textOutputString = DataConvert.ToHexByteString(TransitionDtos.textInputString) + TransitionDtos.textLRCCheck;
        }

        private ObservableCollection<ConvertType> menuBars;

        public ObservableCollection<ConvertType> MenuBars
        {
            get { return menuBars; }
            set { menuBars = value; RaisePropertyChanged(); }

        }

        void CreateMenuBar()
        {
            menuBars.Add(new ConvertType() { DataType = "HEX", Data = "0" });
            menuBars.Add(new ConvertType() { DataType = "DEC", Data = "0" });
            menuBars.Add(new ConvertType() { DataType = "OCT", Data = "0" });
            menuBars.Add(new ConvertType() { DataType = "BIN", Data = "0" });
            menuBars.Add(new ConvertType() { DataType = "Float", Data = "0" });
        }

        //static string InsertFromEnd(string input, int interval, char insertChar)
        //{
        //    int length = input.Length;
        //    int spaceCount = (length - 1) / interval;

        //    // 使用 string.Create 创建新的字符串
        //    return string.Create(length + spaceCount, (input, interval, insertChar), (chars, state) =>
        //    {
        //        int i = state.input.Length - 1;
        //        int j = chars.Length - 1;

        //        int count = 0;

        //        while (i >= 0)
        //        {
        //            chars[j--] = state.input[i--];
        //            count++;

        //            if (count % state.interval == 0 && i >= 0)
        //            {
        //                chars[j--] = state.insertChar;
        //            }
        //        }
        //    });
        //}

        static string InsertFromEnd(string input, int interval, char insertChar)
        {
            int length = input.Length;
            int spaceCount = (length - 1) / interval; // 计算需要插入的字符数量

            StringBuilder sb = new StringBuilder(length + spaceCount); // 初始化StringBuilder

            // 从输入字符串的末尾开始遍历
            for (int i = length - 1; i >= 0; i--)
            {
                sb.Insert(0, input[i]); // 将字符插入StringBuilder的开头，模拟从后向前构建字符串

                // 每次插入字符后，检查是否需要插入分隔符
                if (i > 0 && (i % interval == 0))
                {
                    sb.Insert(0, insertChar); // 在当前字符前插入分隔符
                }
            }

            return sb.ToString(); // 将StringBuilder转换为字符串并返回
        }
    }
}
