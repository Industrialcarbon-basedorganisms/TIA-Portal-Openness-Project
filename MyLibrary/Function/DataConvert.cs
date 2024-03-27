using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyLibrary.Function
{
    public static class DataConvert
    {
        /// <summary>
        /// 字符串转16进制字节数组 
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] ToHexByteArray(string hexString)
        {
            if (string.IsNullOrWhiteSpace(hexString)) return null;
            hexString = CheakHexString(hexString);
            hexString = hexString.Replace(" ", "");
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// 16进制字节数组转字符串 
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static string ToHexByteString(string hexString)
        {
            if (string.IsNullOrWhiteSpace(hexString)) return null;
            hexString = CheakHexString(hexString);
            hexString = hexString.Replace(" ", "");
            string returnstring = "";
            for (int i = 0; i < hexString.Length / 2; i++)
                returnstring = returnstring + hexString.Substring(i * 2, 2) + " ";
            return returnstring;
        }

        /// <summary>
        /// 字节数组转16进制字符串：空格分隔
        /// </summary>
        /// <param name="byteDatas"></param>
        /// <returns></returns>
        public static string ToFromByteHexString(this byte[] byteDatas)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < byteDatas.Length; i++)
            {
                builder.Append(string.Format("{0:X2} ", byteDatas[i]));
            }
            return builder.ToString().Trim();
        }

        /// <summary>
        /// 校验16进制字符串
        /// </summary>
        /// <param name="strBuffHex">16进制字符串</param>
        /// <returns></returns>
        private static string CheakHexString(string strBuffHex)
        {
            strBuffHex = strBuffHex.Trim();     //去除前后空字符
            strBuffHex = strBuffHex.Replace(',', ' ');  //去掉英文逗号
            strBuffHex = strBuffHex.Replace('，', ' '); //去掉中文逗号
            strBuffHex = strBuffHex.Replace("0x", "");  //去掉0x
            strBuffHex = strBuffHex.Replace("0X", "");  //去掉0X
            strBuffHex = strBuffHex.Replace("\r", "");  //去掉\r
            strBuffHex = strBuffHex.Replace("\n", "");  //去掉\n
            strBuffHex = Regex.Replace(Regex.Replace(strBuffHex, @"(?i)[^a-f\d\s]+", ""), "\\w{3,}", m => string.Join(" ", Regex.Split(m.Value, @"(?<=\G\w{2})(?!$)").Select(x => x.PadLeft(2, '0')).ToArray())).ToUpper();
            return strBuffHex;
        }


    }
}
