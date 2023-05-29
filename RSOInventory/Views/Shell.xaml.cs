using MahApps.Metro.Controls;
using Prism;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RSOInventory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Shell
    {
        private IRegionManager _regionManager;

        public Shell(IRegionManager regionManager)
        {
            InitializeComponent();

            _regionManager = regionManager;

            RegionManager.SetRegionName(HamburgerMenuContent, "ContentRegion");
            RegionManager.SetRegionManager(HamburgerMenuContent, _regionManager);
        }

        private void MetroAnimatedSingleRowTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTabItem = e.AddedItems.OfType<FrameworkElement>().FirstOrDefault();
            Debug.WriteLine(e.AddedItems[0].GetType().Name);


            if (selectedTabItem != null)
            {
                var selectedViewModel = selectedTabItem.DataContext as IActiveAware;
                if (selectedViewModel != null)
                {
                    selectedViewModel.IsActive = true;
                }
            }

            var deselectedTabItem = e.RemovedItems.OfType<FrameworkElement>().FirstOrDefault();

            if (deselectedTabItem != null)
            {
                var deselectedViewModel = deselectedTabItem.DataContext as IActiveAware;
                if (deselectedViewModel != null)
                {
                    deselectedViewModel.IsActive = false;
                }
            }
        }

        private void hamburgerMenu_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs args)
        {
            var menu = args.InvokedItem as HamburgerMenuIconItem;
            if (menu != null)
            {
                _regionManager.RequestNavigate("ContentRegion", menu.CommandParameter.ToString());
            }

        }
    }
}
