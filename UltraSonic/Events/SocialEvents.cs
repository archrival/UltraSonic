using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Subsonic.Client.Models;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void SocialTabGotFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      SocialTab.Style = null;
                                  });
        }

        private void ChatListTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (SubsonicClient == null) return;

            switch (e.Key)
            {
                case Key.Return:
                    {
                        string chatMessage = ChatListInput.Text;

                        if (string.IsNullOrWhiteSpace(chatMessage)) return;

                        SubsonicClient.AddChatMessageAsync(chatMessage).ConfigureAwait(false);
                        ChatListInput.Text = string.Empty;
                    }
                    break;
            }
        }

        private void NowPlayingDataGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_working) return;

            _working = true;

            DataGrid source = e.Source as DataGrid;

            if (source == null) return;

            NowPlayingModel selectedTrack = source.CurrentItem as NowPlayingModel;

            if (selectedTrack != null)
                AddTrackItemToPlaylist(selectedTrack, _doubleClickBehavior == DoubleClickBehavior.Play);

            _working = false;
        }

        private void NowPlayingRefreshClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(UpdateNowPlaying);
        }
    }
}
