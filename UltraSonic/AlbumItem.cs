using System.Windows.Media.Imaging;
using Subsonic.Rest.Api;

namespace UltraSonic
{
    public class AlbumItem
    {
        public bool Starred { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public Child Album { get; set; }
        public BitmapSource Image { get; set; }
    }
}