using Subsonic.Rest.Api;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using UltraSonic.Properties;
using UltraSonic.Static;
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
        private int _cacheDownloadLimit;
        private int _throttle = 50;
        private string _serverHash;
        private bool _newChatNotify;
        private bool _playbackFollowsCursor;
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
        private double _chatMessageSince;

        private SubsonicApi SubsonicApi { get; set; }
        private string Username { get; set; }
        private string Password { get; set; }
        private string ServerUrl { get; set; }
        private string ProxyServer { get; set; }
        private int ProxyPort { get; set; }
        private string ProxyUsername { get; set; }
        private string ProxyPassword { get; set; }
        private bool UseProxy { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = Settings.Default.WindowLeft;
                Top = Settings.Default.WindowTop;
                Height = Settings.Default.WindowHeight;
                Width = Settings.Default.WindowWidth;
                if (Settings.Default.WindowMaximized)
                    WindowState = WindowState.Maximized;

                PopulateSettings();
                MusicPlayStatusLabel.Content = "Stopped";
                ArtistTreeView.DataContext = ArtistItems;
                var playlistTrackDragAndDrop = DataGridDragAndDrop<TrackItem>.Create(_playlistTrackItems, PlaylistTrackGrid, this, PlaylistDragPopup);

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
                else
                {
                    SettingsTab.IsSelected = true;
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

                AlbumDataGrid.ItemsSource = _albumItems;
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
                        AlbumDataGridStarred.Visibility = Visibility.Collapsed;
                        //UserShareLabel.Visibility = Visibility.Hidden;
                        //UserShareLabel2.Visibility = Visibility.Hidden;
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
                //ServerApiLabel.Content = SubsonicApi.ServerApiVersion;
            }
        }

        private void Ticktock()
        {
            if (MediaPlayer.Source != null)
            {
                ProgressSlider.Value = MediaPlayer.Position.TotalMilliseconds;
                MusicTimeRemainingLabel.Content = string.Format("{0:mm\\:ss} / {1:mm\\:ss}", TimeSpan.FromMilliseconds(MediaPlayer.Position.TotalMilliseconds), TimeSpan.FromMilliseconds(_position.TotalMilliseconds));
                UpdateTitle();
            }
        }

        private void UpdateArtists()
        {
            if (SubsonicApi == null) return;
            SubsonicApi.GetIndexesAsync().ContinueWith(UpdateArtistsTreeView, GetCancellationToken("UpdateArtists"));
        }

        private void UpdateNowPlaying()
        {
            if (SubsonicApi == null) return;
            SubsonicApi.GetNowPlayingAsync(GetCancellationToken("UpdateNowPlaying")).ContinueWith(UpdateNowPlaying);
        }

        private void UpdateChatMessages()
        {
            if (SubsonicApi == null) return;
            SubsonicApi.GetChatMessagesAsync(_chatMessageSince, GetCancellationToken("UpdateNowPlaying")).ContinueWith(UpdateChatMessages);
        }

        private void UpdateLicenseInformation(License license)
        {
            Dispatcher.Invoke(() =>
                {
                    //PreferencesLicenseDateLabel.Content = license.Date;
                    //PreferencesLicenseEmailLabel.Content = license.Email;
                    //PreferencesLicenseKeyLabel.Content = license.Key;
                    //PreferencesLicenseValidLabel.Content = license.Valid;
                });
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

        private void DownloadCoverArt(AlbumItem albumItem)
        {
            SubsonicApi.GetCoverArtAsync(albumItem.Child.CoverArt).ContinueWith(t => UpdateAlbumImageArt(t, albumItem));
        }

        private void UpdateTrackListingGrid(IEnumerable<Child> children)
        {
            Dispatcher.Invoke(() => TrackDataGrid.ItemsSource = GetTrackItemCollection(children));
        }
    }
}