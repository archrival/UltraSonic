using Subsonic.Rest.Api;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void UpdateCurrentUser(Task<User> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    CurrentUser = task.Result;
                    Dispatcher.Invoke(() =>
                    {
                        AlbumDataGridDownload.Visibility = CurrentUser.DownloadRole ? Visibility.Visible : Visibility.Collapsed;
                        TrackDataGridDownload.Visibility = CurrentUser.DownloadRole ? Visibility.Visible : Visibility.Collapsed;
                        PlaylistTrackGridDownload.Visibility = CurrentUser.DownloadRole ? Visibility.Visible : Visibility.Collapsed;
                        PreviousButton.IsEnabled = CurrentUser.StreamRole;
                        PlayButton.IsEnabled = CurrentUser.StreamRole;
                        StopButton.IsEnabled = CurrentUser.StreamRole;
                        PauseButton.IsEnabled = CurrentUser.StreamRole;
                        NextButton.IsEnabled = CurrentUser.StreamRole;
                        SavePlaylistButton.IsEnabled = CurrentUser.PlaylistRole;
                        PlaylistsGridDeletePlaylist.Visibility = CurrentUser.PlaylistRole ? Visibility.Visible : Visibility.Collapsed;

                        //UserEmailLabel.Content = CurrentUser.Email;
                        //UserScrobblingLabel.Content = CurrentUser.ScrobblingEnabled;
                        //UserAdminLabel.Content = CurrentUser.AdminRole;
                        //UserSettingsLabel.Content = CurrentUser.SettingsRole;
                        //UserStreamLabel.Content = CurrentUser.StreamRole;
                        //UserJukeboxLabel.Content = CurrentUser.JukeboxRole;
                        //UserDownloadLabel.Content = CurrentUser.DownloadRole;
                        //UserUploadLabel.Content = CurrentUser.UploadRole;
                        //UserPlaylistLabel.Content = CurrentUser.PlaylistRole;
                        //UserCoverArtLabel.Content = CurrentUser.CoverArtRole;
                        //UserCommentLabel.Content = CurrentUser.CommentRole;
                        //UserPodcastLabel.Content = CurrentUser.PodcastRole;
                        //UserShareLabel.Content = CurrentUser.ShareRole;

                        if (SubsonicApi.ServerApiVersion >= Version.Parse("1.8.0"))
                            SubsonicApi.GetAvatarAsync(CurrentUser.Username, GetCancellationToken("UpdateCurrentUser")).ContinueWith(UpdateUserAvatar);
                        //else
                        //    UserAvatarImage.Visibility = Visibility.Collapsed;
                    });
                    break;
            }
        }
        
        private void UpdateUserAvatar(Task<Image> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        Image avatarImage = task.Result;

                        //if (avatarImage != null)
                        //    UserAvatarImage.Source = task.Result.ToBitmapSource();
                    });
                    break;
            }
        }
    }
}
