﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TIA程序生成.Themes.Controls
{
    internal class TabControl : System.Windows.Controls.TabControl
    {
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TabCloseItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TabCloseItem();
        }
    }
}
