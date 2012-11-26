using System.Windows;
using Subsonic.Rest.Api;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Threading;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void PopulateSearchResults(Task<SearchResult2> task)
        {
            _albumListItem = null;
            AlbumDataGridNext.Visibility = Visibility.Collapsed;

            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        SearchStatusLabel.Content = string.Empty;

                        UpdateTrackListingGrid(task.Result.Song);
                        UpdateAlbumGrid(task.Result.Album);
                    });
                    break;
            }
        }

        private void UpdateCoverArt(Task<Image> task, Child child)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    _currentAlbumArt = task.Result;

                    if (_currentAlbumArt != null)
                    {
                        string localFileName = GetCoverArtFilename(child);
                        _currentAlbumArt.Save(localFileName);

                        Dispatcher.Invoke(() => MusicCoverArt.Source = _currentAlbumArt.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, (int)MusicCoverArt.Width, (int)MusicCoverArt.Height));
                    }

                    break;
            }
        }
    }
}
