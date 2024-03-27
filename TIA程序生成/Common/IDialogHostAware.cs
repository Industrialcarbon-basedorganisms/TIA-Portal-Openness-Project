﻿using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIA程序生成.Common
{
    public interface IDialogHostAware 
    {
        /// <summary>
        /// DialoHost名称
        /// </summary>
        string DialogHostName { get; set; }

        /// <summary>
        /// 打开过程中执行
        /// </summary>
        /// <param name="parameters"></param>
        void OnDialogOpend(IDialogParameters parameters);

        /// <summary>
        /// 确定
        /// </summary>
        DelegateCommand SaveCommand { get; set; }

        /// <summary>
        /// 取消
        /// </summary>
        DelegateCommand CancelCommand { get; set; }
    }
}
