using Subsonic.Client;
using Subsonic.Client.Interfaces;
using Subsonic.Client.UniversalWindows;
using Subsonic.Common.Interfaces;
using System;
using Windows.Storage;
using UltraSonic.Extensions;
using UltraSonic.Models;
using UltraSonic.ViewModels;
using Windows.UI.Xaml.Media.Imaging;

namespace UltraSonic
{
    public class SettingsHelper
    {
        private static SettingsModel _settingsModel;

        private static ISubsonicServer _subsonicServer;
        private static IImageFormatFactory<SoftwareBitmapSource> _imageFormatFactory;
        private static ISubsonicClient<SoftwareBitmapSource> _subsonicClient;

        private const int DefaultMaxAlbumResults = 25;
        private const int DefaultMaxBitrate = 0;
        private const int DefaultMaxSongResults = 100;

        private const string ServerSettingsKey = "serverSettings";
        private const string ProxySettingsKey = "proxySettings";
        private const string SearchSettingsKey = "searchSettings";
        private const string ServerUrlKey = "serverUrl";
        private const string UsernameKey = "username";
        private const string PasswordKey = "password";
        private const string UseProxyKey = "useProxy";
        private const string ProxyServerKey = "proxyServer";
        private const string ProxyPortKey = "proxyPort";
        private const string ProxyUsernameKey = "proxyUsername";
        private const string ProxyPasswordKey = "proxyPassword";
        private const string MaxBitrateKey = "maxBitrate";
        private const string MaxAlbumResultsKey = "maxAlbumResults";
        private const string MaxSongResultsKey = "maxSongResults";

        public static ISubsonicClient<SoftwareBitmapSource> GetSubsonicClient()
        {
            if (_settingsModel == null)
                GetSettings();

            if (string.IsNullOrWhiteSpace(GetServerUrl()))
                return null;

            if (_subsonicClient != null)
                return _subsonicClient;

            _subsonicServer = new SubsonicServer(new Uri(GetServerUrl()), GetUsername(), GetPassword(), "UltraSonic.Universal");
            _imageFormatFactory = new ImageFormatFactory();
            _subsonicClient = new SubsonicClient(_subsonicServer, _imageFormatFactory);

            return _subsonicClient;
        }
        
        public static SettingsModel GetSettings()
        {
            _settingsModel = new SettingsModel();

            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;

            var serverSettingsContainer = roamingSettings.Containers.ContainsKey(ServerSettingsKey) ? roamingSettings.Containers[ServerSettingsKey] : roamingSettings.CreateContainer(ServerSettingsKey, ApplicationDataCreateDisposition.Always);
            var proxySettingsContainer = roamingSettings.Containers.ContainsKey(ProxySettingsKey) ? roamingSettings.Containers[ProxySettingsKey] : roamingSettings.CreateContainer(ProxySettingsKey, ApplicationDataCreateDisposition.Always);
            var searchSettingsContainer = roamingSettings.Containers.ContainsKey(SearchSettingsKey) ? roamingSettings.Containers[SearchSettingsKey] : roamingSettings.CreateContainer(SearchSettingsKey, ApplicationDataCreateDisposition.Always);

            _settingsModel.ServerUrl = serverSettingsContainer.Values.TryGetProperty(ServerUrlKey, string.Empty);
            _settingsModel.Username = serverSettingsContainer.Values.TryGetProperty(UsernameKey, string.Empty);
            _settingsModel.Password = serverSettingsContainer.Values.TryGetProperty(PasswordKey, string.Empty);

            _settingsModel.UseProxy =  proxySettingsContainer.Values.TryGetProperty(UseProxyKey, false);
            _settingsModel.ProxyServer = proxySettingsContainer.Values.TryGetProperty(ProxyServerKey, string.Empty);
            _settingsModel.ProxyPort = proxySettingsContainer.Values.TryGetProperty(ProxyPortKey, 0);
            _settingsModel.ProxyUsername = proxySettingsContainer.Values.TryGetProperty(ProxyUsernameKey, string.Empty);
            _settingsModel.ProxyPassword = proxySettingsContainer.Values.TryGetProperty(ProxyPasswordKey, string.Empty);

            _settingsModel.MaxAlbumResults = searchSettingsContainer.Values.TryGetProperty(MaxAlbumResultsKey, DefaultMaxAlbumResults);
            _settingsModel.MaxBitrate = searchSettingsContainer.Values.TryGetProperty(MaxBitrateKey, DefaultMaxBitrate);
            _settingsModel.MaxSongResults = searchSettingsContainer.Values.TryGetProperty(MaxSongResultsKey, DefaultMaxSongResults);

            GetSubsonicClient();
                                   
            return _settingsModel;
        }

        public static string GetUsername()
        {
            if (_settingsModel == null)
                GetSettings();

            return _settingsModel?.Username;
        }

        public static string GetPassword()
        {
            if (_settingsModel == null)
                GetSettings();

            return _settingsModel?.Password;
        }

        public static string GetServerUrl()
        {
            if (_settingsModel == null)
                GetSettings();

            return _settingsModel?.ServerUrl;
        }

        public static int GetMaxAlbumResults()
        {
            if (_settingsModel == null)
                GetSettings();

            return _settingsModel.MaxAlbumResults;
        }

        public static void SaveSettings(SettingsViewModel settingsViewModel)
        {
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;

            var serverSettingsContainer = !roamingSettings.Containers.ContainsKey(ServerSettingsKey) ? roamingSettings.CreateContainer(ServerSettingsKey, ApplicationDataCreateDisposition.Always) : roamingSettings.Containers[ServerSettingsKey];
            var proxySettingsContainer = !roamingSettings.Containers.ContainsKey(ProxySettingsKey) ? roamingSettings.CreateContainer(ProxySettingsKey, ApplicationDataCreateDisposition.Always) : roamingSettings.Containers[ProxySettingsKey];
            var searchSettingsContainer = !roamingSettings.Containers.ContainsKey(SearchSettingsKey) ? roamingSettings.CreateContainer(SearchSettingsKey, ApplicationDataCreateDisposition.Always) : roamingSettings.Containers[SearchSettingsKey];

            serverSettingsContainer.Values[ServerUrlKey] = settingsViewModel.Settings.ServerUrl;
            serverSettingsContainer.Values[UsernameKey] = settingsViewModel.Settings.Username;
            serverSettingsContainer.Values[PasswordKey] = settingsViewModel.Settings.Password;

            proxySettingsContainer.Values[UseProxyKey] = settingsViewModel.Settings.UseProxy;
            proxySettingsContainer.Values[ProxyServerKey] = settingsViewModel.Settings.ProxyServer;
            proxySettingsContainer.Values[ProxyPortKey] = settingsViewModel.Settings.ProxyPort;
            proxySettingsContainer.Values[ProxyUsernameKey] = settingsViewModel.Settings.ProxyUsername;
            proxySettingsContainer.Values[ProxyPasswordKey] = settingsViewModel.Settings.ProxyPassword;

            searchSettingsContainer.Values[MaxBitrateKey] = settingsViewModel.Settings.MaxBitrate;
            searchSettingsContainer.Values[MaxAlbumResultsKey] = settingsViewModel.Settings.MaxAlbumResults;
            searchSettingsContainer.Values[MaxSongResultsKey] = settingsViewModel.Settings.MaxSongResults;

            GetSubsonicClient();
        }
    }
}
