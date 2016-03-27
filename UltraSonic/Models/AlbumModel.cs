using System.Windows.Media.Imaging;

namespace UltraSonic.Models
{
    public class AlbumModel : Subsonic.Client.Models.AlbumModel
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
