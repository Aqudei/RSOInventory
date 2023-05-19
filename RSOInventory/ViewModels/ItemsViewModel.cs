﻿using Prism.Commands;
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
    internal class ItemsViewModel : BindableBase, IPage
    {
        private InventoryItem _selectedParent;
        private readonly IInventoryItemRepository _inventoryItemRepository;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogService _dialogService;
        private readonly Dispatcher _currentDispatcher;

        private ObservableCollection<InventoryItem> _parentItems = new ObservableCollection<InventoryItem>();
        public ICollectionView ParentItems { get => parentItems; set => SetProperty(ref parentItems, value); }
        public ObservableCollection<InventoryItem> ChildItems { get; set; } = new ObservableCollection<InventoryItem>();
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

        public string Title => "Items";

        private void HandleCrudAction(string crudAction)
        {
            switch (crudAction.ToUpper())
            {
                case "CREATE":
                    {
                        _dialogService.ShowDialog("NewItem");
                        break;
                    }
                case "UPDATE":
                    {
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
            PropertyChanged += ItemsViewModel_PropertyChanged;
            _inventoryItemRepository = inventoryItemRepository;
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
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
                            ChildItems.Clear();
                            var children = _inventoryItemRepository.ListChildren(SelectedParent.Id);
                            ChildItems.AddRange(children);
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
