using System.Windows.Media.Imaging;
using Subsonic.Client.Items;

namespace UltraSonic.Items
{
    public class UltraSonicAlbumItem : AlbumItem
    {
        private BitmapSource _image;

        public BitmapSource Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }
    }
}
