using System.Windows.Media.Imaging;
using UltraSonic.Items;

namespace UltraSonic
{
    public sealed class NowPlayingItem : TrackItem
    {
        private BitmapSource _image;

        public string User { get; set; }
        public string When { get; set; }
        public int AlbumArtSize { get; set; }
        public BitmapSource Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
            }
        }
    }
}
