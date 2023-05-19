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
                .ForMember(dst => dst.ParentId, opt => opt.MapFrom(src => src.Parent.Id));
            });

            containerRegistry.RegisterInstance(cfg.CreateMapper());
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.Register<IInventoryItemRepository, InventoryItemRepository>();
            containerRegistry.Register<ItemsViewModel>();
            containerRegistry.RegisterDialog<NewItem>();
            containerRegistry.RegisterDialogWindow<MyMetroDialogWindow>();

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("MainRegion", typeof(Views.Items));
        }


    }
}
