using Subsonic.Common;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void PopulateSearchResults(Task<SearchResult2> task)
        {
            _albumListItem = null;

            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        AlbumDataGridNext.Visibility = Visibility.Collapsed;

                        SearchStatusLabel.Content = string.Empty;

                        ProgressIndicator.Visibility = Visibility.Visible;
                        UpdateTrackListingGrid(task.Result.Song);
                        UpdateAlbumGrid(task.Result.Album);
                        ProgressIndicator.Visibility = Visibility.Hidden;
                    });
                    break;
            }
        }

        private void UpdateCoverArt(Task<IImageFormat<Image>> task, Child child)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    _currentAlbumArt = task.Result.GetImage();

                    if (_currentAlbumArt != null)
                    {
                        string localFileName = GetCoverArtFilename(child);
                        _currentAlbumArt.Save(localFileName);

                        Dispatcher.Invoke(() => MusicCoverArt.Source = _currentAlbumArt.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, (int)(MusicCoverArt.Width * 1.5), (int)(MusicCoverArt.Height * 1.5)));
                    }

                    break;
            }
        }
    }
}
