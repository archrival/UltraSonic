using System.Linq;
using System.Windows.Input;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MediaPlayer != null && (MediaPlayer.Source != null || _playlistTrackItems.Any());
        }

        private void PreviousCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MediaPlayer != null && _playlistTrackItems.Any() && (_nowPlayingTrack != null && _playlistTrackItems.IndexOf(_nowPlayingTrack) > 0);
        }

        private void NextCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MediaPlayer != null && _playlistTrackItems.Any() && (_nowPlayingTrack == null || (_nowPlayingTrack != null && _playlistTrackItems.IndexOf(_nowPlayingTrack) < _playlistTrackItems.Count - 1));
        }
    }
}
