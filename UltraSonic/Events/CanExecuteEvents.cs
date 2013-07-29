using System.Linq;
using System.Windows.Input;
using UltraSonic.Items;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MediaPlayer != null && (MediaPlayer.Source != null || Enumerable.Any<TrackItem>(_playlistTrackItems));
        }

        private void PreviousCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MediaPlayer != null && Enumerable.Any<TrackItem>(_playlistTrackItems) && (_nowPlayingTrack != null && _playlistTrackItems.IndexOf(_nowPlayingTrack) > 0);
        }

        private void NextCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MediaPlayer != null && Enumerable.Any<TrackItem>(_playlistTrackItems) && (_nowPlayingTrack != null && _playlistTrackItems.IndexOf(_nowPlayingTrack) < _playlistTrackItems.Count - 1);
        }
    }
}
