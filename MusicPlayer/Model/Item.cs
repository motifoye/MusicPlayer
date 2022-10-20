using System;

namespace VKApplication.Model
{
    public class Item : BaseVM
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public double Size { get; set; }
        public Uri Path { get; set; }
        public DateTime DateOfChange { get; set; }
        public string Playlist { get; set; }
    }
}
