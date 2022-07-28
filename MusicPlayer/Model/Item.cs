using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using VKApplication.App.Model;

namespace VKApplication.Model
{
    public class Item : BaseVM
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public double Size { get; set; }
        public Uri Path { get; set; }
        public DateTime DateOfChange{ get; set; }
    }
}
