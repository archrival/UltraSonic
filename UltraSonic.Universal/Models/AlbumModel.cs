using Subsonic.Common.Interfaces;
using Windows.UI.Xaml.Media.Imaging;

namespace UltraSonic.Models
{
    public class AlbumModel : Subsonic.Client.Models.AlbumModel
    {
        private IImageFormat<SoftwareBitmapSource> _image;

        public IImageFormat<SoftwareBitmapSource> Image
        {
            get { return _image; }
            set
            {
                if (value == _image)
                    return;

                _image = value;
                OnPropertyChanged();
            }
        }
    }
}
