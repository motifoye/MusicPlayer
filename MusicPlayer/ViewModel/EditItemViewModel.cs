using DevExpress.Mvvm;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKApplication.Model;
//using VKApplication.App.Model;

namespace VKApplication.App.ViewModel
{
    class EditItemViewModel : BaseVM
    {
        public Item ItemInfo { get; set; }

        public DelegateCommand<Window> Save
        {
            get
            {
                return new DelegateCommand<Window>((w) =>
                {
                    w?.Close();
                });
            }
        }
    }
}

