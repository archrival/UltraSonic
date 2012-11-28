using System.Windows.Media.Imaging;

namespace UltraSonic
{
    public sealed class AlbumItem : ChildItem
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public int AlbumArtSize { get; set; }
        public BitmapSource Image { get; set; }
    }
}