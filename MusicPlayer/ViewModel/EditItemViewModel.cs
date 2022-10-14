using DevExpress.Mvvm;
using System.Windows;
using VKApplication.Model;

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

