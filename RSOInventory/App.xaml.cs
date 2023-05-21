using AutoMapper;
using LiteDB;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using RSOInventory.Data;
using RSOInventory.Data.Models;
using RSOInventory.ViewModels;
using RSOInventory.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RSOInventory
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {

            return Container.Resolve<Shell>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var database = new LiteDatabase("data.db");
            containerRegistry.RegisterInstance(database);

            var cfg = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<InventoryItem, NewItemViewModel>()
                .ForMember(dst => dst.FoundInStation, opt => opt.MapFrom(src => src.FoundInStation ? "Yes" : "No"))
                .ReverseMap()
                .ForMember(dst => dst.FoundInStation, opt => opt.MapFrom(src => src.FoundInStation.ToUpper() == "Yes" ? true : false))
                .ForMember(dst => dst.ParentId, opt => opt.MapFrom(src => src.Parent.Id))
                .ForMember(dst => dst.EndUser, opt => opt.MapFrom(src => src.EndUser))
                .ForMember(dst => dst.Image, opt => opt.MapFrom(src => src.ImagePath));

                cfg.CreateMap<UsersViewModel, User>().ReverseMap();
                cfg.CreateMap<User, User>().ReverseMap();
            });

            containerRegistry.RegisterInstance(cfg.CreateMapper());
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.Register<IInventoryItemRepository, InventoryItemRepository>();
            containerRegistry.Register<IUserRepository, UserRepository>();
            containerRegistry.Register<ItemsViewModel>();
            containerRegistry.RegisterDialog<NewItem>();
            containerRegistry.RegisterDialogWindow<MyMetroDialogWindow>();



            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("MainRegion", typeof(Items));
            regionManager.RegisterViewWithRegion("MainRegion", typeof(Users));
        }

    }
}
