using System.Windows.Media.Imaging;
using Subsonic.Rest.Api;

namespace UltraSonic
{
    public class AlbumItem
    {
        public bool Selected { get; set; }
        public string Name { get; set; }
        public Child Album { get; set; }
        public BitmapSource Image { get; set; }
    }
}