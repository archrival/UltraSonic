using System.Drawing.Imaging;
using System.Text;
using System.Windows.Media.Imaging;
using Subsonic.Rest.Api;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using UltraSonic.Properties;
using Directory = System.IO.Directory;
using Image = System.Drawing.Image;

namespace UltraSonic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string AppName = "UltraSonic";

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellableTasks = new ConcurrentDictionary<string, CancellationTokenSource>();
        private string _cacheDirectory;
        private readonly string _roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private readonly ConcurrentQueue<Uri> _streamItems = new ConcurrentQueue<Uri>();
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly DispatcherTimer _nowPlayingTimer = new DispatcherTimer();
        private readonly DispatcherTimer _chatMessagesTimer = new DispatcherTimer();
        private string _artistFilter = string.Empty;
        private TrackItem _nowPlayingTrack;
        private Image _currentAlbumArt;
        public AlbumArt AlbumArtWindow;
        private TimeSpan _position;
        private bool _repeatPlaylist;
        private int _maxSearchResults = 25;
        private int _maxBitrate;
        private int _albumListMax = 10;
        private int _nowPlayingInterval = 30;
        private int _chatMessagesInterval = 5;
        private string _serverHash;
        private bool _newChatNotify;
        private User CurrentUser { get; set; }
        private bool _useDiskCache = true;
        private string _musicCacheDirectoryName = string.Empty;
        private string _coverArtCacheDirectoryName = string.Empty;
        private Playlist CurrentPlaylist { get; set; }
        private readonly ObservableCollection<NowPlayingItem> _nowPlayingItems = new ObservableCollection<NowPlayingItem>();
        private readonly ObservableCollection<AlbumItem> _albumItems = new ObservableCollection<AlbumItem>();
        private ObservableCollection<ArtistItem> _filteredArtistItems = new ObservableCollection<ArtistItem>();
        private readonly ObservableCollection<ArtistItem> _artistItems = new ObservableCollection<ArtistItem>();
        private readonly ObservableCollection<ChatItem> _chatMessages = new ObservableCollection<ChatItem>();
        private readonly ObservableCollection<TrackItem> _playlistTrackItems = new ObservableCollection<TrackItem>();
        private readonly ObservableCollection<PlaylistItem> _playlistItems = new ObservableCollection<PlaylistItem>();
        private double _chatMessageSince = 0;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                Left = Settings.Default.WindowX;
                Top = Settings.Default.WindowY;
                Height = Settings.Default.Height;
                Width = Settings.Default.Width;
                if (Settings.Default.Maximized)
                    WindowState = WindowState.Maximized;

                PopulateSettings();
                MusicPlayStatusLabel.Content = "Stopped";
                MusicTreeView.DataContext = ArtistItems;
                var playlistTrackDragAndDrop = DataGridDragAndDrop<TrackItem>.Create(_playlistTrackItems, PlaylistTrackGrid, this, playlistDragPopup);

                PlaylistTrackGrid.BeginningEdit += playlistTrackDragAndDrop.DataGridOnBeginEdit;
                PlaylistTrackGrid.CellEditEnding += playlistTrackDragAndDrop.DataGridOnEndEdit;
                PlaylistTrackGrid.PreviewMouseLeftButtonDown += playlistTrackDragAndDrop.DataGridOnMouseLeftButtonDown;
                MainGrid.MouseLeftButtonUp += playlistTrackDragAndDrop.DataGridOnMouseLeftButtonUp;
                MainGrid.MouseMove += playlistTrackDragAndDrop.DataGridOnMouseMove;

                if (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(ServerUrl))
                {
                    InitSubsonicApi();
                    License license = SubsonicApi.GetLicense();
                    UpdateLicenseInformation(license);

                    if (!license.Valid)
                    {
                        MessageBox.Show(string.Format("You must have a valid REST API license to use {0}", AppName));
                    }
                    else
                    {
                        UpdateArtists();
                        UpdatePlaylists();
                        UpdateChatMessages();
                        UpdateNowPlaying();
                    }
                }

                _timer.Interval = TimeSpan.FromMilliseconds(500);
                _timer.Tick += (o, s) => Ticktock();
                _timer.Start();
                _nowPlayingTimer.Interval = TimeSpan.FromSeconds(_nowPlayingInterval);
                _nowPlayingTimer.Tick += (o, s) => UpdateNowPlaying();
                _nowPlayingTimer.Start();
                _chatMessagesTimer.Interval = TimeSpan.FromSeconds(_chatMessagesInterval);
                _chatMessagesTimer.Tick += (o, s) => UpdateChatMessages();
                _chatMessagesTimer.Start();

                MediaPlayer.MediaEnded += (o, args) => PlayNextTrack();
                MediaPlayer.Volume = Settings.Default.Volume;
                MediaPlayer.IsMuted = Settings.Default.VolumeMuted;
                VolumeSlider.Value = MediaPlayer.Volume*10;

                MusicDataGrid.ItemsSource = _albumItems;
                NowPlayingDataGrid.ItemsSource = _nowPlayingItems;
                ChatListView.ItemsSource = _chatMessages;
                PlaylistTrackGrid.ItemsSource = _playlistTrackItems;
                PlaylistsDataGrid.ItemsSource = _playlistItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\n{1}", ex.Message, ex.StackTrace), string.Format("Exception in {0}", AppName), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private SubsonicApi SubsonicApi { get; set; }
        private string Username { get; set; }
        private string Password { get; set; }
        private string ServerUrl { get; set; }
        private string ProxyServer { get; set; }
        private int ProxyPort { get; set; }
        private string ProxyUsername { get; set; }
        private string ProxyPassword { get; set; }
        private bool UseProxy { get; set; }

        private void PopulateSettings()
        {
            Username = Settings.Default.Username;
            Password = Settings.Default.Password;
            ServerUrl = Settings.Default.ServerUrl;
            ProxyUsername = Settings.Default.ProxyUsername;
            ProxyPassword = Settings.Default.ProxyPassword;
            ProxyServer = Settings.Default.ProxyServer;
            ProxyPort = Settings.Default.ProxyPort;
            UseProxy = Settings.Default.UseProxy;
            _maxSearchResults = Settings.Default.MaxSearchResults;
            _albumListMax = Settings.Default.AlbumListMax;
            _cacheDirectory = string.IsNullOrWhiteSpace(Settings.Default.CacheDirectory) ? Path.Combine(Path.Combine(_roamingPath, AppName), "Cache") : Settings.Default.CacheDirectory;
            _useDiskCache = Settings.Default.UseDiskCache;
            _nowPlayingInterval = Settings.Default.NowPlayingInterval;
            _chatMessagesInterval = Settings.Default.ChatMessagesInterval;
            _serverHash = CalculateSha256(ServerUrl, Encoding.Unicode);
            _musicCacheDirectoryName = Path.Combine(Path.Combine(_cacheDirectory, _serverHash), "Music");
            _coverArtCacheDirectoryName = Path.Combine(Path.Combine(_cacheDirectory, _serverHash), "CoverArt");

            if (!Directory.Exists(_musicCacheDirectoryName))
                Directory.CreateDirectory(_musicCacheDirectoryName);

            if (!Directory.Exists(_coverArtCacheDirectoryName))
                Directory.CreateDirectory(_coverArtCacheDirectoryName);

            PopulateSearchResultItemComboBox();
            PopulateMaxBitrateComboBox();
            PopulateAlbumListMaxComboBox();
            PopulateNowPlayingIntervalComboBox();
            PopulateChatMessagesIntervalComboBox();

            PreferencesUseProxyCheckbox.IsChecked = UseProxy;
            PreferencesUsernameTextBox.Text = Username;
            PreferencesPasswordPasswordBox.Password = Password;
            PreferencesServerAddressTextBox.Text = ServerUrl;
            PreferencesUseProxyCheckbox.IsChecked = UseProxy;
            PreferencesProxyServerAddressTextBox.Text = ProxyServer;
            PreferencesProxyServerPortTextBox.Text = ProxyPort.ToString(CultureInfo.InvariantCulture);
            PreferencesProxyServerUsernameTextBox.Text = ProxyUsername;
            PreferencesProxyServerPasswordTextBox.Password = ProxyPassword;
            CacheDirectoryTextBox.Text = _cacheDirectory;
            UseDiskCacheCheckBox.IsChecked = _useDiskCache;

            SetProxyEntryVisibility(UseProxy);
            SetUseDiskCacheVisibility(_useDiskCache);
        }

        private void PopulateSearchResultItemComboBox()
        {
            List<int> listData = new List<int>();
            for (int i = 1; i <= 2500; i++)
                listData.Add(i);

            MaxSearchResultsComboBox.ItemsSource = listData;
            MaxSearchResultsComboBox.SelectedItem = _maxSearchResults;
        }

        private void PopulateAlbumListMaxComboBox()
        {
            List<int> listData = new List<int>();
            for (int i = 1; i <= 500; i++)
                listData.Add(i);

            AlbumListMaxComboBox.ItemsSource = listData;
            AlbumListMaxComboBox.SelectedItem = _albumListMax;
        }

        private void PopulateNowPlayingIntervalComboBox()
        {
            List<int> listData = new List<int>();
            for (int i = 1; i <= 300; i++)
                listData.Add(i);

            NowPlayingIntervalComboBox.ItemsSource = listData;
            NowPlayingIntervalComboBox.SelectedItem = _nowPlayingInterval;
        }

        private void PopulateChatMessagesIntervalComboBox()
        {
            List<int> listData = new List<int>();
            for (int i = 1; i <= 300; i++)
                listData.Add(i);

            ChatMessagesIntervalComboBox.ItemsSource = listData;
            ChatMessagesIntervalComboBox.SelectedItem = _chatMessagesInterval;
        }

        private void PopulateMaxBitrateComboBox()
        {
            List<int> listData = new List<int> { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320 };

            MaxBitrateComboBox.ItemsSource = listData;
            MaxBitrateComboBox.SelectedItem = _maxBitrate;
        }

        private ObservableCollection<ArtistItem> ArtistItems
        {
            get
            {
                if (!string.IsNullOrEmpty(_artistFilter))
                {
                    _filteredArtistItems = new ObservableCollection<ArtistItem>();

                    foreach (ArtistItem artistItem in _artistItems)
                    {
                        IEnumerable<ArtistItem> filteredArtistItems = artistItem.Children.Where(c => c.Name.Contains(_artistFilter, StringComparison.OrdinalIgnoreCase));
                        List<ArtistItem> artistItems = filteredArtistItems as List<ArtistItem> ?? filteredArtistItems.ToList();

                        if (!artistItems.Any()) continue;

                        ArtistItem newArtistItem = new ArtistItem();
                        ObservableCollection<ArtistItem> children = new ObservableCollection<ArtistItem>(artistItems);
                        artistItem.CopyTo(newArtistItem);
                        newArtistItem.Children = children;
                        _filteredArtistItems.Add(newArtistItem);
                    }

                    return _filteredArtistItems;
                }

                return _artistItems;
            }
        }

        private void InitSubsonicApi()
        {
            SubsonicApi = UseProxy ? new SubsonicApi(new Uri(ServerUrl), Username, Password, ProxyPassword, ProxyUsername, ProxyPort, ProxyServer) {UserAgent = AppName} : new SubsonicApi(new Uri(ServerUrl), Username, Password) {UserAgent = AppName};
            SubsonicApi.Ping();

            if (SubsonicApi.ServerApiVersion < Version.Parse("1.8.0"))
            {
                Dispatcher.Invoke(() =>
                    {
                        PlaylistGridStarred.Visibility = Visibility.Collapsed;
                        TrackDataGridStarred.Visibility = Visibility.Collapsed;
                        MusicDataGridStarred.Visibility = Visibility.Collapsed;
                        UserShareLabel.Visibility = Visibility.Hidden;
                        UserShareLabel2.Visibility = Visibility.Hidden;
                    });
            }
            else if (SubsonicApi.ServerApiVersion < Version.Parse("1.4.0"))
            {
                MessageBox.Show(string.Format("{0} requires a Subsonic server with a REST API version of at least 1.4.0", AppName), "Inavlid server API version", MessageBoxButton.OK, MessageBoxImage.Error);
                SubsonicApi = null;
            }

            if (SubsonicApi != null)
            {
                SubsonicApi.GetUserAsync(Username, GetCancellationToken("InitSubsonicApi")).ContinueWith(UpdateCurrentUser);
                ServerApiLabel.Content = SubsonicApi.ServerApiVersion;
            }
        }

        private void Ticktock()
        {
            ProgressSlider.Value = MediaPlayer.Position.TotalMilliseconds;
            MusicTimeRemainingLabel.Content = string.Format("{0:mm\\:ss} / {1:mm\\:ss}", TimeSpan.FromMilliseconds(MediaPlayer.Position.TotalMilliseconds), TimeSpan.FromMilliseconds(_position.TotalMilliseconds));
            UpdateTitle();
        }

        private void UpdateArtists()
        {
            SubsonicApi.GetIndexesAsync().ContinueWith(UpdateArtistsTreeView, GetCancellationToken("UpdateArtists"));
        }

        private void UpdatePlaylists()
        {
            SubsonicApi.GetPlaylistsAsync().ContinueWith(UpdatePlaylists, GetCancellationToken("UpdatePlaylists"));
        }

        private void UpdateNowPlaying()
        {
            SubsonicApi.GetNowPlayingAsync(GetCancellationToken("UpdateNowPlaying")).ContinueWith(UpdateNowPlaying);
        }

        private void UpdateChatMessages()
        {
            SubsonicApi.GetChatMessagesAsync(_chatMessageSince, GetCancellationToken("UpdateNowPlaying")).ContinueWith(UpdateChatMessages);
        }

        private void UpdateLicenseInformation(License license)
        {
            Dispatcher.Invoke(() =>
                {
                    PreferencesLicenseDateLabel.Content = license.Date;
                    PreferencesLicenseEmailLabel.Content = license.Email;
                    PreferencesLicenseKeyLabel.Content = license.Key;
                    PreferencesLicenseValidLabel.Content = license.Valid;
                });
        }

        private static bool IsTrackCached(string fileName, Child child)
        {
            var fi = new FileInfo(fileName);
            return fi.Exists && fi.Length == child.Size;
        }

        private string GetMusicFilename(Child child)
        {
            string fileName = Path.Combine(_musicCacheDirectoryName, child.Id);
            return Path.ChangeExtension(fileName, child.Suffix);
        }

        private string GetCoverArtFilename(Child child)
        {
            string fileName = Path.Combine(_coverArtCacheDirectoryName, child.CoverArt ?? child.Id);
            return fileName;
        }

        private void QueueTrack(TrackItem trackItem)
        {
            Child child = trackItem.Track;
            string fileName = GetMusicFilename(child);
            Uri fileNameUri = new Uri(fileName);

            if (_streamItems != null)
            {
                if (_streamItems.All(s => s.OriginalString == fileName) && IsTrackCached(fileName, child))
                {
                    QueueTrack(fileNameUri, trackItem);
                    UpdateAlbumArt(child);
                }
                else
                {
                    Uri uri = new Uri(fileName);
                    _streamItems.Enqueue(uri);
                    UpdateAlbumArt(child);
                    
                    if (_useDiskCache)
                    {
                        DownloadStatusLabel.Content = "Caching...";
                        Task<long> streamTask = SubsonicApi.StreamAsync(child.Id, fileName, _maxBitrate == 0 ? null : (int?) _maxBitrate, null, null, null, null, GetCancellationToken("QueueTrack"));
                        streamTask.ContinueWith(t => QueueTrack(streamTask, trackItem));
                    }
                    else
                    {
                        QueueTrack(new Uri(SubsonicApi.BuildStreamUrl(child.Id)), trackItem); // Works with non-SSL servers
                    }
                }
            }
        }

        private void CancelTasks(string tokenType)
        {
            CancellationTokenSource token;

            if (_cancellableTasks.TryRemove(tokenType, out token))
                token.Cancel();
        }

        private void QueueTask(string tokenType, CancellationTokenSource token)
        {
            _cancellableTasks.TryAdd(tokenType, token);
        }

        private void QueueTrack(Uri uri, TrackItem trackItem)
        {
            Dispatcher.Invoke(() =>
                {
                    try
                    {
                        StopMusic();
                        MediaPlayer.Source = uri;
                        ProgressSlider.Value = 0;
                        _nowPlayingTrack = trackItem;
                        PlayMusic();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("{0}\n{1}", ex.Message, ex.StackTrace), string.Format("Exception in {0}", AppName), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
        }

        private void PlayTrack(TrackItem trackItem)
        {
            UpdateAlbumArt(trackItem.Track);
            QueueTrack(trackItem);
        }

        private void UpdateAlbumArt(Child child)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      string localFileName = GetCoverArtFilename(child);
                                      if (File.Exists(localFileName))
                                      {
                                          _currentAlbumArt = Image.FromFile(localFileName);
                                          Dispatcher.Invoke(() => MusicCoverArt.Source = _currentAlbumArt.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, (int) MusicCoverArt.Width, (int) MusicCoverArt.Height));
                                      }
                                      else
                                      {
                                          SubsonicApi.GetCoverArtAsync(child.CoverArt, null, GetCancellationToken("UpdateAlbumArt")).ContinueWith(t => UpdateCoverArt(t, child));
                                      }
                                  });
        }

        private CancellationToken GetCancellationToken(string tokenType)
        {
            CancelTasks(tokenType);
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            QueueTask(tokenType, tokenSource);
            return token;
        }

        private void ExpandAll(ItemsControl items, bool expand)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      if (items == null) return;

                                      foreach (object obj in items.Items)
                                      {
                                          var childControl = items.ItemContainerGenerator.ContainerFromItem(obj) as ItemsControl;

                                          if (childControl != null)
                                              ExpandAll(childControl, expand);

                                          var item = childControl as TreeViewItem;

                                          if (item != null)
                                              item.IsExpanded = expand;
                                      }
                                  });
        }

        private void SetProxyEntryVisibility(bool isChecked)
        {
            Dispatcher.Invoke(() =>
                {
                    PreferencesProxyServerAddressTextBox.IsEnabled = isChecked;
                    PreferencesProxyServerPasswordTextBox.IsEnabled = isChecked;
                    PreferencesProxyServerPortTextBox.IsEnabled = isChecked;
                    PreferencesProxyServerUsernameTextBox.IsEnabled = isChecked;
                });
        }

        private void SetUseDiskCacheVisibility(bool isChecked)
        {
            Dispatcher.Invoke(() =>
            {
                CacheDirectoryTextBox.IsEnabled = isChecked;
            });
        }

        private void UpdateAlbumGrid(IEnumerable<Child> children)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      _albumItems.Clear();

                                      SemaphoreSlim throttler = new SemaphoreSlim(initialCount: 50);

                                      foreach (Child child in children)
                                      {
                                          AlbumItem albumItem = new AlbumItem {Artist = child.Artist, Name = child.Album, Album = child, Starred = (child.Starred != default(DateTime))};
                                          _albumItems.Add(albumItem);

                                          throttler.WaitAsync();

                                          try
                                          {
                                              Task.Run(async () =>
                                                                 {
                                                                     try
                                                                     {
                                                                         await Task.Delay(1);
                                                                         Image image = Image.FromFile(GetCoverArtFilename(albumItem.Album));
                                                                         BitmapFrame bitmapFrame = image.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, 200, 200);
                                                                         image.Dispose();
                                                                         bitmapFrame.Freeze();
                                                                         GC.Collect();
                                                                         return bitmapFrame;
                                                                     }
                                                                     finally
                                                                     {
                                                                         throttler.Release();
                                                                     }    
                                                                 }).ContinueWith(t => UpdateAlbumImageArt(t, albumItem));
                                          }
                                          catch
                                          {
                                              DownloadCoverArt(albumItem);
                                          }
                                      }
                                  });
        }

        private void DownloadCoverArt(AlbumItem albumItem)
        {
            SubsonicApi.GetCoverArtAsync(albumItem.Album.CoverArt).ContinueWith(t => UpdateAlbumImageArt(t, albumItem));
        }

        private void UpdateAlbumGridArt()
        {
            Dispatcher.Invoke(() =>
                                  {
                                      foreach (AlbumItem albumItem in _albumItems)
                                      {
                                          try
                                          {
                                              Image thisImage = Image.FromFile(GetCoverArtFilename(albumItem.Album));
                                              albumItem.Image = thisImage.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, 200, 200);
                                              thisImage.Dispose();
                                          }
                                          catch
                                          {
                                              SubsonicApi.GetCoverArtAsync(albumItem.Album.CoverArt).ContinueWith(t => UpdateAlbumImageArt(t, albumItem));
                                          }

                                          GC.Collect();
                                      }
                                  });
        }

        private void UpdatePlaylists(IEnumerable<Playlist> playlists)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      _playlistItems.Clear();

                                      foreach (PlaylistItem playlistItem in playlists.Select(playlist => new PlaylistItem
                                                                                                             {
                                                                                                                 Duration = TimeSpan.FromSeconds(playlist.Duration),
                                                                                                                 Name = playlist.Name,
                                                                                                                 Tracks = playlist.SongCount,
                                                                                                                 Playlist = playlist
                                                                                                             }))
                                      {
                                          _playlistItems.Add(playlistItem);
                                      }

                                      if (SubsonicApi.ServerApiVersion >= Version.Parse("1.8.0"))
                                          SubsonicApi.GetStarredAsync(GetCancellationToken("UpdatePlaylists")).ContinueWith(AddStarredToPlaylists);
                                  });
        }

        private void AddTrackItemToPlaylist(TrackItem trackItem)
        {
            Dispatcher.Invoke(() =>
            {
                TrackItem playlistTrackItem = new TrackItem();
                trackItem.CopyTo(playlistTrackItem);
                playlistTrackItem.PlaylistGuid = Guid.NewGuid();

                _playlistTrackItems.Add(playlistTrackItem);
            });
        }

        private void UpdateTrackListingGrid(IEnumerable<Child> children)
        {
            Dispatcher.Invoke(() => TrackDataGrid.ItemsSource = GetTrackItemCollection(children));
        }
    }
}