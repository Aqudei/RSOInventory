using AutoMapper;
using ImTools;
using Prism.Commands;
using Prism.Mvvm;
using RSOInventory.Data;
using RSOInventory.Data.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace RSOInventory.ViewModels
{
    internal class UsersViewModel : BindableBase, IPage
    {
        private Dispatcher _dispatcher;
        private User selectedUser;
        private string firstName;
        private string lastName;
        private string unit;
        private DelegateCommand<string> formCommand;
        private int id;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

        public User SelectedUser { get => selectedUser; set => SetProperty(ref selectedUser, value); }
        public string FirstName { get => firstName; set => SetProperty(ref firstName, value); }
        public string LastName { get => lastName; set => SetProperty(ref lastName, value); }
        public string Unit { get => unit; set => SetProperty(ref unit, value); }
        public int Id { get => id; set => SetProperty(ref id, value); }

        public DelegateCommand<string> FormActionCommand { get => formCommand ??= new DelegateCommand<string>(HandleFormActionCommand); }

        private void HandleFormActionCommand(string obj)
        {
            switch (obj.ToUpper())
            {
                case "NEW":
                    {
                        FirstName = "";
                        LastName = "";
                        Unit = "";
                        Id = 0;
                        break;
                    }
                case "SAVE":
                    {

                        var user = mapper.Map<User>(this);
                        if (user.Id == 0)
                        {
                            userRepository.Add(user);
                            Users.Add(user);
                        }
                        else
                        {
                            userRepository.Update(user);
                        }
                        break;
                    }
                case "DELETE":
                    {
                        if (SelectedUser != null)
                        {
                            userRepository.Delete(SelectedUser.Id);
                            Users.Remove(selectedUser);
                        }
                        break;
                    }
                case "CANCEL":
                    {
                        if (SelectedUser != null)
                        {
                            mapper.Map(selectedUser, this);
                        }
                        break;
                    }
            }
        }

        public string Title => "Users";

        public UsersViewModel(IUserRepository userRepository, IMapper mapper)
        {
            _dispatcher = Application.Current.Dispatcher;
            this.userRepository = userRepository;
            this.mapper = mapper;
            Task.Run(FetchUsers);

            PropertyChanged += UsersViewModel_PropertyChanged;
        }

        private void UsersViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SelectedUser):
                    {
                        if(SelectedUser != null)
                        {
                            mapper.Map(selectedUser, this);
                        }
                        break;
                    }
            }

        }

        private async void FetchUsers()
        {
            var users = userRepository.GetAll().ToArray();
            foreach (var user in users)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    Users.Add(user);
                });
            }

        }
    }
}
