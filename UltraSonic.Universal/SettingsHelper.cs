using Subsonic.Client;
using Subsonic.Client.Interfaces;
using Subsonic.Client.UniversalWindows;
using Subsonic.Common.Interfaces;
using System;
using UltraSonic.Extensions;
using UltraSonic.Models;
using UltraSonic.ViewModels;
using Windows.UI.Xaml.Media.Imaging;

namespace UltraSonic
{
    public class SettingsHelper
    {
        private static SettingsModel _settingsModel;

        private static ISubsonicServer SubsonicServer;
        private static IImageFormatFactory<SoftwareBitmapSource> ImageFormatFactory;
        private static ISubsonicClient<SoftwareBitmapSource> SubsonicClient;

        private const int defaultMaxAlbumResults = 25;
        private const int defaultMaxBitrate = 0;
        private const int defaultMaxSongResults = 100;

        private const string serverSettingsKey = "serverSettings";
        private const string proxySettingsKey = "proxySettings";
        private const string searchSettingsKey = "searchSettings";
        private const string serverUrlKey = "serverUrl";
        private const string usernameKey = "username";
        private const string passwordKey = "password";
        private const string useProxyKey = "useProxy";
        private const string proxyServerKey = "proxyServer";
        private const string proxyPortKey = "proxyPort";
        private const string proxyUsernameKey = "proxyUsername";
        private const string proxyPasswordKey = "proxyPassword";
        private const string maxBitrateKey = "maxBitrate";
        private const string maxAlbumResultsKey = "maxAlbumResults";
        private const string maxSongResultsKey = "maxSongResults";

        public static ISubsonicClient<SoftwareBitmapSource> GetSubsonicClient()
        {
            if (_settingsModel == null)
                GetSettings();

            if (string.IsNullOrWhiteSpace(GetServerUrl()))
                return null;

            if (SubsonicClient == null)
            {
                SubsonicServer = new SubsonicServer(new Uri(GetServerUrl()), GetUsername(), GetPassword(), "UltraSonic.Universal");
                ImageFormatFactory = new ImageFormatFactory();
                SubsonicClient = new SubsonicClient(SubsonicServer, ImageFormatFactory);
            }

            return SubsonicClient;
        }
        
        public static SettingsModel GetSettings()
        {
            _settingsModel = new SettingsModel();

            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

            Windows.Storage.ApplicationDataContainer serverSettingsContainer = null;
            Windows.Storage.ApplicationDataContainer proxySettingsContainer = null;
            Windows.Storage.ApplicationDataContainer searchSettingsContainer = null;

            if (roamingSettings.Containers.ContainsKey(serverSettingsKey))
            {
                serverSettingsContainer = roamingSettings.Containers[serverSettingsKey];
            }
            else
            {
                serverSettingsContainer = roamingSettings.CreateContainer(serverSettingsKey, Windows.Storage.ApplicationDataCreateDisposition.Always);

            }

            if (roamingSettings.Containers.ContainsKey(proxySettingsKey))
            {
                proxySettingsContainer = roamingSettings.Containers[proxySettingsKey];
            }
            else
            {
                proxySettingsContainer = roamingSettings.CreateContainer(proxySettingsKey, Windows.Storage.ApplicationDataCreateDisposition.Always);
            }

            if (roamingSettings.Containers.ContainsKey(searchSettingsKey))
            {
                searchSettingsContainer = roamingSettings.Containers[searchSettingsKey];
            }
            else
            {
                searchSettingsContainer = roamingSettings.CreateContainer(searchSettingsKey, Windows.Storage.ApplicationDataCreateDisposition.Always);
            }

            _settingsModel.ServerUrl = serverSettingsContainer.Values.TryGetProperty(serverUrlKey, string.Empty);
            _settingsModel.Username = serverSettingsContainer.Values.TryGetProperty(usernameKey, string.Empty);
            _settingsModel.Password = serverSettingsContainer.Values.TryGetProperty(passwordKey, string.Empty);

            _settingsModel.UseProxy =  proxySettingsContainer.Values.TryGetProperty(useProxyKey, false);
            _settingsModel.ProxyServer = proxySettingsContainer.Values.TryGetProperty(proxyServerKey, string.Empty);
            _settingsModel.ProxyPort = proxySettingsContainer.Values.TryGetProperty(proxyPortKey, 0);
            _settingsModel.ProxyUsername = proxySettingsContainer.Values.TryGetProperty(proxyUsernameKey, string.Empty);
            _settingsModel.ProxyPassword = proxySettingsContainer.Values.TryGetProperty(proxyPasswordKey, string.Empty);

            _settingsModel.MaxAlbumResults = searchSettingsContainer.Values.TryGetProperty(maxAlbumResultsKey, defaultMaxAlbumResults);
            _settingsModel.MaxBitrate = searchSettingsContainer.Values.TryGetProperty(maxBitrateKey, defaultMaxBitrate);
            _settingsModel.MaxSongResults = searchSettingsContainer.Values.TryGetProperty(maxSongResultsKey, defaultMaxSongResults);

            GetSubsonicClient();
                                   
            return _settingsModel;
        }

