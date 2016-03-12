using System.Threading.Tasks;
using Subsonic.Client;
using Subsonic.Client.Enums;
using Subsonic.Client.Handlers;
using Subsonic.Client.Items;
using Subsonic.Client.Monitors;
using Subsonic.Client.Windows;
using Subsonic.Common;
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
using Subsonic.Common.Interfaces;
using UltraSonic.Items;
using UltraSonic.Properties;
using UltraSonic.Static;
using Directory = System.IO.Directory;
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
        private readonly ObservableCollection<UltraSonicNowPlayingItem> _nowPlayingItems = new ObservableCollection<UltraSonicNowPlayingItem>();
        private readonly ObservableCollection<UltraSonicAlbumItem> _albumItems = new ObservableCollection<UltraSonicAlbumItem>();
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
        private StreamParameters _streamParameters;
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

        //private double _chatMessageSince;
        private AlbumListItem _albumListItem;
        private string _currentPlaylist = string.Empty;
        private string _currentPlaybackList = string.Empty;

        private bool _repeatPlaylist;
        private bool _saveWorkingPlaylist;
        private bool _savePlaybackList;
        private bool _showAlbumArt;
        //private bool _newChatNotify;
        private bool _playbackFollowsCursor;
        private bool _shouldCachePlaylist;
        private bool _caching;
        private bool _cachePlaylistTracks = true;
        private bool _movingSlider;
        private bool _working;

        private ObservableCollection<ArtistItem> _filteredArtistItems = new ObservableCollection<ArtistItem>();

        private Playlist CurrentPlaylist { get; set; }
        private User CurrentUser { get; set; }
        private ISubsonicClient<Image> SubsonicClient { get; set; }
        private SubsonicServer SubsonicServer { get; set; }
        private string Username { get; set; }
        private string Password { get; set; }
        private string ServerUrl { get; set; }
        private string ProxyServer { get; set; }
        private int ProxyPort { get; set; }
        private string ProxyUsername { get; set; }
        private string ProxyPassword { get; set; }
        private bool UseProxy { get; set; }
        private StreamProxy StreamProxy { get; set; }
        private FileLogger FileLogger { get; }
        private ChatMonitor<Image> ChatMonitor { get; set; }
        
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
                    Directory.CreateDirectory(appData);
                
                String logFile = Path.Combine(appData, "ultrasonic.log");

                FileLogger = new FileLogger(logFile, LoggingLevel.Verbose);
                FileLogger.Log("UltraSonic Started", LoggingLevel.Basic);
                
                WindowStartupLocation = WindowStartupLocation.Manual;

                FileLogger.Log($"WindowLeft: {Settings.Default.WindowLeft}", LoggingLevel.Verbose);
                Left = Settings.Default.WindowLeft;

                FileLogger.Log($"WindowTop: {Settings.Default.WindowTop}", LoggingLevel.Verbose);
                Top = Settings.Default.WindowTop;
                
                FileLogger.Log($"WindowHeight: {Settings.Default.WindowHeight}", LoggingLevel.Verbose);
                Height = Settings.Default.WindowHeight;

                FileLogger.Log($"WindowWidth: {Settings.Default.WindowWidth}", LoggingLevel.Verbose);
                Width = Settings.Default.WindowWidth;

                FileLogger.Log($"WindowMaximized: {Settings.Default.WindowMaximized}", LoggingLevel.Verbose);
                
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
                    FileLogger.Log("Creating StreamProxy", LoggingLevel.Information);

                    StreamProxy = StreamProxy.Instance;
                    StreamProxy.Start();

                    FileLogger.Log($"StreamProxy Port: {StreamProxy.GetPort()}", LoggingLevel.Information);
                }

                if (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(ServerUrl))
                {
                    InitSubsonicApi();

                    SubsonicClient?.GetLicenseAsync(GetCancellationToken("MainWindow")).ContinueWith(CheckLicense);
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

                ConfigureChat(_chatMessagesInterval);

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
                MessageBox.Show($"Exception:\n\n{ex.Message}\n{ex.StackTrace}", AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisableChat()
        {
            if (ChatMonitor == null) return;

            ChatMonitor.Unsubscribe();
            ChatMonitor = null;
        }

        private void ConfigureChat(int interval)
        {
            if (interval <= 0)
            {
                DisableChat();
                return;
            }

            if (ChatMonitor == null)
            {
                ChatMonitor = new ChatMonitor<Image>();

                ChatMonitor.Subscribe(new ChatHandler<Image>
                {
                    Client = SubsonicClient,
                    Interval = _chatMessagesInterval*1000,
                    CancellationToken = GetCancellationToken("ConfigureChat")
                });

                ChatMonitor.PropertyChanged += ((e, o) =>
                {
                    if (e.Disposed)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            CancelTasks("ConfigureChat");
                            _chatMessages.Clear();
                        });
                    }
                    else if (_chatMessages != null && o.ChatItem != null)
                    {
                        Dispatcher.Invoke(() => _chatMessages.Insert(0, o.ChatItem));
                    }
                });
            }
            else
            {
                ChatMonitor.ChatHandler.Interval = _chatMessagesInterval*1000;
            }
        }

        private void InitSubsonicApi()
        {
            Uri serverUri = new Uri(ServerUrl);
            string proxyUri = string.IsNullOrWhiteSpace(ProxyServer) ? null : ProxyServer;
            
            if (!ValidateCertificate(serverUri))
            {
                FileLogger.Log("Unable to validate server certificate, this issue must be corrected before continuing.", LoggingLevel.Error);
                MessageBox.Show("Unable to validate server certificate, this issue must be corrected before continuing.", AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                SubsonicClient = null;
            }
            else
            {
                SubsonicServer = UseProxy ? new SubsonicServer(serverUri, Username, Password, ClientName, proxyUri, ProxyPort, ProxyUsername, ProxyPassword) : new SubsonicServer(serverUri, Username, Password, ClientName);

                SubsonicClient = new SubsonicClient(SubsonicServer, new ImageFormatFactory());
                SubsonicClient.PingAsync(GetCancellationToken("InitSubsonicApi")).ContinueWith(ValidateServerVersion);
            }
        }

        private void ValidateServerVersion(Task<bool> task)
        {
            if (!task.Result)
            {
                FileLogger.Log("Error communicating with server", LoggingLevel.Error);
                MessageBox.Show("Error communicating with server", AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                SubsonicClient = null;
                return;
            }

            FileLogger.Log($"Subsonic Server API Version: {SubsonicServer.ApiVersion}", LoggingLevel.Information);

            if (SubsonicServer.ApiVersion < SubsonicApiVersions.Version1_8_0)
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
            else if (SubsonicServer.ApiVersion < SubsonicApiVersions.Version1_4_0)
            {
                FileLogger.Log($"{AppName} requires a Subsonic server with a REST API version of at least 1.4.0", LoggingLevel.Error);
                MessageBox.Show($"{AppName} requires a Subsonic server with a REST API version of at least 1.4.0", AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                SubsonicClient = null;
            }

            if (SubsonicClient == null)
                return;

            Dispatcher.Invoke(() =>
            {
                ServerApiLabel.Text = SubsonicServer.ApiVersion.ToString();
            });

            SubsonicClient.GetUserAsync(Username, GetCancellationToken("ValidateServerVersion")).ContinueWith(UpdateCurrentUser);
        }

        private static bool ValidateCertificate(Uri serverUri)
        {
            if (serverUri.Scheme == "https")
            {
                X509Certificate2 cert = GetSslCertificate(serverUri);

                if (cert == null) return false;

                if (!cert.Verify())
                {
                    MessageBoxResult result = MessageBox.Show($"Server certificate is not trusted, would you like to add it to trusted root certificate authorities (CAs)?\n\n\tName: {cert.Subject}\n\tIssuer: {cert.Issuer}\n\tSerial: {cert.GetSerialNumberString()}\n\tThumbprint: {cert.Thumbprint}\n\tExpiration: {cert.GetExpirationDateString()}", AppName, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No);

                    if (result != MessageBoxResult.Yes)
                        return cert.Verify();

                    X509Store store = new X509Store(StoreName.Root);
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(cert);
                    store.Close();
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
        
            MusicTimeRemainingLabel.Content = $"{TimeSpan.FromMilliseconds(MediaPlayer.Position.TotalMilliseconds):mm\\:ss} / {TimeSpan.FromMilliseconds(_position.TotalMilliseconds):mm\\:ss}";
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

        //private async void UpdateChatMessages()
        //{
        //    if (SubsonicClient == null) return;
        //    if (_working) return;

        //    _working = true;
        //    ProgressIndicator.Visibility = Visibility.Visible;
        //    await SubsonicClient.GetChatMessagesAsync(_chatMessageSince, GetCancellationToken("UpdateNowPlaying")).ContinueWith(UpdateChatMessages);
        //    ProgressIndicator.Visibility = Visibility.Hidden;
        //    _working = false;
        //}

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
            _cachingThrottle?.Dispose();

            FileLogger?.Dispose();
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

        private void DownloadCoverArt(UltraSonicAlbumItem albumItem)
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