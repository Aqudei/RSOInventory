using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace RSOInventory.Views
{
    internal class MyMenuItem : MahApps.Metro.Controls.HamburgerMenuIconItem
    {
        public string NavigationPath { get; set; }
        public Type NavigationType { get; set; }

    }
}
