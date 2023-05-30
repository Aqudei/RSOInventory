using MahApps.Metro.IconPacks;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using RSOInventory.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSOInventory.ViewModels
{
    internal class ShellViewModel : BindableBase
    {
        private DelegateCommand<string> navigateCommand;
        private readonly IRegionManager _regionManager;

        public DelegateCommand<string> NavigateToCommand { get => navigateCommand ??= new DelegateCommand<string>(HandleNavigation); }

        public ObservableCollection<MyMenuItem> Menu { get; set; } = new ObservableCollection<MyMenuItem>();
        public ObservableCollection<MyMenuItem> OptionsMenu { get; set; } = new ObservableCollection<MyMenuItem>();

        public ShellViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            Menu.Add(new MyMenuItem
            {
                Label = "Inventory",
                NavigationPath = "Items",
                NavigationType = typeof(Items),
                Icon = new PackIconBootstrapIcons { Kind = PackIconBootstrapIconsKind.CardChecklist }
            });

            Menu.Add(new MyMenuItem
            {
                Label = "Users",
                NavigationPath = "Users",
                NavigationType = typeof(Users),
                Icon = new PackIconBootstrapIcons { Kind = PackIconBootstrapIconsKind.People }

            });

            Menu.Add(new MyMenuItem
            {
                Label = "Barcoding",
                NavigationPath = "Barcoding",
                NavigationType = typeof(Barcoding),
                Icon = new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.BarcodeSolid }

            });
        }
        private void HandleNavigation(string obj)
        {
            _regionManager.RequestNavigate("ContentRegion", obj);
        }
    }
}
