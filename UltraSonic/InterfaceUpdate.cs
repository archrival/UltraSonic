using System;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void SetVolumeMetadata()
        {
            VolumeSlider.ToolTip = MediaPlayer.IsMuted ? "Volume: Muted" : $"Volume: {Math.Round(MediaPlayer.Volume*100, 0)}%";
            MuteButton.ToolTip = MediaPlayer.IsMuted ? "UnMute Volume" : "Mute Volume";

            if (MediaPlayer.IsMuted)
                MuteButtonIcon.Name = "VolumeMuted";
            else if (MediaPlayer.Volume >= 0 && MediaPlayer.Volume <= 0.25)
                MuteButtonIcon.Name = "VolumeZero";
            else if (MediaPlayer.Volume > 0.25 && MediaPlayer.Volume <= 0.5)
                MuteButtonIcon.Name = "VolumeLow";
            else if (MediaPlayer.Volume > 0.5 && MediaPlayer.Volume <= 0.75)
                MuteButtonIcon.Name = "VolumeMedium";
            else if (MediaPlayer.Volume > 0.75 && MediaPlayer.Volume <= 1)
                MuteButtonIcon.Name = "VolumeHigh";
        }

        private void UpdateTitle()
        {
            string title = AppName;

            if (MediaPlayer.Source != null)
            {
                title = $"{AppName} - {_nowPlayingTrack.Artist} - {_nowPlayingTrack.Title} [{MusicPlayStatusLabel.Content}]";
                MusicArtistLabel.Text = _nowPlayingTrack.Artist;
                MusicTitleLabel.Text = _nowPlayingTrack.Title;
                MusicAlbumLabel.Text = _nowPlayingTrack.Album;
            }

            Title = title;
        }
    }
}