        public static string GetUsername()
        {
            if (_settingsModel == null)
                GetSettings();

            return _settingsModel.Username;
        }

        public static string GetPassword()
        {
            if (_settingsModel == null)
                GetSettings();

            return _settingsModel.Password;
        }

        public static string GetServerUrl()
        {
            if (_settingsModel == null)
                GetSettings();

            return _settingsModel.ServerUrl;
        }

        public static int GetMaxAlbumResults()
        {
            if (_settingsModel == null)
                GetSettings();

            return _settingsModel.MaxAlbumResults;
        }

        public static void SaveSettings(SettingsViewModel settingsViewModel)
        {
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

            Windows.Storage.ApplicationDataContainer serverSettingsContainer = null;
            Windows.Storage.ApplicationDataContainer proxySettingsContainer = null;
            Windows.Storage.ApplicationDataContainer searchSettingsContainer = null;

            if (!roamingSettings.Containers.ContainsKey(serverSettingsKey))
            {
                serverSettingsContainer = roamingSettings.CreateContainer(serverSettingsKey, Windows.Storage.ApplicationDataCreateDisposition.Always);
            }
            else
            {
                serverSettingsContainer = roamingSettings.Containers[serverSettingsKey];
            }

            if (!roamingSettings.Containers.ContainsKey(proxySettingsKey))
            {
                proxySettingsContainer = roamingSettings.CreateContainer(proxySettingsKey, Windows.Storage.ApplicationDataCreateDisposition.Always);
            }
            else
            {
                proxySettingsContainer = roamingSettings.Containers[proxySettingsKey];
            }

            if (!roamingSettings.Containers.ContainsKey(searchSettingsKey))
            {
                searchSettingsContainer = roamingSettings.CreateContainer(searchSettingsKey, Windows.Storage.ApplicationDataCreateDisposition.Always);
            }
            else
            {
                searchSettingsContainer = roamingSettings.Containers[searchSettingsKey];
            }

            serverSettingsContainer.Values[serverUrlKey] = settingsViewModel.Settings.ServerUrl;
            serverSettingsContainer.Values[usernameKey] = settingsViewModel.Settings.Username;
            serverSettingsContainer.Values[passwordKey] = settingsViewModel.Settings.Password;

            proxySettingsContainer.Values[useProxyKey] = settingsViewModel.Settings.UseProxy;
            proxySettingsContainer.Values[proxyServerKey] = settingsViewModel.Settings.ProxyServer;
            proxySettingsContainer.Values[proxyPortKey] = settingsViewModel.Settings.ProxyPort;
            proxySettingsContainer.Values[proxyUsernameKey] = settingsViewModel.Settings.ProxyUsername;
            proxySettingsContainer.Values[proxyPasswordKey] = settingsViewModel.Settings.ProxyPassword;

            searchSettingsContainer.Values[maxBitrateKey] = settingsViewModel.Settings.MaxBitrate;
            searchSettingsContainer.Values[maxAlbumResultsKey] = settingsViewModel.Settings.MaxAlbumResults;
            searchSettingsContainer.Values[maxSongResultsKey] = settingsViewModel.Settings.MaxSongResults;

            GetSubsonicClient();
        }
    }
}
