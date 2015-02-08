using Subsonic.Common;
using Subsonic.Common.Classes;
using Subsonic.Common.Interfaces;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using UltraSonic.Static;

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

                        UserEmailLabel.Text = CurrentUser.Email;
                        UserScrobblingLabel.Text = CurrentUser.ScrobblingEnabled.ToString();
                        UserAdminLabel.Text = CurrentUser.AdminRole.ToString();
                        UserSettingsLabel.Text = CurrentUser.SettingsRole.ToString();
                        UserStreamLabel.Text = CurrentUser.StreamRole.ToString();
                        UserJukeboxLabel.Text = CurrentUser.JukeboxRole.ToString();
                        UserDownloadLabel.Text = CurrentUser.DownloadRole.ToString();
                        UserUploadLabel.Text = CurrentUser.UploadRole.ToString();
                        UserPlaylistLabel.Text = CurrentUser.PlaylistRole.ToString();
                        UserCoverArtLabel.Text = CurrentUser.CoverArtRole.ToString();
                        UserCommentLabel.Text = CurrentUser.CommentRole.ToString();
                        UserPodcastLabel.Text = CurrentUser.PodcastRole.ToString();
                        UserShareLabel.Text = CurrentUser.ShareRole.ToString();

                        if (SubsonicServer.GetApiVersion() >= SubsonicApiVersions.Version1_8_0)
                            SubsonicClient.GetAvatarAsync(CurrentUser.Username, GetCancellationToken("UpdateCurrentUser")).ContinueWith(UpdateUserAvatar);
                        else
                        {
                            AvatarImage.Visibility = Visibility.Collapsed;
                            AvatarLabel.Visibility = Visibility.Collapsed;
                            AvatarBorder.Visibility = Visibility.Collapsed;
                        }
                    });
                    break;
            }
        }
        
        private void UpdateUserAvatar(Task<IImageFormat<Image>> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        using (Image avatarImage = task.Result == null ? null : task.Result.GetImage())
                        {
                            if (avatarImage != null)
                            {
                                AvatarImage.Visibility = Visibility.Visible;
                                AvatarLabel.Visibility = Visibility.Visible;
                                AvatarBorder.Visibility = Visibility.Visible;
                                AvatarBorder.Height = avatarImage.Height;
                                AvatarBorder.Width = avatarImage.Width;
                                AvatarImage.Source = avatarImage.ToBitmapSource();
                            }
                            else
                            {
                                AvatarImage.Visibility = Visibility.Collapsed;
                                AvatarLabel.Visibility = Visibility.Collapsed;
                                AvatarBorder.Visibility = Visibility.Collapsed;
                            }
                        }

                        if (task.Result != null) 
                            task.Result.Dispose();
                    });
                    break;
            }
        }
    }
}
