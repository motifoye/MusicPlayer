using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKApplication.Model
{
    public class OverlayService : BaseVM
    {
        private static OverlayService _Instance = new OverlayService();
        public static OverlayService GetInstance() => _Instance;

        private OverlayService() { }

        public Action<string, bool> Show { get; set; }

        public string Text { get; set; } = "";
        public bool ProgressBarVisible { get; set; } = true;

        public void Close()
        {
            Text = "";
            ProgressBarVisible = true;
        }

    }
}
