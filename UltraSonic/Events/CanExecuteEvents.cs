using System.Linq;
using System.Windows.Input;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MediaPlayer != null && (MediaPlayer.Source != null || _playbackTrackItems.Any());
        }

        private void PreviousCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MediaPlayer != null && _playbackTrackItems.Any() && (_nowPlayingTrack != null && _playbackTrackItems.IndexOf(_nowPlayingTrack) > 0);
        }

        private void NextCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MediaPlayer != null && _playbackTrackItems.Any() && (_nowPlayingTrack != null && _playbackTrackItems.IndexOf(_nowPlayingTrack) < _playbackTrackItems.Count - 1);
        }
    }
}
