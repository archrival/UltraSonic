using Subsonic.Common.Classes;
using Subsonic.Common.Interfaces;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
                        UpdateTrackListingGrid(task.Result.Songs);
                        UpdateAlbumGrid(task.Result.Albums);
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
                    if (task.Result == null)
                        return;

                    _currentAlbumArt = task.Result.GetImage();

                    if (_currentAlbumArt != null)
                    {
                        string localFileName = GetCoverArtFilename(child);
                        _currentAlbumArt.Save(localFileName);

                        Dispatcher.Invoke(() => MusicCoverArt.Source = _currentAlbumArt.ToBitmapSource().Resize(BitmapScalingMode.HighQuality, true, (int)(MusicCoverArt.Width * ScalingFactor), (int)(MusicCoverArt.Height * ScalingFactor)));
                    }

                    break;
            }
        }
    }
}
