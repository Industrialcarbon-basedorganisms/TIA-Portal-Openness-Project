using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using TIA程序生成.Common;
using TIA程序生成.Common.Models;
using TIA程序生成.Extensions;

namespace TIA程序生成.ViewModels
{
    public class MainViewModel : BindableBase, IConfigureService
    {
        public DelegateCommand LoginOutCommand { get; private set; }

        public MainViewModel(IContainerProvider containerProvider,
            IRegionManager regionManager)
        {
            MenuBars = new ObservableCollection<MenuBar>();
            NavigateCommand = new DelegateCommand<MenuBar>(Navigate);
            this.containerProvider = containerProvider;
            this.regionManager = regionManager;

        }

        private void Navigate(MenuBar obj)
        {

            if (obj == null || string.IsNullOrWhiteSpace(obj.NameSpace))
                return;
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(obj.NameSpace, back =>
            {
                journal = back.Context.NavigationService.Journal;
            });
        }

        public DelegateCommand<MenuBar> NavigateCommand { get; private set; }
        private ObservableCollection<MenuBar> menuBars;
        private readonly IContainerProvider containerProvider;
        private readonly IRegionManager regionManager;
        private IRegionNavigationJournal journal;

        public ObservableCollection<MenuBar> MenuBars
        {
            get { return menuBars; }
            set { menuBars = value; RaisePropertyChanged(); }
        }


        void CreateMenuBar()
        {
            menuBars.Add(new MenuBar() { Icon = "/Assets/Images/interaction.png", Title = "Modbus生成", NameSpace = "StartTIAPortalView" });
            menuBars.Add(new MenuBar() { Icon = "/Assets/Images/retweet-.png", Title = "转换助手", NameSpace = "TransitionView" });
            menuBars.Add(new MenuBar() { Icon = "/Assets/Images/setting.png", Title = "系统设置", NameSpace = "SettingsView" });
            menuBars.Add(new MenuBar() { Icon = "/Assets/Images/audit.png", Title = "日志事件", NameSpace = "LogsView" });
            menuBars.Add(new MenuBar() { Icon = "/Assets/Images/info.png", Title = "关于软件", NameSpace = "AboutView" });
        }

        public void Configure()
        {
            CreateMenuBar();
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("StartTIAPortalView");
        }
    }
}
