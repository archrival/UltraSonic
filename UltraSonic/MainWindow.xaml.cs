using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Subsonic.Rest.Api;
using UltraSonic.Properties;
using Image = System.Drawing.Image;

namespace UltraSonic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string AppName = "UltraSonic";

        private readonly ConcurrentQueue<KeyValuePair<Task<Image>, CancellationTokenSource>> _albumArtRequests =
            new ConcurrentQueue<KeyValuePair<Task<Image>, CancellationTokenSource>>();

        private readonly ObservableCollection<ArtistItem> _artistItems = new ObservableCollection<ArtistItem>();
        private readonly string _cacheDirectory;
        private readonly string _roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private readonly ConcurrentQueue<Uri> _streams = new ConcurrentQueue<Uri>();
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private string _artistFilter = string.Empty;
        private string _currentArtist = string.Empty;
        private string _currentTitle = string.Empty;
        private ObservableCollection<ArtistItem> _filteredArtistItems = new ObservableCollection<ArtistItem>();
        private TimeSpan _position;
        private bool _repeatPlaylist;
        private int _maxSearchResults = 1;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Height = Settings.Default.Height;
                Width = Settings.Default.Width;

                _cacheDirectory = Path.Combine(Path.Combine(_roamingPath, AppName), "Cache");

                Username = Settings.Default.Username;
                Password = Settings.Default.Password;
                ServerUrl = Settings.Default.ServerUrl;
                ProxyUsername = Settings.Default.ProxyUsername;
                ProxyPassword = Settings.Default.ProxyPassword;
                ProxyServer = Settings.Default.ProxyServer;
                ProxyPort = Settings.Default.ProxyPort;
                UseProxy = Settings.Default.UseProxy;
                _maxSearchResults = Settings.Default.MaxSearchResults;

                PopulateSearchResultItemComboBox();

                PreferencesUseProxyCheckbox.IsChecked = UseProxy;

                PreferencesUsernameTextBox.Text = Username;
                PreferencesPasswordPasswordBox.Password = Password;
                PreferencesServerAddressTextBox.Text = ServerUrl;
                PreferencesUseProxyCheckbox.IsChecked = UseProxy;
                PreferencesProxyServerAddressTextBox.Text = ProxyServer;
                PreferencesProxyServerPortTextBox.Text = ProxyPort.ToString();
                PreferencesProxyServerUsernameTextBox.Text = ProxyUsername;
                PreferencesProxyServerPasswordTextBox.Password = ProxyPassword;

                SetProxyEntryVisibility(UseProxy);

                MusicPlayStatusLabel.Content = "Stopped";

                MusicTreeView.DataContext = ArtistItems;

                if (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) &&
                    !string.IsNullOrWhiteSpace(ServerUrl))
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
                    }
                }

                _timer.Interval = TimeSpan.FromMilliseconds(1000);
                _timer.Tick += Ticktock;
                _timer.Start();
                MediaPlayer.MediaEnded += (o, args) => PlayNextTrack();
                MediaPlayer.LoadedBehavior = MediaState.Manual;
                MediaPlayer.Volume = Settings.Default.Volume;
                VolumeSlider.Value = MediaPlayer.Volume*10;
            }
            catch (Exception ex)
            {
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

        private void PopulateSearchResultItemComboBox()
        {
            List<int> ListData = new List<int> {1, 5, 10, 25, 50, 100, 250, 500, 1000};

            MaxSearchResultsComboBox.ItemsSource = ListData;
            MaxSearchResultsComboBox.SelectedItem = _maxSearchResults;
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

                        List<ArtistItem> artistItems = filteredArtistItems as List<ArtistItem> ??
                                                       filteredArtistItems.ToList();
                        if (artistItems.Any())
                        {
                            var newArtistItem = new ArtistItem();
                            var children = new ObservableCollection<ArtistItem>(artistItems);
                            artistItem.CopyTo(newArtistItem);
                            newArtistItem.Children = children;
                            _filteredArtistItems.Add(newArtistItem);
                        }
                    }

                    return _filteredArtistItems;
                }

                return _artistItems;
            }
        }

        private void InitSubsonicApi()
        {
            SubsonicApi = UseProxy ? new SubsonicApi(new Uri(ServerUrl), Username, Password, ProxyPassword, ProxyUsername, ProxyPort, ProxyServer) {UserAgent = AppName} : new SubsonicApi(new Uri(ServerUrl), Username, Password) {UserAgent = AppName};
        }

        private void Ticktock(object sender, EventArgs e)
        {
            ProgressSlider.Value = MediaPlayer.Position.TotalSeconds;
            MusicTimeRemainingLabel.Content = string.Format("{0:mm\\:ss} / {1:mm\\:ss}", TimeSpan.FromMilliseconds(MediaPlayer.Position.TotalMilliseconds), TimeSpan.FromMilliseconds(_position.TotalMilliseconds));
            UpdateTitle();
        }

        private void UpdateArtists()
        {
            SubsonicApi.GetIndexesAsync().ContinueWith(UpdateArtistsTreeView);
        }

        private void UpdatePlaylists()
        {
            SubsonicApi.GetPlaylistsAsync().ContinueWith(UpdatePlaylists);
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

        private void QueueTrack(Child child)
        {
            string fileName = Path.Combine(_cacheDirectory, child.Id);
            fileName = Path.ChangeExtension(fileName, child.Suffix);

            var fileNameUri = new Uri(fileName);

            var fi = new FileInfo(fileName);

            if (_streams != null)
            {
                if (_streams.All(s => s.OriginalString == fileName) && File.Exists(fileName) && fi.Length == child.Size)
                {
                    UpdateAlbumArt(child.Id);
                    QueueTrack(fileNameUri, child);
                }
                else
                {
                    _streams.Enqueue(new Uri(fileName));
                    UpdateAlbumArt(child.Id);
                    Task<long> streamTask = SubsonicApi.StreamAsync(child.Id, fileName);
                    streamTask.ContinueWith((t) => QueueTrack(streamTask, child));
                }
            }
        }

        private void QueueTrack(Uri uri, Child child)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      StopMusic();
                                      _currentArtist = child.Artist;
                                      _currentTitle = child.Title;
                                      MediaPlayer.Source = uri;
                                      ProgressSlider.Value = 0;
                                  });
        }

        private void PlayTrack(Child child)
        {
            UpdateAlbumArt(child.Id);
            QueueTrack(child);
            MediaPlayer.MediaOpened += MediaPlayerPlayQueuedTrack;
        }

        private void UpdateAlbumArt(string id)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      var tokenSource = new CancellationTokenSource();
                                      CancellationToken token = tokenSource.Token;

                                      KeyValuePair<Task<Image>, CancellationTokenSource> kvp;

                                      while (_albumArtRequests.TryDequeue(out kvp))
                                          kvp.Value.Cancel();

                                      Task<Image> albumArtTask = SubsonicApi.GetCoverArtAsync(id, null, token);
                                      _albumArtRequests.Enqueue(
                                          new KeyValuePair<Task<Image>, CancellationTokenSource>(albumArtTask,
                                                                                                 tokenSource));
                                      albumArtTask.ContinueWith(UpdateCoverArt);
                                  });
        }

        private void ExpandAll(ItemsControl items, bool expand)
        {
            foreach (object obj in items.Items)
            {
                var childControl = items.ItemContainerGenerator.ContainerFromItem(obj) as ItemsControl;

                if (childControl != null)
                    ExpandAll(childControl, expand);

                var item = childControl as TreeViewItem;

                if (item != null)
                    item.IsExpanded = expand;
            }
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

        private void UpdateAlbumGrid(IEnumerable<Child> children)
        {
            _albumItems = new ObservableCollection<AlbumItem>();

            Dispatcher.Invoke(() =>
            {
                foreach (Child child in children)
                {
                    AlbumItem albumItem = new AlbumItem { Name = child.Album, Album = child };
                    _albumItems.Add(albumItem);

                    Task<Image> coverArtTask = SubsonicApi.GetCoverArtAsync(child.Id);
                    coverArtTask.ContinueWith((t) => UpdateAlbumImageArt(coverArtTask, albumItem));

                    MusicDataGrid.ItemsSource = _albumItems;
                    MusicDataGrid.DataContext = _albumItems;
                }
            });
        }
    }
}