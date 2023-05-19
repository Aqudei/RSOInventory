using AutoMapper;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Xaml.Behaviors.Core;
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace RSOInventory.ViewModels
{
    internal class NewItemViewModel : BindableBase, IDialogAware
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
        private readonly string _dataFolder;
        private string _imagePath;
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
        public string ImagePath { get => _imagePath; set => SetProperty(ref _imagePath, value); }
        private InventoryItem _parent;

        private DelegateCommand _browseFileCommand;


        public event Action<IDialogResult> RequestClose;

        public DelegateCommand BrowseFileCommand => _browseFileCommand ??= new DelegateCommand(HandleBrowseFile);

        private void HandleBrowseFile()
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = "Select Image",
                Multiselect = false
            };

            dlg.Filters.Add(new CommonFileDialogFilter("Image Files", "*.jpg;*.jpeg;*.png;*.gif;*.bmp"));
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

        public string Title => "Inventory Item Editor";

        public NewItemViewModel(IRegionManager regionManager, IInventoryItemRepository inventoryItemRepository, IMapper mapper, IEventAggregator eventAggregator)
        {
            _inventoryItemRepository = inventoryItemRepository;
            _eventAggregator = eventAggregator;
            _regionManager = regionManager;
            _mapper = mapper;

            _dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataDir");
            if (!Directory.Exists(_dataFolder))
            {
                Directory.CreateDirectory(_dataFolder);
            }

            Items.AddRange(_inventoryItemRepository.GetAll());
        }

        private void HandleAction(string cmd)
        {
            switch (cmd.ToUpper())
            {
                case "CLOSE":
                    {
                        var result = new DialogResult(ButtonResult.Abort);
                        RequestClose?.Invoke(result);
                        break;
                    }
                case "SAVE":
                    {
                        var oldId = Id;
                        var newItem = _mapper.Map<InventoryItem>(this);
                        if (oldId == 0)
                        {
                            _inventoryItemRepository.Add(newItem);
                            if (!string.IsNullOrWhiteSpace(_imagePath))
                            {
                                var ext = Path.GetExtension(_imagePath);
                                File.Copy(_imagePath, Path.Combine(_dataFolder, $"{newItem.Id}{ext}"), true);
                            }

                            _eventAggregator.GetEvent<PubSubEvent<CrudEvent<InventoryItem>>>().Publish(new CrudEvent<InventoryItem>
                            {
                                CrudAction = CrudEvent<InventoryItem>.CrudActionType.Created,
                                Entity = newItem
                            });


                            var result = new DialogResult(ButtonResult.OK);
                            RequestClose?.Invoke(result);
                        }
                        else
                        {
                            newItem.Id = oldId;
                            _inventoryItemRepository.Update(newItem);
                            if (!string.IsNullOrWhiteSpace(_imagePath) && !_imagePath.Contains( _dataFolder ))
                            {
                                var ext = Path.GetExtension(_imagePath);
                                File.Copy(_imagePath, Path.Combine(_dataFolder, $"{newItem.Id}{ext}"), true);
                            }
                        }
                        break;
                    }
            }
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var hasValue = parameters.TryGetValue<InventoryItem>("SelectedItem", out var selectedItem);
            if (hasValue)
            {
                _mapper.Map(selectedItem, this);
                if (selectedItem.ParentId != 0)
                {
                    Parent = Items.FirstOrDefault(i => i.Id == selectedItem.ParentId);
                }
            }
        }
    }
}
