using MahApps.Metro.Controls;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RSOInventory.Views
{
    /// <summary>
    /// Interaction logic for MetroDialogWindow.xaml
    /// </summary>
    public partial class MyMetroDialogWindow : MetroWindow, IDialogWindow
    {
        public MyMetroDialogWindow()
        {
            InitializeComponent();
        }

        public IDialogResult Result { get; set; }
    }
}
