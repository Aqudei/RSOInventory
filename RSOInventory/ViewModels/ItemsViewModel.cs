using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
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
    internal class ItemsViewModel : PageBase
    {
        private InventoryItem _selectedParent;
        private readonly IInventoryItemRepository _inventoryItemRepository;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogService _dialogService;
        private readonly Dispatcher _currentDispatcher;

        private ObservableCollection<InventoryItem> _parentItems = new ObservableCollection<InventoryItem>();
        public ICollectionView ParentItemsView { get => parentItems; set => SetProperty(ref parentItems, value); }
        private ObservableCollection<InventoryItem> _childItems = new ObservableCollection<InventoryItem>();
        public ICollectionView ChildItemsView { get => _childItemsView; set => SetProperty(ref _childItemsView, value); }

        public InventoryItem SelectedParent { get => _selectedParent; set => SetProperty(ref _selectedParent, value); }
        public InventoryItem SelectedChild { get => _selectedChild; set => SetProperty(ref _selectedChild, value); }
        private string _searchText;

        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }

        private DelegateCommand<string> _crudActionCommand;
        private ICollectionView parentItems;

        public DelegateCommand<string> CrudActionCommand => _crudActionCommand ??= new DelegateCommand<string>(HandleCrudAction);
        private DelegateCommand _clearFilterCommand;
        private string parentImage;
        private string childImage;
        private InventoryItem _selectedChild;

        public string ParentImage { get => parentImage; set => SetProperty(ref parentImage, value); }
        public string ChildImage { get => childImage; set => SetProperty(ref childImage, value); }

        private DelegateCommand<string> _subItemCrudActionCommand;
        private ICollectionView _childItemsView;

        public DelegateCommand<string> SubItemCrudActionCommand
        {
            get { return _subItemCrudActionCommand ??= new DelegateCommand<string>(HandleSubItemCrudAction); }
        }

        private void HandleSubItemCrudAction(string command)
        {
            switch (command.ToUpper())
            {
                case "UPDATE":
                    {
                        if (SelectedChild == null)
                            return;

                        var parameters = new DialogParameters
                        {
                            { "SelectedItem", SelectedChild },
                        };

                        _dialogService.Show("NewItem", parameters, r =>
                        {

                        });
                        break;
                    }
                case "DELETE":
                    {
                        _inventoryItemRepository.Delete(SelectedChild.Id);
                        _childItems.Remove(SelectedChild);
                        break;
                    }

                case "NEW":
                    {
                        if (SelectedParent == null)
                            return;


                        var parameters = new DialogParameters
                        {
                            { "SelectedItem", SelectedParent },
                            { "IsSubItem", true }
                        };

                        _dialogService.Show("NewItem", parameters, r =>
                        {

                        });
                        break;
                    }
            }
        }

        public DelegateCommand ClearFilterCommand
        {
            get
            {
                return _clearFilterCommand ??= new DelegateCommand(() =>
                {
                    ParentItemsView.Filter = null;
                    _searchText = "";
                });
            }
        }

        public override string Title => "Items";

        private void HandleCrudAction(string crudAction)
        {
            switch (crudAction.ToUpper())
            {
                case "DELETE":
                    {
                        _inventoryItemRepository.Delete(SelectedParent.Id);
                        _parentItems.Remove(SelectedParent);
                        break;
                    }
                case "CREATE":
                    {
                        _dialogService.ShowDialog("NewItem");
                        break;
                    }
                case "UPDATE":
                    {
                        if (SelectedParent == null)
                            return;

                        var navParams = new DialogParameters
                        {
                            { "SelectedItem", SelectedParent }
                        };

                        _dialogService.ShowDialog("NewItem", navParams, r =>
                        {

                        });
                        break;
                    }
                default:
                    break;
            }
        }

        public ItemsViewModel(IInventoryItemRepository inventoryItemRepository, IRegionManager regionManager, IEventAggregator eventAggregator, IDialogService dialogService)
        {
            _inventoryItemRepository = inventoryItemRepository;
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
            _currentDispatcher = Application.Current.Dispatcher;
            ParentItemsView = CollectionViewSource.GetDefaultView(_parentItems);
            ChildItemsView = CollectionViewSource.GetDefaultView(_childItems);

            Task.Run(LoadItems);

            _eventAggregator.GetEvent<PubSubEvent<CrudEvent<InventoryItem>>>().Subscribe(r =>
            {
                var parentItem = _parentItems.FirstOrDefault(i => i.Id == r.Entity.Id);
                if (parentItem != null)
                {
                    _parentItems.Remove(parentItem);
                }

                var childItem = _childItems.FirstOrDefault(i => i.Id == r.Entity.Id);
                if (childItem != null)
                {
                    _childItems.Remove(childItem);
                }


                if (r.CrudAction == CrudEvent<InventoryItem>.CrudActionType.Created || r.CrudAction == CrudEvent<InventoryItem>.CrudActionType.Updated)
                {
                    if (r.Entity.ParentId == 0)
                    {
                        _parentItems.Add(r.Entity);
                    }
                    else
                    {
                        _childItems.Add(r.Entity);
                    }
                }
            });

            PropertyChanged += ItemsViewModel_PropertyChanged;

        }

        private void LoadItems()
        {
            foreach (var item in _inventoryItemRepository.GetParentItems())
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
                            _childItems.Clear();
                            var children = _inventoryItemRepository.ListChildren(SelectedParent.Id);
                            _childItems.AddRange(children);
                            ParentImage = SelectedParent.Image;
                        }
                        break;
                    }
                case nameof(SelectedChild):
                    {
                        if (SelectedChild != null)
                        {
                            ChildImage = SelectedChild.Image;
                        }
                        break;
                    }
                case nameof(SearchText):
                    {
                        if (string.IsNullOrWhiteSpace(SearchText) && ParentItemsView.Filter != null)
                        {
                            ParentItemsView.Filter = null;
                        }
                        else
                        {
                            if (SearchText.Length >= 3)
                            {
                                ParentItemsView.Filter = i =>
                                {
                                    var inventoryItem = i as InventoryItem;

                                    var serialMatched = inventoryItem.SerialNumber.Contains(SearchText);
                                    var pinMatched = inventoryItem.PinNumber.Contains(SearchText);

                                    var childrenMatched = _childItems.Where(c => c.SerialNumber.Contains(SearchText) || c.PinNumber.Contains(SearchText)).ToList();
                                    var parentMatched = childrenMatched.Select(c => c.ParentId).Contains(inventoryItem.Id);
                                    var nameMatched = inventoryItem.Name.ToLower().Contains(SearchText.ToLower());

                                    return parentMatched || serialMatched || pinMatched || nameMatched;
                                };
                            }
                        }
                        break;
                    }
            }
        }
    }
}
