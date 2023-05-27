using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using RSOInventory.Data;
using RSOInventory.Data.Models;
using RSOInventory.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ZXing.Common;
using ZXing.QrCode;

namespace RSOInventory.ViewModels
{
    internal class BarcodingViewModel : PageBase
    {
        private readonly IInventoryItemRepository _inventoryItemRepository;
        private readonly IDialogCoordinator _dialogCoordinator;

        public string SearchText { get => searchText; set => SetProperty(ref searchText, value); }
        private ObservableCollection<InventoryItem> _items = new ObservableCollection<InventoryItem>();
        private string searchText;
        private DelegateCommand generateBarcodeCommand;

        public DelegateCommand GenerateBarcodeCommand { get => generateBarcodeCommand ??= new DelegateCommand(HandleGenerateBarcode); }


        public string SanitizeFilename(string filename)
        {
            // Remove invalid characters from the filename
            string sanitizedFilename = Regex.Replace(filename, "[^a-zA-Z0-9_.-]", "");

            // Trim any leading or trailing periods or spaces
            sanitizedFilename = sanitizedFilename.Trim('.', ' ');

            // Ensure the filename is not empty
            if (string.IsNullOrEmpty(sanitizedFilename))
            {
                // If the filename becomes empty after sanitization, generate a default filename
                sanitizedFilename = "default_filename";
            }

            // Ensure the filename is not too long
            const int maxFilenameLength = 255;
            if (sanitizedFilename.Length > maxFilenameLength)
            {
                sanitizedFilename = sanitizedFilename.Substring(0, maxFilenameLength);
            }

            return sanitizedFilename;
        }

        private async void HandleGenerateBarcode()
        {
            using var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog()
            {
                IsFolderPicker = true
            };

            var result = dialog.ShowDialog();
            if (result == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                var progress = await _dialogCoordinator.ShowProgressAsync(this, "Loading", "Please wait while I generate your barcodes...");
                progress.SetIndeterminate();

                try
                {
                    foreach (var selectedItem in _items.Where(i => i.IsSelected))
                    {
                        using var image = GenerateBarcodeForItem(selectedItem);
                        var filename = $"{SanitizeFilename(selectedItem.Name)}.{selectedItem.Id}.{selectedItem.SerialNumber}.{selectedItem.PinNumber}.png";
                        filename = Path.Combine(dialog.FileName, filename);
                        image.Save(filename);
                    }

                    Process.Start("explorer.exe", $"\"{dialog.FileName}\"");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
                finally
                {
                    await progress.CloseAsync();
                }
            }
        }

        private Bitmap GenerateBarcodeForItem(InventoryItem selectedItem)
        {
            var width = 300; // width of the QR Code
            var height = 100; // height of the QR Code
            var margin = 0;
            var qrCodeWriter = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.CODE_128,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = margin
                }
            };

            var qrText = $"{selectedItem.Id}.{selectedItem.SerialNumber}.{selectedItem.PinNumber}";

            var pixelData = qrCodeWriter.Write(qrText);
            return pixelData;
        }

        public ICollectionView Items { get; set; }

        public BarcodingViewModel(IInventoryItemRepository inventoryItemRepository, IDialogCoordinator dialogCoordinator)
        {
            _inventoryItemRepository = inventoryItemRepository;
            _dialogCoordinator = dialogCoordinator;
            Items = CollectionViewSource.GetDefaultView(_items);

            FetchItems();

            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(SearchText):
                        {
                            if (!string.IsNullOrEmpty(searchText))
                            {
                                Items.Filter = i =>
                                {
                                    var item = i as InventoryItem;
                                    var lowerSearchText = searchText.ToLower();
                                    var serialMatched = item.SerialNumber.ToLower().Contains(lowerSearchText);
                                    var pinMatched = item.PinNumber.ToLower().Contains(lowerSearchText);
                                    var nameMatched = item.Name.ToLower().Contains(lowerSearchText);

                                    var match = serialMatched || pinMatched || nameMatched;
                                    return match;
                                };
                            }
                            else
                            {
                                Items.Filter = null;
                            }
                            break;
                        }
                }
            };
        }

        private void FetchItems()
        {
            _items.Clear();

            var items = _inventoryItemRepository.GetAll();

            foreach (var item in items)
            {
                _items.Add(item);
            }
        }

        public override string Title => "Barcoding";
    }
}
