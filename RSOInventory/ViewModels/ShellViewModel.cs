using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSOInventory.ViewModels
{
    internal class ShellViewModel : BindableBase
    {
        private DelegateCommand<string> navigateCommand;
        private readonly IRegionManager regionManager;

        public DelegateCommand<string> NavigateToCommand { get => navigateCommand ??= new DelegateCommand<string>(HandleNavigation); }


        public ShellViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }
        private void HandleNavigation(string obj)
        {
            regionManager.RequestNavigate("ContentRegion", obj);
        }
    }
}
