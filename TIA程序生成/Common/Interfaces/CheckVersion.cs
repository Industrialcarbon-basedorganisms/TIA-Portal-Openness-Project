using DryIoc;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TIA程序生成.Extensions;
using TIA程序生成.Properties;

namespace TIA程序生成.Common.Interfaces
{
    public partial class CheckVersion
    {
        private readonly IDialogHostService _dialogHostService;
        string SelectedEngineeringVersion;
        string SelectedOpennessApiVersion;
        public CheckVersion(IDialogHostService dialogHostService)
        {
            this._dialogHostService = dialogHostService;
        }
        public bool GetPlcSoftware()
        {

            // 获取当前用户的 Windows 身份对象
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal currentPrincipal = new WindowsPrincipal(currentIdentity);

            // 指定要检查的用户组
            string groupName = "Siemens TIA Openness";

            // 检查当前用户是否属于指定的用户组
            bool isInGroup = currentPrincipal.IsInRole(groupName);

            return isInGroup;
        }

        public List<string> GetEngineeringVersions()
        {
            List<string> list = FnGetEngineeringVersions();
            if (list != null && list.Count > 0)
            {
                SelectedEngineeringVersion = list.FirstOrDefault();

                if (string.IsNullOrEmpty(SelectedEngineeringVersion))
                {
                    SelectedEngineeringVersion = list.FirstOrDefault();
                }
            }
            else
            {

            }
            return list;

        }

        public List<string> GetOpennessApiVersions()
        {
            List<string> list = new List<string>();

            if (!string.IsNullOrEmpty(SelectedEngineeringVersion))
            {
                list = GetAssemblies(SelectedEngineeringVersion);
                if (list != null && list.Count > 0)
                {
                    SelectedOpennessApiVersion = list.FirstOrDefault();

                    if (string.IsNullOrEmpty(SelectedOpennessApiVersion))
                    {
                        SelectedOpennessApiVersion = string.Empty;
                    }
                }
                else
                {

                }
            }

            return list;
        }

        /// <summary>
        /// Get version info from registry key
        /// </summary>
        /// <returns></returns>
        public static List<string> FnGetEngineeringVersions()
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

        public static List<string> GetAssemblies(string version)
        {
            RegistryKey key = GetRegistryKey("SOFTWARE\\Siemens\\Automation\\Openness\\" + version);

            if (key != null)
            {
                try
                {
                    var subKey = key.OpenSubKey("PublicAPI");

                    if (subKey != null)
                    {
                        var subKeys = subKey.GetSubKeyNames().OrderBy(x => x).ToList();

                        var result = (from item in subKeys
                                      where Convert.ToDecimal(item.Substring(0, 4)) >= Convert.ToDecimal("V15.0".Substring(1, 4))
                                      select item.Substring(0, 4)).ToList();

                        subKey.Dispose();

                        return result;
                    }
                }
                finally
                {
                    key.Dispose();
                }
            }

            return new List<string>();
        }

        public string AddUserToGroup()
        {
            string message = string.Empty;
            string userName = Environment.UserName; ;
            string groupName = "Siemens TIA Openness";
            try
            {
                // 创建PrincipalContext对象，连接到本地计算机
                using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
                {
                    // 获取用户和组对象
                    UserPrincipal user = UserPrincipal.FindByIdentity(context, userName);
                    GroupPrincipal group = GroupPrincipal.FindByIdentity(context, groupName);

                    // 确保找到了用户和组
                    if (user != null && group != null)
                    {
                        // 检查用户是否已经是组的成员
                        if (!group.Members.Contains(user))
                        {
                            // 将用户添加到组
                            group.Members.Add(user);
                            group.Save();
                            message = "温馨提示,用户已成功添加到用户组,请重启系统。";
                        }
                        else
                        {
                            message = "温馨提示,用户已经是用户组的成员。";
                        }
                    }
                    else
                    {
                        message = "温馨提示,没有找到用户和组。";;
                    }
                }
            }
            catch (Exception ex)
            {
                message = $"错误信息,{ex.Message}";
            }
            return message;
        }

        /// <summary>
        /// 在注册表中为TIA Portal设置白名单条目。
        /// </summary>
        /// <param name="ApplicationName"></param>
        /// <param name="ApplicationStartupPath"></param>
        /// <param name="version"></param>
        public  string SetWhitelist(string ApplicationName, string ApplicationStartupPath, string version)
        {
            string message=string.Empty;
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
                message = e.Message;
            }
            return message;
        }
    }
}
