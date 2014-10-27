using FileLoggerWindows;
using StreamProxyWindows;
using Subsonic.Client;
using Subsonic.Client.Items;
using Subsonic.Client.Windows;
using Subsonic.Common.Classes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using UltraSonic.Properties;
using UltraSonic.Static;
using Image = System.Drawing.Image;

namespace UltraSonic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IDisposable
    {
        private const double ScalingFactor = 1.6180339887;
        private const string AppName = "UltraSonic";
        private const string ClientName = "UltraSonic for Windows";

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellableTasks = new ConcurrentDictionary<string, CancellationTokenSource>();
        private readonly string _roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private readonly ConcurrentQueue<Uri> _streamItems = new ConcurrentQueue<Uri>();
        private readonly DispatcherTimer _mainTimer = new DispatcherTimer();
        private readonly DispatcherTimer _nowPlayingTimer = new DispatcherTimer();
        private readonly DispatcherTimer _chatMessagesTimer = new DispatcherTimer();
        private readonly SemaphoreSlim _cachingThrottle = new SemaphoreSlim(1);
        private readonly ObservableCollection<NowPlayingItem> _nowPlayingItems = new ObservableCollection<NowPlayingItem>();
        private readonly ObservableCollection<AlbumItem> _albumItems = new ObservableCollection<AlbumItem>();
        private readonly ObservableCollection<ArtistItem> _artistItems = new ObservableCollection<ArtistItem>();
        private readonly ObservableCollection<ChatItem> _chatMessages = new ObservableCollection<ChatItem>();
        private readonly ObservableCollection<TrackItem> _playlistTrackItems = new ObservableCollection<TrackItem>();
        private readonly ObservableCollection<TrackItem> _playbackTrackItems = new ObservableCollection<TrackItem>();
        private readonly ObservableCollection<PlaylistItem> _playlistItems = new ObservableCollection<PlaylistItem>();
        private readonly ObservableCollection<TrackItem> _trackItems = new ObservableCollection<TrackItem>();
        public AlbumArt AlbumArtWindow;
        private string _cacheDirectory;
        private string _artistFilter = string.Empty;
        private TrackItem _nowPlayingTrack;
        private Image _currentAlbumArt;
        private TimeSpan _position;
        private int _maxSearchResults = 25;
        private int _maxBitrate;
        private int _albumListMax = 10;
        private int _nowPlayingInterval = 30;
        private int _chatMessagesInterval = 5;
        private int _throttle = 6;
        private int _albumArtSize = 50;
        private string _serverHash;
        private string _musicCacheDirectoryName = string.Empty;
        private string _coverArtCacheDirectoryName = string.Empty;
        private DoubleClickBehavior _doubleClickBehavior = DoubleClickBehavior.Add;
        private AlbumPlayButtonBehavior _albumPlayButtonBehavior = AlbumPlayButtonBehavior.Ask;

        private double _chatMessageSince;
        private AlbumListItem _albumListItem;
        private string _currentPlaylist = string.Empty;
        private string _currentPlaybackList = string.Empty;

        private bool _repeatPlaylist;
        private bool _saveWorkingPlaylist;
        private bool _savePlaybackList;
        private bool _showAlbumArt;
        private bool _newChatNotify;
        private bool _playbackFollowsCursor;
        private bool _shouldCachePlaylist;
        private bool _caching;
        private bool _cachePlaylistTracks = true;
        private bool _movingSlider;
        private bool _working;

        private ObservableCollection<ArtistItem> _filteredArtistItems = new ObservableCollection<ArtistItem>();

        private Playlist CurrentPlaylist { get; set; }
        private User CurrentUser { get; set; }
        private SubsonicClientWindows SubsonicClient { get; set; }
        private string Username { get; set; }
        private string Password { get; set; }
        private string ServerUrl { get; set; }
        private string ProxyServer { get; set; }
        private int ProxyPort { get; set; }
        private string ProxyUsername { get; set; }
        private string ProxyPassword { get; set; }
        private bool UseProxy { get; set; }
        private StreamProxy StreamProxy { get; set; }
        private FileLogger FileLogger { get; set; }
        public static RoutedCommand NextCommand = new RoutedCommand();
        public static RoutedCommand PreviousCommand = new RoutedCommand();
        public static RoutedCommand PlayPauseCommand = new RoutedCommand();

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

                        var newArtistItem = new ArtistItem();
                        var children = new ObservableCollection<ArtistItem>(artistItems);
                        artistItem.CopyTo(newArtistItem);
                        newArtistItem.Children = children;
                        _filteredArtistItems.Add(newArtistItem);
                    }

                    return _filteredArtistItems;
                }

                return _artistItems;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            NextCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Alt));
            PreviousCommand.InputGestures.Add(new KeyGesture(Key.P, ModifierKeys.Alt));
            PlayPauseCommand.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.Alt));

            try
            {
                String appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);
                var appDir = new DirectoryInfo(appData);
                
                if (!appDir.Exists)
                    System.IO.Directory.CreateDirectory(appData);
                
                String logFile = Path.Combine(appData, "ultrasonic.log");

                FileLogger = new FileLogger(logFile, Subsonic.Client.Enums.LoggingLevel.Verbose);
                FileLogger.Log("UltraSonic Started", Subsonic.Client.Enums.LoggingLevel.Basic);
                
                WindowStartupLocation = WindowStartupLocation.Manual;

                FileLogger.Log(string.Format("WindowLeft: {0}", Settings.Default.WindowLeft), Subsonic.Client.Enums.LoggingLevel.Verbose);
                Left = Settings.Default.WindowLeft;

                FileLogger.Log(string.Format("WindowTop: {0}", Settings.Default.WindowTop), Subsonic.Client.Enums.LoggingLevel.Verbose);
                Top = Settings.Default.WindowTop;
                
                FileLogger.Log(string.Format("WindowHeight: {0}", Settings.Default.WindowHeight), Subsonic.Client.Enums.LoggingLevel.Verbose);
                Height = Settings.Default.WindowHeight;

                FileLogger.Log(string.Format("WindowWidth: {0}", Settings.Default.WindowWidth), Subsonic.Client.Enums.LoggingLevel.Verbose);
                Width = Settings.Default.WindowWidth;

                FileLogger.Log(string.Format("WindowMaximized: {0}", Settings.Default.WindowMaximized), Subsonic.Client.Enums.LoggingLevel.Verbose);
                
                if (Settings.Default.WindowMaximized)
                    WindowState = WindowState.Maximized;

                PopulateSettings();
                MusicPlayStatusLabel.Content = "Stopped";
                ArtistTreeView.DataContext = ArtistItems;
                //DataGridDragAndDrop<TrackItem> playlistTrackDragAndDrop = DataGridDragAndDrop<TrackItem>.Create(_playlistTrackItems, PlaylistTrackGrid, this, PlaylistDragPopup);

                //PlaylistTrackGrid.BeginningEdit += playlistTrackDragAndDrop.DataGridOnBeginEdit;
                //PlaylistTrackGrid.CellEditEnding += playlistTrackDragAndDrop.DataGridOnEndEdit;
                //PlaylistTrackGrid.PreviewMouseLeftButtonDown += playlistTrackDragAndDrop.DataGridOnMouseLeftButtonDown;
                //MainGrid.MouseLeftButtonUp += playlistTrackDragAndDrop.DataGridOnMouseLeftButtonUp;
                //MainGrid.MouseMove += playlistTrackDragAndDrop.DataGridOnMouseMove;

                if (StreamProxy == null)
                {
                    FileLogger.Log("Creating StreamProxy", Subsonic.Client.Enums.LoggingLevel.Information);

                    StreamProxy = new StreamProxy();
                    StreamProxy.Start();

                    FileLogger.Log(string.Format("StreamProxy Port: {0}", StreamProxy.GetPort()), Subsonic.Client.Enums.LoggingLevel.Information);
                }

                if (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(ServerUrl))
                {
                    InitSubsonicApi();

                    if (SubsonicClient != null)
                    {
                        License license = new License();
                        license.Valid = true;

                        //var licenseTask = SubsonicClient.GetLicenseAsync();
                        //licenseTask.Wait();

                        //var license = licenseTask.Result;

                        UpdateLicenseInformation(license);

                        if (!license.Valid)
                        {
                            MessageBox.Show(string.Format("You must have a valid REST API license to use {0}", AppName));
                        }
                        else
                        {
                            UpdateArtists();
                            PopulatePlaylist();
                            PopulatePlaybackList();
                            UpdatePlaylists();
                        }
                    }
                }
                else
                {
                    SettingsTab.IsSelected = true;
                    ServerSettingsExpander.IsExpanded = true;
                    SettingsServerAddressTextBox.Focus();
                }

                _mainTimer.Interval = TimeSpan.FromMilliseconds(500);
                _mainTimer.Tick += (o, s) => Ticktock();
                _mainTimer.Start();

                if (_nowPlayingInterval > 0)
                    UpdateNowPlaying();

                _nowPlayingTimer.Interval = TimeSpan.FromSeconds(_nowPlayingInterval);
                _nowPlayingTimer.Tick += (o, s) => UpdateNowPlaying();
                _nowPlayingTimer.Start();

                if (_chatMessagesInterval > 0)
                    UpdateChatMessages();

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
                PlaybackTrackGrid.ItemsSource = _playbackTrackItems;
                PlaylistsDataGrid.ItemsSource = _playlistItems;
                TrackDataGrid.ItemsSource = _trackItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception:\n\n{0}\n{1}", ex.Message, ex.StackTrace), AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitSubsonicApi()
        {
            Uri serverUri = new Uri(ServerUrl);
            string proxyUri = string.IsNullOrWhiteSpace(ProxyServer) ? null : ProxyServer;
            
            if (!ValidateCertificate(serverUri))
            {
                FileLogger.Log("Unable to validate server certificate, this issue must be corrected before continuing.", Subsonic.Client.Enums.LoggingLevel.Error);
                MessageBox.Show("Unable to validate server certificate, this issue must be corrected before continuing.", AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                SubsonicClient = null;
            }
            else
            {
                SubsonicClient = UseProxy ? new SubsonicClientWindows(serverUri, Username, Password, proxyUri, ProxyPort, ProxyUsername, ProxyPassword, ClientName) : new SubsonicClientWindows(serverUri, Username, Password, ClientName);
                //SubsonicClient.PingAsync().Wait();
                SubsonicClient.ServerApiVersion = new Version("1.10.2");
                ServerApiLabel.Text = SubsonicClient.ServerApiVersion.ToString();
                SubsonicClient.GetUserAsync(Username, GetCancellationToken("InitSubsonicApi")).ContinueWith(UpdateCurrentUser);

                FileLogger.Log(string.Format("Subsonic Server API Version: {0}", SubsonicClient.ServerApiVersion), Subsonic.Client.Enums.LoggingLevel.Information);

                if (SubsonicClient.ServerApiVersion < Version.Parse("1.8.0"))
                {
                    Dispatcher.Invoke(() =>
                    {
                        PlaylistGridStarred.Visibility = Visibility.Collapsed;
                        TrackDataGridStarred.Visibility = Visibility.Collapsed;
                        AlbumDataGridStarred.Visibility = Visibility.Collapsed;
                        UserShareLabel.Visibility = Visibility.Hidden;
                        UserShareLabel2.Visibility = Visibility.Hidden;
                    });
                }
                else if (SubsonicClient.ServerApiVersion < Version.Parse("1.4.0"))
                {
                    FileLogger.Log(string.Format("{0} requires a Subsonic server with a REST API version of at least 1.4.0", AppName), Subsonic.Client.Enums.LoggingLevel.Error);
                    MessageBox.Show(string.Format("{0} requires a Subsonic server with a REST API version of at least 1.4.0", AppName), AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                    SubsonicClient = null;
                }
            }
        }

        private static bool ValidateCertificate(Uri serverUri)
        {
            if (serverUri.Scheme == "https")
            {
                X509Certificate2 cert = GetSslCertificate(serverUri);

                if (cert == null) return false;

                if (!cert.Verify())
                {
                    MessageBoxResult result = MessageBox.Show(string.Format("Server certificate is not trusted, would you like to add it to trusted root certificate authorities (CAs)?\n\n\tName: {0}\n\tIssuer: {1}\n\tSerial: {2}\n\tThumbprint: {3}\n\tExpiration: {4}", cert.Subject, cert.Issuer, cert.GetSerialNumberString(), cert.Thumbprint, cert.GetExpirationDateString()), AppName, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No);

                    if (result == MessageBoxResult.Yes)
                    {
                        X509Store store = new X509Store(StoreName.Root);
                        store.Open(OpenFlags.ReadWrite);
                        store.Add(cert);
                        store.Close();
                    }
                }

                return cert.Verify();
            }

            return true;
        }

        private static X509Certificate2 GetSslCertificate(Uri url)
        {
            ServicePoint sp = ServicePointManager.FindServicePoint(url);
            X509Certificate2 cert = null;

            var sslFailureCallback = new RemoteCertificateValidationCallback(delegate { return true; });

            try
            {
                ServicePointManager.ServerCertificateValidationCallback += sslFailureCallback;
                string groupName = Guid.NewGuid().ToString();
                var req = WebRequest.Create(url) as HttpWebRequest;
                if (req != null)
                {
                    req.ConnectionGroupName = groupName;
                    using (WebResponse resp = req.GetResponse()) { }
                }

                sp.CloseConnectionGroup(groupName);

                if (sp.Certificate != null)
                    cert = new X509Certificate2(sp.Certificate);
            }
            catch
            {
                
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback -= sslFailureCallback;
            }

            return cert;
        }

        private void UpdateLicenseInformation(License license)
        {
            Dispatcher.Invoke(() =>
            {
                LicenseDateLabel.Text = license.Date != new DateTime() ? license.Date.ToString(CultureInfo.InvariantCulture) : string.Empty;
                LicenseEmailLabel.Text = license.Email;
                LicenseKeyLabel.Text = license.Key;
                LicenseValidLabel.Text = license.Valid.ToString();
            });
        }

        private async void UpdateArtists()
        {
            if (SubsonicClient == null) return;
            ProgressIndicator.Visibility = Visibility.Visible;
            await SubsonicClient.GetIndexesAsync().ContinueWith(UpdateArtistsTreeView, GetCancellationToken("UpdateArtists"));
            ProgressIndicator.Visibility = Visibility.Hidden;
        }

        private void Ticktock()
        {
            if (MediaPlayer.Source == null) return;

            if (!_movingSlider)
                ProgressSlider.Value = MediaPlayer.Position.TotalMilliseconds;
        
            MusicTimeRemainingLabel.Content = string.Format("{0:mm\\:ss} / {1:mm\\:ss}", TimeSpan.FromMilliseconds(MediaPlayer.Position.TotalMilliseconds), TimeSpan.FromMilliseconds(_position.TotalMilliseconds));
            UpdateTitle();

            if (_cachePlaylistTracks && _shouldCachePlaylist)
                CachePlaylistTracks();
        }

        private async void UpdateNowPlaying()
        {
            if (SubsonicClient == null) return;
            if (_working) return;

            _working = true;
            ProgressIndicator.Visibility = Visibility.Visible;
            await SubsonicClient.GetNowPlayingAsync(GetCancellationToken("UpdateNowPlaying")).ContinueWith(UpdateNowPlaying);
            ProgressIndicator.Visibility = Visibility.Hidden;
            _working = false;
        }

        private async void UpdateChatMessages()
        {
            if (SubsonicClient == null) return;
            if (_working) return;

            _working = true;
            ProgressIndicator.Visibility = Visibility.Visible;
            await SubsonicClient.GetChatMessagesAsync(_chatMessageSince, GetCancellationToken("UpdateNowPlaying")).ContinueWith(UpdateChatMessages);
            ProgressIndicator.Visibility = Visibility.Hidden;
            _working = false;
        }

        protected override void OnClosed(EventArgs e)
        {
 	        base.OnClosed(e);
            Dispose();
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool managed)
        {
            if (_cachingThrottle != null)
                _cachingThrottle.Dispose();

            if (FileLogger != null)
                FileLogger.Dispose();
        }

        private string GetMusicFilename(Child child)
        {
            return GetMusicFilename(child, _musicCacheDirectoryName);
        }

        public static string GetMusicFilename(Child child, string cacheDir)
        {
            string fileName = Path.Combine(cacheDir, child.Id);
            return Path.ChangeExtension(fileName, child.Suffix);
        }

        private string GetCoverArtFilename(Child child)
        {
            string fileName = Path.Combine(_coverArtCacheDirectoryName, child.CoverArt ?? child.Id);
            return fileName;
        }

        private void DownloadCoverArt(AlbumItem albumItem)
        {
            SubsonicClient.GetCoverArtAsync(albumItem.Child.CoverArt).ContinueWith(t => UpdateAlbumImageArt(t, albumItem));
        }

        private void UpdateTrackListingGrid(IEnumerable<Child> children)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressIndicator.Visibility = Visibility.Visible;
                _trackItems.Clear();
                foreach (DataGridColumn column in TrackDataGrid.Columns)
                {
                    column.Width = column.MinWidth;
                    column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                }

                PopulateTrackItemCollection(children);
                UiHelpers.ScrollToTop(TrackDataGrid);
                ProgressIndicator.Visibility = Visibility.Hidden;
            });
        }
    }
}