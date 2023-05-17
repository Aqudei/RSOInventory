using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using RSOInventory.Data;
using RSOInventory.Data.Models;
using RSOInventory.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace RSOInventory.ViewModels
{
    internal class ItemsViewModel : BindableBase
    {
        private InventoryItem _selectedParent;
        private readonly IInventoryItemRepository _inventoryItemRepository;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly Dispatcher _currentDispatcher;

        private ObservableCollection<InventoryItem> _parentItems = new ObservableCollection<InventoryItem>();
        public ICollectionView ParentItems { get => parentItems; set => SetProperty(ref parentItems, value); }
        public ObservableCollection<InventoryItem> ChildItems { get; set; } = new ObservableCollection<InventoryItem>();
        public InventoryItem SelectedParent { get => _selectedParent; set => SetProperty(ref _selectedParent, value); }
        private string _searchText;

        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }

        private DelegateCommand<string> _crudActionCommand;
        private ICollectionView parentItems;

        public DelegateCommand<string> CrudActionCommand => _crudActionCommand ??= new DelegateCommand<string>(HandleCrudAction);
        private DelegateCommand _clearFilterCommand;

        public DelegateCommand ClearFilterCommand
        {
            get
            {
                return _clearFilterCommand ??= new DelegateCommand(() =>
                {
                    ParentItems.Filter = null;
                    _searchText = "";
                });
            }
        }


        private void HandleCrudAction(string crudAction)
        {
            switch (crudAction.ToUpper())
            {
                case "CREATE":
                    {
                        _regionManager.RequestNavigate("RightRegion", "NewItem");
                        break;
                    }
                case "UPDATE":
                    {
                        var navParams = new NavigationParameters
                        {
                            { "SelectedItem", SelectedParent }
                        };
                        _regionManager.RequestNavigate("RightRegion", "NewItem", navParams);
                        break;
                    }
                default:
                    break;
            }
        }

        public ItemsViewModel(IInventoryItemRepository inventoryItemRepository, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            PropertyChanged += ItemsViewModel_PropertyChanged;
            _inventoryItemRepository = inventoryItemRepository;
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _currentDispatcher = Application.Current.Dispatcher;
            ParentItems = CollectionViewSource.GetDefaultView(_parentItems);

            Task.Run(LoadItems);

            _eventAggregator.GetEvent<PubSubEvent<CrudEvent<InventoryItem>>>().Subscribe(r =>
            {
                var existing = _parentItems.FirstOrDefault(i => i.Id == r.Entity.Id);
                if (existing != null)
                    _parentItems.Remove(existing);

                if (r.CrudAction == CrudEvent<InventoryItem>.CrudActionType.Created || r.CrudAction == CrudEvent<InventoryItem>.CrudActionType.Updated)
                {
                    _parentItems.Add(r.Entity);
                }
            });
        }

        private void LoadItems()
        {
            foreach (var item in _inventoryItemRepository.GetAll())
            {
                _currentDispatcher.Invoke(() =>
                {
                    _parentItems.Add(item);
                });
            }
        }

        private void ItemsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SelectedParent):
                    {
                        if (SelectedParent != null)
                        {
                            ChildItems.Clear();
                            var children = _inventoryItemRepository.ListChildren(SelectedParent.Id);
                            ChildItems.AddRange(children);
                        }
                        break;
                    }
                case nameof(SearchText):
                    {
                        if (string.IsNullOrWhiteSpace(SearchText) && ParentItems.Filter != null)
                        {
                            ParentItems.Filter = null;
                        }
                        else
                        {
                            if (SearchText.Length >= 3)
                            {
                                ParentItems.Filter = i =>
                                {
                                    var inventoryItem = i as InventoryItem;
                                    return inventoryItem.Name.ToLower().Contains(SearchText.ToLower());
                                };
                            }
                        }


                        break;
                    }
            }
        }
    }
}
