using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSOInventory.ViewModels
{
    internal class UserEditorViewModel : BindableBase, IDialogAware
    {
        private DelegateCommand<string> dialogActionCommand;
        private int _id;
        private string _firstName;
        private string _lastName;

        public string Title => "User Editor";

        public event Action<IDialogResult> RequestClose;
        public DelegateCommand<string> DialogActionCommand { get => dialogActionCommand ??= new DelegateCommand<string>(HandleDialogAction); }

        public int Id { get => _id; set => SetProperty(ref _id, value); }
        public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }
        public string LastName { get => _lastName; set => SetProperty(ref _lastName, value); }

        private void HandleDialogAction(string obj)
        {
            switch (obj.ToUpper())
            {
                case "SAVE":
                    {
                        RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
                        break;
                    }

                case "CLOSE":
                    {
                        RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
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

        }
    }
}
