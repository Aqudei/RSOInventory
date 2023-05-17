using AutoMapper;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Xaml.Behaviors.Core;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSOInventory.ViewModels
{
    internal class NewItemViewModel : BindableBase, INavigationAware
    {
        public ObservableCollection<InventoryItem> Items { get; set; } = new ObservableCollection<InventoryItem>();
        public InventoryItem Parent { get => _parent; set => SetProperty(ref _parent, value); }

        private DelegateCommand<string> _actionCommand;
        private int _parentId;
        public int Id { get => _id; set => SetProperty(ref _id, value); }
        private string _serialNumber;
        private string _description;
        private string _name;
        private DateTime? _datePurchased;
        private string _condition;
        private string _location;
        private string _foundInStation = "No";
        private readonly IRegionManager _regionManager;
        private readonly IInventoryItemRepository _inventoryItemRepository;
        private readonly IMapper _mapper;
        private readonly IEventAggregator _eventAggregator;

        public string FoundInStation { get => _foundInStation; set => SetProperty(ref _foundInStation, value); }
        public DelegateCommand<string> ActionCommand => _actionCommand ??= new DelegateCommand<string>(HandleAction);

        public DateTime? DatePurchased { get => _datePurchased; set => SetProperty(ref _datePurchased, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public string SerialNumber { get => _serialNumber; set => SetProperty(ref _serialNumber, value); }
        public int ParentId { get => _parentId; set => SetProperty(ref _parentId, value); }

        public string Location { get => _location; set => SetProperty(ref _location, value); }
        public string Condition { get => _condition; set => SetProperty(ref _condition, value); }
        private string _pinNumber;
        private int _id;
        public string ImagePath { get => imagePath; set => SetProperty(ref imagePath, value); }
        private InventoryItem _parent;

        private DelegateCommand _browseFileCommand;
        private string imagePath;

        public DelegateCommand BrowseFileCommand
        {
            get { return _browseFileCommand ??= new DelegateCommand(HandleBrowseFile); }
        }

        private void HandleBrowseFile()
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = "Select Image",
                Multiselect = false
            };

            dlg.Filters.Add(new CommonFileDialogFilter("Image Files", "*.jpg;*.jpeg;*.png;*.gif"));
            var result = dlg.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                ImagePath = dlg.FileName;
            }
        }

        public string[] Conditions { get; set; } = { "BER", "Servicable" };
        public string PinNumber
        {
            get { return _pinNumber; }
            set { SetProperty(ref _pinNumber, value); }
        }

        public NewItemViewModel(IRegionManager regionManager, IInventoryItemRepository inventoryItemRepository, IMapper mapper, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _inventoryItemRepository = inventoryItemRepository;
            _mapper = mapper;
            _eventAggregator = eventAggregator;
            Items.AddRange(_inventoryItemRepository.GetAll());
        }

        private void HandleAction(string cmd)
        {
            switch (cmd.ToUpper())
            {
                case "CLOSE":
                    {
                        _regionManager.Regions["RightRegion"].RemoveAll();
                        break;
                    }
                case "SAVE":
                    {
                        var oldId = Id;
                        var newItem = _mapper.Map<InventoryItem>(this);
                        if (oldId == 0)
                        {
                            _inventoryItemRepository.Add(newItem);
                            _eventAggregator.GetEvent<PubSubEvent<CrudEvent<InventoryItem>>>().Publish(new CrudEvent<InventoryItem>
                            {
                                CrudAction = CrudEvent<InventoryItem>.CrudActionType.Created,
                                Entity = newItem
                            });

                            _regionManager.Regions["RightRegion"].RemoveAll();
                        }
                        else
                        {
                            newItem.Id = oldId;
                            _inventoryItemRepository.Update(newItem);
                        }
                        break;
                    }
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var hasValue = navigationContext.Parameters.TryGetValue<InventoryItem>("SelectedItem", out var selectedItem);
            if (hasValue)
            {
                _mapper.Map(selectedItem, this);
                if (selectedItem.ParentId != 0)
                {
                    Parent = Items.FirstOrDefault(i => i.Id == selectedItem.ParentId);
                }
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
