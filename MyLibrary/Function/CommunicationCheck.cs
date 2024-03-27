using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;

namespace MyLibrary.Function
{
    public static class CommunicationCheck
    {
        /// <summary>
        /// CRC16Modbus校验 
        /// </summary>
        /// <param name="CheckString"></param>
        /// <returns></returns>
        public static string CRC16ModbusCheck(string CheckString)
        {
            if (string.IsNullOrWhiteSpace(CheckString)) return null;
            byte[] buffer = DataConvert.ToHexByteArray(CheckString);
            ushort CRC16 = 0xFFFF;
            for (int i = 0; i < buffer.Length; i++)
            {
                CRC16 ^= buffer[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((CRC16 & 1) > 0)
                        CRC16 = (ushort)((CRC16 >> 1) ^ 0xA001);
                    else
                        CRC16 = (ushort)(CRC16 >> 1);
                }
            }
            byte[] crc16 = BitConverter.GetBytes(CRC16);

            string returnstring = DataConvert.ToFromByteHexString(crc16);

            return returnstring;
        }

        /// <summary>
        /// XOR校验 
        /// </summary>
        /// <param name="CheckString"></param>
        /// <returns></returns>
        public static string XORCheck(string CheckString)
        {
            if (string.IsNullOrWhiteSpace(CheckString)) return null;
            byte[] buffer = DataConvert.ToHexByteArray(CheckString);
            byte CheckCode = 0;
            
            for (int i = 0; i < buffer.Length; i++)
            {
                CheckCode ^= buffer[i];
            }

            return CheckCode.ToString("X2");
        }

        /// <summary>
        /// FCS校验 
        /// </summary>
        /// <param name="CheckString"></param>
        /// <returns></returns>
        public static string FCSCheck(string CheckString)
        {
            if (string.IsNullOrWhiteSpace(CheckString)) return null;
            CheckString = DataConvert.ToHexByteString(CheckString);
            CheckString = CheckString.Trim().Replace(" ", "");
            int buffer = 0;
            for (int i = 0; i < CheckString.Length; i++)
            {
                buffer = buffer + Convert.ToInt32(CheckString[i]);
            }
            if (buffer > 0xFF)
            {
                buffer = buffer % 256;
            }
            byte[] buffer2 = { (Byte)buffer };
            return DataConvert.ToFromByteHexString(buffer2);
        }

        /// <summary>
        /// CheckSum校验 
        /// </summary>
        /// <param name="CheckString"></param>
        /// <returns></returns>
        public static string CheckSumCheck(string CheckString)
        {
            if(string.IsNullOrWhiteSpace(CheckString)) return null;
            byte[] CheckString2 = DataConvert.ToHexByteArray(CheckString);
            int buffer = 0;
            for (int i = 0; i < CheckString2.Length; i++)
            {
                buffer += CheckString2[i];
            }
            if (buffer > 0xFF)
            {
                buffer = buffer % 256;
            }
            byte[] buffer2 = { (Byte)buffer };
            return DataConvert.ToFromByteHexString(buffer2);
        }

        /// <summary>
        /// LRC校验 
        /// </summary>
        /// <param name="CheckString"></param>
        /// <returns></returns>
        public static string LRCCheck(string CheckString)
        {
            if (string.IsNullOrWhiteSpace(CheckString)) return null;
            byte[] CheckString2 = DataConvert.ToHexByteArray(CheckString);
            byte buffer = 0;
            for (int i = 0; i < CheckString2.Length; i++)
            {
                buffer += CheckString2[i];
            }
            buffer = (byte)((buffer ^ 0xFF) + 1);

            byte[] buffer2 = { buffer };
            return DataConvert.ToFromByteHexString(buffer2);
        }
    }
}
