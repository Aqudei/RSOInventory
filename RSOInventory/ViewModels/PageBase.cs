using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace RSOInventory.ViewModels
{
    public abstract class PageBase : BindableBase
    {
        protected Dispatcher _dispatcher;

        public abstract string Title { get; }

        public PageBase()
        {
            _dispatcher = Application.Current.Dispatcher;
        }
    }
}
