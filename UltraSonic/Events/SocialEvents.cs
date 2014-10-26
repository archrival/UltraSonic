using Subsonic.Client.Items;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

                        SubsonicClient.AddChatMessageAsync(chatMessage);
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

            NowPlayingItem selectedTrack = source.CurrentItem as NowPlayingItem;

            if (selectedTrack != null)
                AddTrackItemToPlaylist(selectedTrack, _doubleClickBehavior == DoubleClickBehavior.Play);

            _working = false;
        }

        private void NowPlayingRefreshClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(UpdateNowPlaying);
        }

        private void ChatListRefreshClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      _chatMessageSince = 0;
                                      _chatMessages.Clear();
                                      
                                      UpdateChatMessages();
                                  });
        }
    }
}
