using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TIA程序生成.Common;
using TIA程序生成.Extensions;

namespace TIA程序生成.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        public MainView(IEventAggregator aggregator, IDialogHostService dialogHostService)
        {
            InitializeComponent();
            //注册提示消息
            aggregator.ResgiterMessage(arg =>
            {
                Snackbar.MessageQueue.Enqueue(arg.Message);
            }, "MainView");

            ////注册等待消息窗口
            aggregator.Resgiter(arg =>
            {
                DialogHost.IsOpen = arg.IsOpen;

                if (DialogHost.IsOpen)
                    DialogHost.DialogContent = new ProgressView();
            });

            btnMin.Click += (s, e) => { this.WindowState = WindowState.Minimized; };
            btnMax.Click += (sender, e) =>
            {
                // 设置窗口的 MaxHeight，考虑任务栏高度
                MaxHeight = SystemParameters.WorkArea.Height;
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Maximized;
            };
            btnClose.Click += async (s, e) =>
            {
                var dialogResult = await dialogHostService.Question("温馨提示", "确认退出系统?");
                if (dialogResult.Result != Prism.Services.Dialogs.ButtonResult.OK) return;
                Application.Current.MainWindow.Close();
            };

            toggleMenuButton.Click += BtnDoubleLeft_Click; ;

            ColorZone.MouseMove += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    this.DragMove();
            };

            ColorZone.MouseDoubleClick += (s, e) =>
            {
                // 设置窗口的 MaxHeight，考虑任务栏高度
                MaxHeight = SystemParameters.WorkArea.Height;
                if (this.WindowState == WindowState.Normal)
                    this.WindowState = WindowState.Maximized;
                else
                    this.WindowState = WindowState.Normal;
            };

        }
        private void BtnDoubleLeft_Click(object sender, RoutedEventArgs e)
        {
            CollapseMenu();
        }

        private void CollapseMenu()
        {
            if (StackHeader.Visibility == Visibility.Visible)
            {
                StackHeader.Visibility = Visibility.Collapsed;
                GridLeftMenu.Width = new GridLength(70);
            }
            else
            {
                StackHeader.Visibility = Visibility.Visible;
                GridLeftMenu.Width = new GridLength(220);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置窗口的 MaxHeight，考虑任务栏高度
            MaxHeight = SystemParameters.WorkArea.Height;
        }
    }
}
