﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TIA程序生成.Common.Event;

namespace TIA程序生成.Views
{
    /// <summary>
    /// AboutView.xaml 的交互逻辑
    /// </summary>
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }
        private void EmailButton_OnClick(object sender, RoutedEventArgs e)
           => Link.OpenInBrowser("mailto://zmx371525@outlook.com");
        private void BilibiliButton_OnClick(object sender, RoutedEventArgs e)
          => Process.Start("https://space.bilibili.com/32737701?spm_id_from=333.337.0.0");
    }
}
