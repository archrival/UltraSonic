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
            if (SubsonicApi == null) return;

            switch (e.Key)
            {
                case Key.Return:
                    {
                        string chatMessage = ChatListInput.Text;

                        if (string.IsNullOrWhiteSpace(chatMessage)) return;

                        SubsonicApi.AddChatMessageAsync(chatMessage);
                        ChatListInput.Text = string.Empty;
                    }
                    break;
            }
        }

        private void NowPlayingDataGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid source = e.Source as DataGrid;

            if (source == null) return;

            NowPlayingItem selectedTrack = source.CurrentItem as NowPlayingItem;

            if (selectedTrack != null)
                Dispatcher.Invoke(() => AddTrackItemToPlaylist(selectedTrack));
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
