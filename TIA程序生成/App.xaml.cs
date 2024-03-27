//using Prism.DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TIA程序生成.Common;
using TIA程序生成.ViewModels;
using TIA程序生成.Views;

namespace TIA程序生成
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainView>();
        }

       

        protected override void OnInitialized()
        {
            var dialog = Container.Resolve<IDialogService>();

            var connectionString = "Project_Logs.db";
            var levelSwitch = new LoggingLevelSwitch();
            Log.Logger = new LoggerConfiguration()
           .WriteTo.SQLite(connectionString, tableName: "Project_Logs", levelSwitch: levelSwitch)
           .Enrich.FromLogContext() 
           .CreateLogger();

            dialog.ShowDialog("StartView", callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    Environment.Exit(0);
                    return;
                }
            });
            var service = App.Current.MainWindow.DataContext as IConfigureService;
            if (service != null)
                service.Configure();
            base.OnInitialized();

        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDialogHostService, DialogHostService>();
            containerRegistry.RegisterForNavigation<MsgView, MsgViewModel>();
            containerRegistry.RegisterForNavigation<ProgressView, ProgressViewModel>();
            containerRegistry.RegisterForNavigation<StartTIAPortalView, StartTIAPortalViewModel>();
            containerRegistry.RegisterForNavigation<StartView, StartViewModel>();
            containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
            containerRegistry.RegisterForNavigation<SkinView, SkinViewModel>();
            containerRegistry.RegisterForNavigation<AboutView, AboutViewModel>();
            containerRegistry.RegisterForNavigation<TransitionView, TransitionViewModel>();
            containerRegistry.RegisterForNavigation<LogsView, LogsViewModel>();
        }
    }
}
