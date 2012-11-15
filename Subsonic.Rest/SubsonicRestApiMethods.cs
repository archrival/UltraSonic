using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Subsonic.Rest.Api.Enums;

namespace Subsonic.Rest.Api
{
    public partial class SubsonicApi
    {
        private readonly Version _version100 = Version.Parse("1.0.0");
        private readonly Version _version110 = Version.Parse("1.1.0");
        private readonly Version _version120 = Version.Parse("1.2.0");
        private readonly Version _version130 = Version.Parse("1.3.0");
        private readonly Version _version140 = Version.Parse("1.4.0");
        private readonly Version _version150 = Version.Parse("1.5.0");
        private readonly Version _version160 = Version.Parse("1.6.0");
        private readonly Version _version180 = Version.Parse("1.8.0");

        /// <summary>
        /// Get a boolean response from the Subsonic server for the given method.
        /// </summary>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <param name="cancelToken"> </param>
        /// <returns>bool</returns>
        private async Task<bool> GetResponseAsync(Methods method, Version methodApiVersion, ICollection parameters = null, CancellationToken? cancelToken = null)
        {
            bool success = false;

            if (ServerApiVersion != null && methodApiVersion > ServerApiVersion)
                throw new SubsonicInvalidApiException(string.Format(CultureInfo.CurrentCulture, "Method {0} requires Subsonic Server API version {1}, but the actual Subsonic Server API version is {2}.", Enum.GetName(typeof(Methods), method), methodApiVersion, ServerApiVersion));

            Response response = await RequestAsync(method, methodApiVersion, parameters, cancelToken);

            switch (response.Status)
            {
                case ResponseStatus.Ok:
                    success = true;
                    break;
                case ResponseStatus.Failed:
                    if (response.ItemElementName == ItemChoiceType.Error)
                        throw new SubsonicErrorException(string.Format(CultureInfo.CurrentCulture, "Error occurred in {0}", Enum.GetName(typeof(Methods), method)), response.Item as Error);

                    throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "Unknown error occurred in {0}", Enum.GetName(typeof(Methods), method)));
            }

            return success;
        }

        /// <summary>
        /// Get a boolean response from the Subsonic server for the given method.
        /// </summary>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <returns>bool</returns>
        private bool GetResponse(Methods method, Version methodApiVersion, ICollection parameters = null)
        {
            bool success = false;

            if (ServerApiVersion != null && methodApiVersion > ServerApiVersion)
                throw new SubsonicInvalidApiException(string.Format(CultureInfo.CurrentCulture, "Method {0} requires Subsonic Server API version {1}, but the actual Subsonic Server API version is {2}.", Enum.GetName(typeof (Methods), method), methodApiVersion, ServerApiVersion));

            Response response = Request(method, methodApiVersion, parameters);

            switch (response.Status)
            {
                case ResponseStatus.Ok:
                    success = true;
                    break;
                case ResponseStatus.Failed:
                    if (response.ItemElementName == ItemChoiceType.Error)
                        throw new SubsonicErrorException(string.Format(CultureInfo.CurrentCulture, "Error occurred in {0}", Enum.GetName(typeof (Methods), method)), response.Item as Error);

                    throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "Unknown error occurred in {0}", Enum.GetName(typeof (Methods), method)));
            }

            return success;
        }

        /// <summary>
        /// Get a response from the Subsonic server for the given method.
        /// </summary>
        /// <typeparam name="T">Object type the method will return.</typeparam>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <param name="cancelToken"> </param>
        /// <returns>T</returns>
        private async Task<T> GetResponseAsync<T>(Methods method, Version methodApiVersion, ICollection parameters = null, CancellationToken? cancelToken = null)
        {
            T result = default(T);

            if (ServerApiVersion != null && methodApiVersion > ServerApiVersion)
                throw new SubsonicInvalidApiException(string.Format(CultureInfo.CurrentCulture, "Method {0} requires Subsonic Server API version {1}, but the actual Subsonic Server API version is {2}.", Enum.GetName(typeof(Methods), method), methodApiVersion, ServerApiVersion));

            Response response = await RequestAsync(method, methodApiVersion, parameters, cancelToken);

            switch (response.Status)
            {
                case ResponseStatus.Ok:
                    result = (T)response.Item;
                    break;
                case ResponseStatus.Failed:
                    if (response.ItemElementName == ItemChoiceType.Error)
                        throw new SubsonicErrorException(string.Format(CultureInfo.CurrentCulture, "Error occurred in {0}", Enum.GetName(typeof(Methods), method)), response.Item as Error);

                    throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "Unknown error occurred in {0}", Enum.GetName(typeof(Methods), method)));
            }

            return result;
        }

        /// <summary>
        /// Get a response from the Subsonic server for the given method.
        /// </summary>
        /// <typeparam name="T">Object type the method will return.</typeparam>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <returns>T</returns>
        private T GetResponse<T>(Methods method, Version methodApiVersion, ICollection parameters = null)
        {
            T result = default(T);

            if (ServerApiVersion != null && methodApiVersion > ServerApiVersion)
                throw new SubsonicInvalidApiException(string.Format(CultureInfo.CurrentCulture, "Method {0} requires Subsonic Server API version {1}, but the actual Subsonic Server API version is {2}.", Enum.GetName(typeof (Methods), method), methodApiVersion, ServerApiVersion));

            Response response = Request(method, methodApiVersion, parameters);

            switch (response.Status)
            {
                case ResponseStatus.Ok:
                    result = (T) response.Item;
                    break;
                case ResponseStatus.Failed:
                    if (response.ItemElementName == ItemChoiceType.Error)
                        throw new SubsonicErrorException(string.Format(CultureInfo.CurrentCulture, "Error occurred in {0}", Enum.GetName(typeof (Methods), method)), response.Item as Error);

                    throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "Unknown error occurred in {0}", Enum.GetName(typeof (Methods), method)));
            }

            return result;
        }

        /// <summary>
        /// Get a response from the Subsonic server for the given method.
        /// </summary>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <param name="cancelToken"> </param>
        /// <returns>T</returns>
        private async Task<long> GetImageSizeAsync(Methods method, Version methodApiVersion, ICollection parameters = null, CancellationToken? cancelToken = null)
        {
            if (ServerApiVersion != null && methodApiVersion > ServerApiVersion)
                throw new SubsonicInvalidApiException(string.Format(CultureInfo.CurrentCulture, "Method {0} requires Subsonic Server API version {1}, but the actual Subsonic Server API version is {2}.", Enum.GetName(typeof(Methods), method), methodApiVersion, ServerApiVersion));

            return await ImageSizeRequestAsync(method, methodApiVersion, parameters, cancelToken);
        }

        /// <summary>
        /// Get a response from the Subsonic server for the given method.
        /// </summary>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <param name="cancelToken"> </param>
        /// <returns>T</returns>
        private async Task<Image> GetImageResponseAsync(Methods method, Version methodApiVersion, ICollection parameters = null, CancellationToken? cancelToken = null)
        {
            if (ServerApiVersion != null && methodApiVersion > ServerApiVersion)
                throw new SubsonicInvalidApiException(string.Format(CultureInfo.CurrentCulture, "Method {0} requires Subsonic Server API version {1}, but the actual Subsonic Server API version is {2}.", Enum.GetName(typeof(Methods), method), methodApiVersion, ServerApiVersion));

            return await ImageRequestAsync(method, methodApiVersion, parameters, cancelToken);
        }

        /// <summary>
        /// Get a response from the Subsonic server for the given method.
        /// </summary>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <returns>T</returns>
        private Image GetImageResponse(Methods method, Version methodApiVersion, ICollection parameters = null)
        {
            if (ServerApiVersion != null && methodApiVersion > ServerApiVersion)
                throw new SubsonicInvalidApiException(string.Format(CultureInfo.CurrentCulture, "Method {0} requires Subsonic Server API version {1}, but the actual Subsonic Server API version is {2}.", Enum.GetName(typeof (Methods), method), methodApiVersion, ServerApiVersion));

            return ImageRequest(method, methodApiVersion, parameters);
        }

        /// <summary>
        /// Used to test connectivity with the server.
        /// </summary>
        /// <returns>bool</returns>
        public async Task<bool> PingAsync()
        {
            return await GetResponseAsync(Methods.ping, _version100);
        }

        /// <summary>
        /// Used to test connectivity with the server.
        /// </summary>
        /// <returns>bool</returns>
        public bool Ping()
        {
            return GetResponse(Methods.ping, _version100);
        }

        /// <summary>
        /// Get details about the software license. Please note that access to the REST API requires that the server has a valid license (after a 30-day trial period). To get a license key you can give a donation to the Subsonic project.
        /// </summary>
        /// <returns>License</returns>
        public async Task<License> GetLicenseAsync(CancellationToken? cancelToken = null)
        {
            return await GetResponseAsync<License>(Methods.getLicense, _version100, null, cancelToken);
        }

        /// <summary>
        /// Get details about the software license. Please note that access to the REST API requires that the server has a valid license (after a 30-day trial period). To get a license key you can give a donation to the Subsonic project.
        /// </summary>
        /// <returns>License</returns>
        public License GetLicense()
        {
            return GetResponse<License>(Methods.getLicense, _version100);
        }

        /// <summary>
        /// Returns all configured top-level music folders.
        /// </summary>
        /// <returns>MusicFolders</returns>
        public async Task<MusicFolders> GetMusicFoldersAsync(CancellationToken? cancelToken = null)
        {
            return await GetResponseAsync<MusicFolders>(Methods.getMusicFolders, _version100, null, cancelToken);
        }

        /// <summary>
        /// Returns all configured top-level music folders.
        /// </summary>
        /// <returns>MusicFolders</returns>
        public MusicFolders GetMusicFolders()
        {
            return GetResponse<MusicFolders>(Methods.getMusicFolders, _version100);
        }

        /// <summary>
        /// Returns what is currently being played by all users.
        /// </summary>
        /// <returns>NowPlaying</returns>
        public async Task<NowPlaying> GetNowPlayingAsync(CancellationToken? cancelToken = null)
        {
            return await GetResponseAsync<NowPlaying>(Methods.getNowPlaying, _version100, null, cancelToken);
        }

        /// <summary>
        /// Returns what is currently being played by all users.
        /// </summary>
        /// <returns>NowPlaying</returns>
        public NowPlaying GetNowPlaying()
        {
            return GetResponse<NowPlaying>(Methods.getNowPlaying, _version100);
        }

        /// <summary>
        /// Returns starred songs, albums and artists.
        /// </summary>
        /// <returns>Starred</returns>
        public async Task<Starred> GetStarredAsync(CancellationToken? cancelToken = null)
        {
            return await GetResponseAsync<Starred>(Methods.getStarred, _version180, null, cancelToken);
        }

        /// <summary>
        /// Returns starred songs, albums and artists.
        /// </summary>
        /// <returns>Starred</returns>
        public Starred GetStarred()
        {
            return GetResponse<Starred>(Methods.getStarred, _version180);
        }

        /// <summary>
        /// Similar to getStarred, but organizes music according to ID3 tags.
        /// </summary>
        /// <returns>Starred2</returns>
        public async Task<Starred2> GetStarred2Async(CancellationToken? cancelToken = null)
        {
            return await GetResponseAsync<Starred2>(Methods.getStarred2, _version180, null, cancelToken);
        }

        /// <summary>
        /// Similar to getStarred, but organizes music according to ID3 tags.
        /// </summary>
        /// <returns>Starred2</returns>
        public Starred2 GetStarred2()
        {
            return GetResponse<Starred2>(Methods.getStarred2, _version180);
        }

        /// <summary>
        /// Returns an indexed structure of all artists.
        /// </summary>
        /// <param name="musicFolderId">If specified, only return artists in the music folder with the given ID.</param>
        /// <param name="ifModifiedSince">If specified, only return a result if the artist collection has changed since the given time.</param>
        /// <returns>Indexes</returns>
        public async Task<Indexes> GetIndexesAsync(string musicFolderId = null, long? ifModifiedSince = null, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable();

            if (!string.IsNullOrWhiteSpace(musicFolderId))
                parameters.Add("musicFolderId", musicFolderId);

            if (ifModifiedSince != null)
                parameters.Add("ifModifiedSince", ifModifiedSince);

            return await GetResponseAsync<Indexes>(Methods.getIndexes, _version100, parameters, cancelToken);
        }

        /// <summary>
        /// Returns an indexed structure of all artists.
        /// </summary>
        /// <param name="musicFolderId">If specified, only return artists in the music folder with the given ID.</param>
        /// <param name="ifModifiedSince">If specified, only return a result if the artist collection has changed since the given time.</param>
        /// <returns>Indexes</returns>
        public Indexes GetIndexes(string musicFolderId = null, long? ifModifiedSince = null)
        {
            Hashtable parameters = new Hashtable();

            if (!string.IsNullOrWhiteSpace(musicFolderId))
                parameters.Add("musicFolderId", musicFolderId);

            if (ifModifiedSince != null)
                parameters.Add("ifModifiedSince", ifModifiedSince);

            return GetResponse<Indexes>(Methods.getIndexes, _version100, parameters);
        }

        /// <summary>
        /// Returns a listing of all files in a music directory. Typically used to get list of albums for an artist, or list of songs for an album.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the music folder. Obtained by calls to GetIndexes or GetMusicDirectory.</param>
        /// <returns>Directory</returns>
        public async Task<Directory> GetMusicDirectoryAsync(string id, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable { { "id", id } };
            return await GetResponseAsync<Directory>(Methods.getMusicDirectory, _version100, parameters, cancelToken);
        }

        /// <summary>
        /// Returns a listing of all files in a music directory. Typically used to get list of albums for an artist, or list of songs for an album.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the music folder. Obtained by calls to GetIndexes or GetMusicDirectory.</param>
        /// <returns>Directory</returns>
        public Directory GetMusicDirectory(string id)
        {
            Hashtable parameters = new Hashtable {{"id", id}};
            return GetResponse<Directory>(Methods.getMusicDirectory, _version100, parameters);
        }

        /// <summary>
        /// Returns details for an artist, including a list of albums. This method organizes music according to ID3 tags.
        /// </summary>
        /// <param name="id">The artist ID.</param>
        /// <returns>ArtistID3</returns>
        public async Task<ArtistWithAlbumsID3> GetArtistAsync(string id, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable { { "id", id } };
            return await GetResponseAsync<ArtistWithAlbumsID3>(Methods.getArtist, _version180, parameters, cancelToken);
        }

        /// <summary>
        /// Returns details for an artist, including a list of albums. This method organizes music according to ID3 tags.
        /// </summary>
        /// <param name="id">The artist ID.</param>
        /// <returns>ArtistID3</returns>
        public ArtistWithAlbumsID3 GetArtist(string id)
        {
            Hashtable parameters = new Hashtable {{"id", id}};
            return GetResponse<ArtistWithAlbumsID3>(Methods.getArtist, _version180, parameters);
        }

        /// <summary>
        /// Similar to getIndexes, but organizes music according to ID3 tags.
        /// </summary>
        /// <returns>ArtistsID3</returns>
        public async Task<ArtistsID3> GetArtistsAsync(CancellationToken? cancelToken = null)
        {
            return await GetResponseAsync<ArtistsID3>(Methods.getArtists, _version180, null, cancelToken);
        }

        /// <summary>
        /// Similar to getIndexes, but organizes music according to ID3 tags.
        /// </summary>
        /// <returns>ArtistsID3</returns>
        public ArtistsID3 GetArtists()
        {
            return GetResponse<ArtistsID3>(Methods.getArtists, _version180);
        }

        /// <summary>
        /// Returns details for an album, including a list of songs. This method organizes music according to ID3 tags.
        /// </summary>
        /// <param name="id">The album ID.</param>
        /// <param name="cancelToken"> </param>
        /// <returns>AlbumID3</returns>
        public async Task<AlbumID3> GetAlbumAsync(string id, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable { { "id", id } };
            return await GetResponseAsync<AlbumID3>(Methods.getAlbum, _version180, parameters, cancelToken);
        }

        /// <summary>
        /// Returns details for an album, including a list of songs. This method organizes music according to ID3 tags.
        /// </summary>
        /// <param name="id">The album ID.</param>
        /// <returns>AlbumID3</returns>
        public AlbumID3 GetAlbum(string id)
        {
            Hashtable parameters = new Hashtable {{"id", id}};
            return GetResponse<AlbumID3>(Methods.getAlbum, _version180, parameters);
        }

        /// <summary>
        /// Returns details for a song.
        /// </summary>
        /// <param name="id">The song ID.</param>
        /// <returns>Song</returns>
        public async Task<Child> GetSongAsync(string id, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable { { "id", id } };
            return await GetResponseAsync<Child>(Methods.getSong, _version180, parameters, cancelToken);
        }

        /// <summary>
        /// Returns details for a song.
        /// </summary>
        /// <param name="id">The song ID.</param>
        /// <returns>Song</returns>
        public Child GetSong(string id)
        {
            Hashtable parameters = new Hashtable {{"id", id}};
            return GetResponse<Child>(Methods.getSong, _version180, parameters);
        }

        /// <summary>
        /// Returns all video files.
        /// </summary>
        /// <returns>Videos</returns>
        public async Task<Videos> GetVideosAsync(CancellationToken? cancelToken = null)
        {
            return await GetResponseAsync<Videos>(Methods.getVideos, _version180, null, cancelToken);
        }

        /// <summary>
        /// Returns all video files.
        /// </summary>
        /// <returns>Videos</returns>
        public Videos GetVideos()
        {
            return GetResponse<Videos>(Methods.getVideos, _version180);
        }

        /// <summary>
        /// Returns a listing of files matching the given search criteria. Supports paging through the result. Deprecated since 1.4.0, use Search2 instead.
        /// </summary>
        /// <param name="artist">Artist to search for.</param>
        /// <param name="album">Album to search for.</param>
        /// <param name="title">Song title to search for.</param>
        /// <param name="any">Searches all fields.</param>
        /// <param name="count">Maximum number of results to return. [Default = 20]</param>
        /// <param name="offset">Search result offset. Used for paging. [Default = 0]</param>
        /// <param name="newerThan">Only return matches that are newer this time. Given as milliseconds since Jan 1, 1970.</param>
        /// <returns>SearchResult</returns>
        public async Task<SearchResult> SearchAsync(string artist = null, string album = null, string title = null, string any = null, int? count = null, int? offset = null, long? newerThan = null)
        {
            Hashtable parameters = new Hashtable();

            if (!string.IsNullOrWhiteSpace(artist))
                parameters.Add("artist", artist);

            if (!string.IsNullOrWhiteSpace(album))
                parameters.Add("album", album);

            if (!string.IsNullOrWhiteSpace(title))
                parameters.Add("title", title);

            if (!string.IsNullOrWhiteSpace(any))
                parameters.Add("any", any);

            if (count != null)
                parameters.Add("count", count);

            if (offset != null)
                parameters.Add("offset", offset);

            if (newerThan != null)
                parameters.Add("newerThan", newerThan);

            return await GetResponseAsync<SearchResult>(Methods.search, _version100, parameters);
        }

        /// <summary>
        /// Returns a listing of files matching the given search criteria. Supports paging through the result. Deprecated since 1.4.0, use Search2 instead.
        /// </summary>
        /// <param name="artist">Artist to search for.</param>
        /// <param name="album">Album to search for.</param>
        /// <param name="title">Song title to search for.</param>
        /// <param name="any">Searches all fields.</param>
        /// <param name="count">Maximum number of results to return. [Default = 20]</param>
        /// <param name="offset">Search result offset. Used for paging. [Default = 0]</param>
        /// <param name="newerThan">Only return matches that are newer this time. Given as milliseconds since Jan 1, 1970.</param>
        /// <returns>SearchResult</returns>
        public SearchResult Search(string artist = null, string album = null, string title = null, string any = null, int? count = null, int? offset = null, long? newerThan = null)
        {
            Hashtable parameters = new Hashtable();

            if (!string.IsNullOrWhiteSpace(artist))
                parameters.Add("artist", artist);

            if (!string.IsNullOrWhiteSpace(album))
                parameters.Add("album", album);

            if (!string.IsNullOrWhiteSpace(title))
                parameters.Add("title", title);

            if (!string.IsNullOrWhiteSpace(any))
                parameters.Add("any", any);

            if (count != null)
                parameters.Add("count", count);

            if (offset != null)
                parameters.Add("offset", offset);

            if (newerThan != null)
                parameters.Add("newerThan", newerThan);

            return GetResponse<SearchResult>(Methods.search, _version100, parameters);
        }

        /// <summary>
        /// Returns albums, artists and songs matching the given search criteria. Supports paging through the result.
        /// </summary>
        /// <param name="query">Search query.</param>
        /// <param name="artistCount">Maximum number of artists to return. [Default = 20]</param>
        /// <param name="artistOffset">Search result offset for artists. Used for paging. [Default = 0]</param>
        /// <param name="albumCount">Maximum number of albums to return. [Default = 20]</param>
        /// <param name="albumOffset">Search result offset for albums. Used for paging. [Default = 0]</param>
        /// <param name="songCount">Maximum number of songs to return. [Default = 20]</param>
        /// <param name="songOffset">Search result offset for songs. Used for paging. [Default = 0]</param>
        /// <returns>SearchResult2</returns>
        public async Task<SearchResult2> Search2Async(string query, int? artistCount = null, int? artistOffset = null, int? albumCount = null, int? albumOffset = null, int? songCount = null, int? songOffset = null, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable { { "query", query } };

            if (artistCount != null)
                parameters.Add("artistCount", artistCount);

            if (artistOffset != null)
                parameters.Add("artistOffset", artistOffset);

            if (albumCount != null)
                parameters.Add("albumCount", albumCount);

            if (albumOffset != null)
                parameters.Add("albumOffset", albumOffset);

            if (songCount != null)
                parameters.Add("songCount", songCount);

            if (songOffset != null)
                parameters.Add("songOffset", songOffset);

            return await GetResponseAsync<SearchResult2>(Methods.search2, _version140, parameters, cancelToken);
        }

        /// <summary>
        /// Returns albums, artists and songs matching the given search criteria. Supports paging through the result.
        /// </summary>
        /// <param name="query">Search query.</param>
        /// <param name="artistCount">Maximum number of artists to return. [Default = 20]</param>
        /// <param name="artistOffset">Search result offset for artists. Used for paging. [Default = 0]</param>
        /// <param name="albumCount">Maximum number of albums to return. [Default = 20]</param>
        /// <param name="albumOffset">Search result offset for albums. Used for paging. [Default = 0]</param>
        /// <param name="songCount">Maximum number of songs to return. [Default = 20]</param>
        /// <param name="songOffset">Search result offset for songs. Used for paging. [Default = 0]</param>
        /// <returns>SearchResult2</returns>
        public SearchResult2 Search2(string query, int? artistCount = null, int? artistOffset = null, int? albumCount = null, int? albumOffset = null, int? songCount = null, int? songOffset = null)
        {
            Hashtable parameters = new Hashtable {{"query", query}};

            if (artistCount != null)
                parameters.Add("artistCount", artistCount);

            if (artistOffset != null)
                parameters.Add("artistOffset", artistOffset);

            if (albumCount != null)
                parameters.Add("albumCount", albumCount);

            if (albumOffset != null)
                parameters.Add("albumOffset", albumOffset);

            if (songCount != null)
                parameters.Add("songCount", songCount);

            if (songOffset != null)
                parameters.Add("songOffset", songOffset);

            return GetResponse<SearchResult2>(Methods.search2, _version140, parameters);
        }

        /// <summary>
        /// Similar to search2, but organizes music according to ID3 tags.
        /// </summary>
        /// <param name="query">Search query.</param>
        /// <param name="artistCount">Maximum number of artists to return. [Default = 20]</param>
        /// <param name="artistOffset">Search result offset for artists. Used for paging. [Default = 0]</param>
        /// <param name="albumCount">Maximum number of albums to return. [Default = 20]</param>
        /// <param name="albumOffset">Search result offset for albums. Used for paging. [Default = 0]</param>
        /// <param name="songCount">Maximum number of songs to return. [Default = 20]</param>
        /// <param name="songOffset">Search result offset for songs. Used for paging. [Default = 0]</param>
        /// <returns>SearchResult3</returns>
        public async Task<SearchResult3> Search3Async(string query, int? artistCount = null, int? artistOffset = null, int? albumCount = null, int? albumOffset = null, int? songCount = null, int? songOffset = null, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable { { "query", query } };

            if (artistCount != null)
                parameters.Add("artistCount", artistCount);

            if (artistOffset != null)
                parameters.Add("artistOffset", artistOffset);

            if (albumCount != null)
                parameters.Add("albumCount", albumCount);

            if (albumOffset != null)
                parameters.Add("albumOffset", albumOffset);

            if (songCount != null)
                parameters.Add("songCount", songCount);

            if (songOffset != null)
                parameters.Add("songOffset", songOffset);

            return await GetResponseAsync<SearchResult3>(Methods.search3, _version180, parameters, cancelToken);
        }

        /// <summary>
        /// Similar to search2, but organizes music according to ID3 tags.
        /// </summary>
        /// <param name="query">Search query.</param>
        /// <param name="artistCount">Maximum number of artists to return. [Default = 20]</param>
        /// <param name="artistOffset">Search result offset for artists. Used for paging. [Default = 0]</param>
        /// <param name="albumCount">Maximum number of albums to return. [Default = 20]</param>
        /// <param name="albumOffset">Search result offset for albums. Used for paging. [Default = 0]</param>
        /// <param name="songCount">Maximum number of songs to return. [Default = 20]</param>
        /// <param name="songOffset">Search result offset for songs. Used for paging. [Default = 0]</param>
        /// <returns>SearchResult3</returns>
        public SearchResult3 Search3(string query, int? artistCount = null, int? artistOffset = null, int? albumCount = null, int? albumOffset = null, int? songCount = null, int? songOffset = null)
        {
            Hashtable parameters = new Hashtable {{"query", query}};

            if (artistCount != null)
                parameters.Add("artistCount", artistCount);

            if (artistOffset != null)
                parameters.Add("artistOffset", artistOffset);

            if (albumCount != null)
                parameters.Add("albumCount", albumCount);

            if (albumOffset != null)
                parameters.Add("albumOffset", albumOffset);

            if (songCount != null)
                parameters.Add("songCount", songCount);

            if (songOffset != null)
                parameters.Add("songOffset", songOffset);

            return GetResponse<SearchResult3>(Methods.search3, _version180, parameters);
        }

        /// <summary>
        /// Returns the ID and name of all saved playlists.
        /// </summary>
        /// <param name="username">(Since 1.8.0) If specified, return playlists for this user rather than for the authenticated user. The authenticated user must have admin role if this parameter is used.</param>
        /// <returns>Playlists</returns>
        public async Task<Playlists> GetPlaylistsAsync(string username = null, CancellationToken? cancelToken = null)
        {
            Version methodApiVersion = _version100;

            Hashtable parameters = new Hashtable();

            if (username != null)
            {
                parameters.Add("username", username);
                methodApiVersion = _version180;
            }

            return await GetResponseAsync<Playlists>(Methods.getPlaylists, methodApiVersion, parameters, cancelToken);
        }

        /// <summary>
        /// Returns the ID and name of all saved playlists.
        /// </summary>
        /// <param name="username">(Since 1.8.0) If specified, return playlists for this user rather than for the authenticated user. The authenticated user must have admin role if this parameter is used.</param>
        /// <returns>Playlists</returns>
        public Playlists GetPlaylists(string username = null)
        {
            Version methodApiVersion = _version100;

            Hashtable parameters = new Hashtable();

            if (username != null)
            {
                parameters.Add("username", username);
                methodApiVersion = _version180;
            }

            return GetResponse<Playlists>(Methods.getPlaylists, methodApiVersion, parameters);
        }

        /// <summary>
        /// Returns a listing of files in a saved playlist.
        /// </summary>
        /// <param name="id">ID of the playlist to return, as obtained by GetPlaylists.</param>
        /// <returns>PlaylistWithSongs</returns>
        public async Task<PlaylistWithSongs> GetPlaylistAsync(string id, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable { { "id", id } };
            return await GetResponseAsync<PlaylistWithSongs>(Methods.getPlaylist, _version100, parameters, cancelToken);
        }

        /// <summary>
        /// Returns a listing of files in a saved playlist.
        /// </summary>
        /// <param name="id">ID of the playlist to return, as obtained by GetPlaylists.</param>
        /// <returns>Playlist</returns>
        public Playlist GetPlaylist(string id)
        {
            Hashtable parameters = new Hashtable {{"id", id}};
            return GetResponse<Playlist>(Methods.getPlaylist, _version100, parameters);
        }


        /// <summary>
        /// Creates or updates a saved playlist. Note: The user must be authorized to create playlists (see Settings > Users > User is allowed to create and delete playlists).
        /// </summary>
        /// <param name="playlistId">The playlist ID.</param>
        /// <param name="name">The human-readable name of the playlist.</param>
        /// <param name="songId">ID of a song in the playlist. Use one songId parameter for each song in the playlist.</param>
        /// <returns>bool</returns>
        public async Task<bool> CreatePlaylistAsync(string playlistId = null, string name = null, IEnumerable<string> songId = null)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrWhiteSpace(playlistId) && !string.IsNullOrWhiteSpace(name))
                throw new SubsonicApiException("Only one of playlist ID and name can be specified.");

            if (!string.IsNullOrWhiteSpace(playlistId))
                parameters.Add(new KeyValuePair<string, string>("playlistId", playlistId));
            else if (!string.IsNullOrWhiteSpace(name))
                parameters.Add(new KeyValuePair<string, string>("name", name));
            else
                throw new SubsonicApiException("One of playlist ID and name must be specified.");

            if (songId != null)
                parameters.AddRange(songId.Select(paramId => new KeyValuePair<string, string>("songId", paramId)));

            return await GetResponseAsync(Methods.createPlaylist, _version120, parameters);
        }

        /// <summary>
        /// Creates or updates a saved playlist. Note: The user must be authorized to create playlists (see Settings > Users > User is allowed to create and delete playlists).
        /// </summary>
        /// <param name="playlistId">The playlist ID.</param>
        /// <param name="name">The human-readable name of the playlist.</param>
        /// <param name="songId">ID of a song in the playlist. Use one songId parameter for each song in the playlist.</param>
        /// <returns>bool</returns>
        public bool CreatePlaylist(string playlistId = null, string name = null, IEnumerable<string> songId = null)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrWhiteSpace(playlistId) && !string.IsNullOrWhiteSpace(name))
                throw new SubsonicApiException("Only one of playlist ID and name can be specified.");

            if (!string.IsNullOrWhiteSpace(playlistId))
                parameters.Add(new KeyValuePair<string, string>("playlistId", playlistId));
            else if (!string.IsNullOrWhiteSpace(name))
                parameters.Add(new KeyValuePair<string, string>("name", name));
            else
                throw new SubsonicApiException("One of playlist ID and name must be specified.");

            if (songId != null)
                foreach (string paramId in songId)
                    parameters.Add(new KeyValuePair<string, string>("songId", paramId));

            return GetResponse(Methods.createPlaylist, _version120, parameters);
        }

        /// <summary>
        /// Updates a playlist. Only the owner of a playlist is allowed to update it.
        /// </summary>
        /// <param name="playlistId">The playlist ID.</param>
        /// <param name="name">The human-readable name of the playlist.</param>
        /// <param name="comment">The playlist comment.</param>
        /// <param name="songIdToAdd">Add this song with this ID to the playlist. Multiple parameters allowed.</param>
        /// <param name="songIndexToRemove">Remove the song at this position in the playlist. Multiple parameters allowed.</param>
        /// <returns>bool</returns>
        public async Task<bool> UpdatePlaylistAsync(string playlistId, string name = null, string comment = null, IEnumerable<string> songIdToAdd = null, IEnumerable<string> songIndexToRemove = null)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", playlistId)};

            if (!string.IsNullOrWhiteSpace(name))
                parameters.Add(new KeyValuePair<string, string>("name", name));

            if (!string.IsNullOrWhiteSpace(comment))
                parameters.Add(new KeyValuePair<string, string>("comment", comment));

            if (songIdToAdd != null)
                foreach (string songId in songIdToAdd)
                    parameters.Add(new KeyValuePair<string, string>("songIdToAdd", songId));

            if (songIndexToRemove != null)
                foreach (string songindex in songIndexToRemove)
                    parameters.Add(new KeyValuePair<string, string>("songIndexToRemove", songindex));

            return await GetResponseAsync(Methods.updatePlaylist, _version180, parameters);
        }

        /// <summary>
        /// Updates a playlist. Only the owner of a playlist is allowed to update it.
        /// </summary>
        /// <param name="playlistId">The playlist ID.</param>
        /// <param name="name">The human-readable name of the playlist.</param>
        /// <param name="comment">The playlist comment.</param>
        /// <param name="songIdToAdd">Add this song with this ID to the playlist. Multiple parameters allowed.</param>
        /// <param name="songIndexToRemove">Remove the song at this position in the playlist. Multiple parameters allowed.</param>
        /// <returns>bool</returns>
        public bool UpdatePlaylist(string playlistId, string name = null, string comment = null, IEnumerable<string> songIdToAdd = null, IEnumerable<string> songIndexToRemove = null)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("id", playlistId) };

            if (!string.IsNullOrWhiteSpace(name))
                parameters.Add(new KeyValuePair<string, string>("name", name));

            if (!string.IsNullOrWhiteSpace(comment))
                parameters.Add(new KeyValuePair<string, string>("comment", comment));

            if (songIdToAdd != null)
                foreach (string songId in songIdToAdd)
                    parameters.Add(new KeyValuePair<string, string>("songIdToAdd", songId));

            if (songIndexToRemove != null)
                foreach (string songindex in songIndexToRemove)
                    parameters.Add(new KeyValuePair<string, string>("songIndexToRemove", songindex));

            return GetResponse(Methods.updatePlaylist, _version180, parameters);
        }

        /// <summary>
        /// Deletes a saved playlist.
        /// </summary>
        /// <param name="id">ID of the playlist to delete, as obtained by GetPlaylists.</param>
        /// <returns>bool</returns>
        public async Task<bool> DeletePlaylistAsync(string id)
        {
            Hashtable parameters = new Hashtable { { "id", id } };
            return await GetResponseAsync(Methods.deletePlaylist, _version120, parameters);
        }

        /// <summary>
        /// Deletes a saved playlist.
        /// </summary>
        /// <param name="id">ID of the playlist to delete, as obtained by GetPlaylists.</param>
        /// <returns>bool</returns>
        public bool DeletePlaylist(string id)
        {
            Hashtable parameters = new Hashtable {{"id", id}};
            return GetResponse(Methods.deletePlaylist, _version120, parameters);
        }

        /// <summary>
        /// Downloads a given media file. Similar to stream, but this method returns the original media data without transcoding or downsampling.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the file to download. Obtained by calls to GetMusicDirectory.</param>
        /// <param name="path"> </param>
        /// <param name="pathOverride"> </param>
        /// <returns>long</returns>
        public async Task<long> DownloadAsync(string id, string path, bool pathOverride = false, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable { { "id", id } };
            return await RequestAsync(path, pathOverride, Methods.download, _version100, parameters, cancelToken);
        }

        /// <summary>
        /// Downloads a given media file. Similar to stream, but this method returns the original media data without transcoding or downsampling.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the file to download. Obtained by calls to GetMusicDirectory.</param>
        /// <param name="path"> </param>
        /// <param name="pathOverride"> </param>
        /// <returns>long</returns>
        public long Download(string id, string path, bool pathOverride = false)
        {
            Hashtable parameters = new Hashtable {{"id", id}};
            return Request(path, pathOverride, Methods.download, _version100, parameters);
        }

        /// <summary>
        /// Streams a given media file.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the file to stream. Obtained by calls to getMusicDirectory.</param>
        /// <param name="path"></param>
        /// <param name="maxBitRate">(Since 1.2.0) If specified, the server will attempt to limit the bitrate to this value, in kilobits per second. If set to zero, no limit is imposed.</param>
        /// <param name="format">(Since 1.6.0) Specifies the preferred target format (e.g., "mp3" or "flv") in case there are multiple applicable transcodings</param>
        /// <param name="timeOffset">Only applicable to video streaming. If specified, start streaming at the given offset (in seconds) into the video. Typically used to implement video skipping.</param>
        /// <param name="size">(Since 1.6.0) Only applicable to video streaming. Requested video size specified as WxH, for instance "640x480".</param>
        /// <param name="estimateContentLength">(Since 1.8.0). If set to "true", the Content-Length HTTP header will be set to an estimated value for transcoded or downsampled media.</param>
        /// <returns>long</returns>
        public async Task<long> StreamAsync(string id, string path, int? maxBitRate = null, StreamFormat? format = null, int? timeOffset = null, string size = null, bool? estimateContentLength = null, CancellationToken? cancelToken = null)
        {
            Version methodApiVersion = _version120;
            Hashtable parameters = new Hashtable { { "id", id } };

            if (maxBitRate != null && maxBitRate != 0)
                parameters.Add("maxBitRate", maxBitRate);

            if (format != null)
            {
                string streamFormatName = Enum.GetName(typeof(StreamFormat), format);
                if (streamFormatName != null) parameters.Add("streamFormat", streamFormatName);
                methodApiVersion = _version160;
            }

            if (timeOffset != null)
            {
                parameters.Add("timeOffset", timeOffset);
                methodApiVersion = _version160;
            }

            if (!string.IsNullOrWhiteSpace(size))
            {
                parameters.Add("size", size);
                methodApiVersion = _version160;
            }

            if (estimateContentLength != null)
            {
                parameters.Add("estimateContentLength", estimateContentLength);
                methodApiVersion = _version180;
            }

            return await RequestAsync(path, true, Methods.stream, methodApiVersion, parameters, cancelToken);
        }

        /// <summary>
        /// Streams a given media file.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the file to stream. Obtained by calls to getMusicDirectory.</param>
        /// <param name="path"></param>
        /// <param name="maxBitRate">(Since 1.2.0) If specified, the server will attempt to limit the bitrate to this value, in kilobits per second. If set to zero, no limit is imposed.</param>
        /// <param name="format">(Since 1.6.0) Specifies the preferred target format (e.g., "mp3" or "flv") in case there are multiple applicable transcodings</param>
        /// <param name="timeOffset">Only applicable to video streaming. If specified, start streaming at the given offset (in seconds) into the video. Typically used to implement video skipping.</param>
        /// <param name="size">(Since 1.6.0) Only applicable to video streaming. Requested video size specified as WxH, for instance "640x480".</param>
        /// <param name="estimateContentLength">(Since 1.8.0). If set to "true", the Content-Length HTTP header will be set to an estimated value for transcoded or downsampled media.</param>
        /// <returns>long</returns>
        public long Stream(string id, string path, int? maxBitRate = null, StreamFormat? format = null, int? timeOffset = null, string size = null, bool? estimateContentLength = null)
        {
            Version methodApiVersion = _version120;
            Hashtable parameters = new Hashtable {{"id", id}};

            if (maxBitRate != null)
                parameters.Add("maxBitRate", maxBitRate);

            if (format != null)
            {
                string streamFormatName = Enum.GetName(typeof (StreamFormat), format);
                if (streamFormatName != null) parameters.Add("streamFormat", streamFormatName);
                methodApiVersion = _version160;
            }

            if (timeOffset != null)
            {
                parameters.Add("timeOffset", timeOffset);
                methodApiVersion = _version160;
            }

            if (!string.IsNullOrWhiteSpace(size))
            {
                parameters.Add("size", size);
                methodApiVersion = _version160;
            }

            if (estimateContentLength != null)
            {
                parameters.Add("estimateContentLength", estimateContentLength);
                methodApiVersion = _version180;
            }

            return Request(path, true, Methods.stream, methodApiVersion, parameters);
        }

        /// <summary>
        /// Downloads a given media file. Similar to stream, but this method returns the original media data without transcoding or downsampling.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the file to download. Obtained by calls to GetMusicDirectory.</param>W
        /// <returns>long</returns>
        public string BuildDownloadUrl(string id)
        {
            Hashtable parameters = new Hashtable { { "id", id } };
            return BuildRequestUriUser(Methods.download, _version100, parameters);
        }

        /// <summary>
        /// Streams a given media file.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the file to stream. Obtained by calls to getMusicDirectory.</param>
        /// <param name="maxBitRate">(Since 1.2.0) If specified, the server will attempt to limit the bitrate to this value, in kilobits per second. If set to zero, no limit is imposed.</param>
        /// <param name="format">(Since 1.6.0) Specifies the preferred target format (e.g., "mp3" or "flv") in case there are multiple applicable transcodings</param>
        /// <param name="timeOffset">Only applicable to video streaming. If specified, start streaming at the given offset (in seconds) into the video. Typically used to implement video skipping.</param>
        /// <param name="size">(Since 1.6.0) Only applicable to video streaming. Requested video size specified as WxH, for instance "640x480".</param>
        /// <param name="estimateContentLength">(Since 1.8.0). If set to "true", the Content-Length HTTP header will be set to an estimated value for transcoded or downsampled media.</param>
        /// <returns>long</returns>
        public string BuildStreamUrl(string id, int? maxBitRate = null, StreamFormat? format = null, int? timeOffset = null, string size = null, bool? estimateContentLength = null)
        {
            Version methodApiVersion = _version120;
            Hashtable parameters = new Hashtable { { "id", id } };

            if (maxBitRate != null)
                parameters.Add("maxBitRate", maxBitRate);

            if (format != null)
            {
                string streamFormatName = Enum.GetName(typeof(StreamFormat), format);
                if (streamFormatName != null) parameters.Add("streamFormat", streamFormatName);
                methodApiVersion = _version160;
            }

            if (timeOffset != null)
            {
                parameters.Add("timeOffset", timeOffset);
                methodApiVersion = _version160;
            }

            if (!string.IsNullOrWhiteSpace(size))
            {
                parameters.Add("size", size);
                methodApiVersion = _version160;
            }

            if (estimateContentLength != null)
            {
                parameters.Add("estimateContentLength", estimateContentLength);
                methodApiVersion = _version180;
            }

            return BuildRequestUriUser(Methods.stream, methodApiVersion, parameters);
        }

        /// <summary>
        /// Returns a cover art image.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the cover art file to download. Obtained by calls to getMusicDirectory.</param>
        /// <param name="size">If specified, scale image to this size.</param>
        /// <param name="cancelToken"> </param>
        /// <returns>bool</returns>
        public async Task<long> GetCoverArtSizeAsync(string id, int? size = null, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable { { "id", id } };

            if (size != null)
                parameters.Add("size", size);

            return await GetImageSizeAsync(Methods.getCoverArt, _version100, parameters, cancelToken);
        }

        /// <summary>
        /// Returns a cover art image.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the cover art file to download. Obtained by calls to getMusicDirectory.</param>
        /// <param name="size">If specified, scale image to this size.</param>
        /// <param name="cancelToken"> </param>
        /// <returns>bool</returns>
        public async Task<Image> GetCoverArtAsync(string id, int? size = null, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable { { "id", id } };

            if (size != null)
                parameters.Add("size", size);

            return await GetImageResponseAsync(Methods.getCoverArt, _version100, parameters, cancelToken);
        }

        /// <summary>
        /// Returns a cover art image.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the cover art file to download. Obtained by calls to getMusicDirectory.</param>
        /// <param name="size">If specified, scale image to this size.</param>
        /// <returns>bool</returns>
        public Image GetCoverArt(string id, int? size = null)
        {
            Hashtable parameters = new Hashtable {{"id", id}};

            if (size != null)
                parameters.Add("size", size);

            return GetImageResponse(Methods.getCoverArt, _version100, parameters);
        }

        /// <summary>
        /// "Scrobbles" a given music file on last.fm. Requires that the user has configured his/her last.fm credentials on the Subsonic server (Settings > Personal).
        /// </summary>
        /// <param name="id">A string which uniquely identifies the file to scrobble.</param>
        /// <param name="submission">Whether this is a "submission" or a "now playing" notification. [Default = true]</param>
        /// <param name="time">(Since 1.8.0) The time (in milliseconds since 1 Jan 1970) at which the song was listened to.</param>
        /// <returns>bool</returns>
        public async Task<bool> ScrobbleAsync(string id, bool? submission = null, long? time = null)
        {
            Version methodApiVersion = _version150;
            Hashtable parameters = new Hashtable { { "id", id } };

            if (submission != null)
                parameters.Add("submission", submission);

            if (time != null)
            {
                parameters.Add("time", time);
                methodApiVersion = _version180;
            }

            return await GetResponseAsync(Methods.scrobble, methodApiVersion, parameters);
        }

        /// <summary>
        /// "Scrobbles" a given music file on last.fm. Requires that the user has configured his/her last.fm credentials on the Subsonic server (Settings > Personal).
        /// </summary>
        /// <param name="id">A string which uniquely identifies the file to scrobble.</param>
        /// <param name="submission">Whether this is a "submission" or a "now playing" notification. [Default = true]</param>
        /// <param name="time">(Since 1.8.0) The time (in milliseconds since 1 Jan 1970) at which the song was listened to.</param>
        /// <returns>bool</returns>
        public bool Scrobble(string id, bool? submission = null, long? time = null)
        {
            Version methodApiVersion = _version150;
            Hashtable parameters = new Hashtable {{"id", id}};

            if (submission != null)
                parameters.Add("submission", submission);

            if (time != null)
            {
                parameters.Add("time", time);
                methodApiVersion = _version180;
            }

            return GetResponse(Methods.scrobble, methodApiVersion, parameters);
        }

        /// <summary>
        /// Returns information about shared media this user is allowed to manage.
        /// </summary>
        /// <returns>Shares</returns>
        public async Task<Shares> GetSharesAsync(CancellationToken? cancelToken = null)
        {
            return await GetResponseAsync<Shares>(Methods.getShares, _version160, null, cancelToken);
        }

        /// <summary>
        /// Returns information about shared media this user is allowed to manage.
        /// </summary>
        /// <returns>Shares</returns>
        public Shares GetShares()
        {
            return GetResponse<Shares>(Methods.getShares, _version160);
        }

        /// <summary>
        /// Changes the password of an existing Subsonic user, using the following parameters. You can only change your own password unless you have admin privileges.
        /// </summary>
        /// <param name="username">The name of the user which should change its password.</param>
        /// <param name="password">The new password for the user.</param>
        /// <returns>bool</returns>
        public async Task<bool> ChangePasswordAsync(string username, string password)
        {
            Hashtable parameters = new Hashtable { { "username", username } };

            if (EncodePasswords)
                password = string.Format(CultureInfo.InvariantCulture, "enc:{0}", Strings.AsciiToHex(password));

            parameters.Add("password", password);

            return await GetResponseAsync(Methods.changePassword, _version110, parameters);
        }

        /// <summary>
        /// Changes the password of an existing Subsonic user, using the following parameters. You can only change your own password unless you have admin privileges.
        /// </summary>
        /// <param name="username">The name of the user which should change its password.</param>
        /// <param name="password">The new password for the user.</param>
        /// <returns>bool</returns>
        public bool ChangePassword(string username, string password)
        {
            Hashtable parameters = new Hashtable {{"username", username}};

            if (EncodePasswords)
                password = string.Format(CultureInfo.InvariantCulture, "enc:{0}", Strings.AsciiToHex(password));

            parameters.Add("password", password);

            return GetResponse(Methods.changePassword, _version110, parameters);
        }

        /// <summary>
        /// Get details about a given user, including which authorization roles it has. Can be used to enable/disable certain features in the client, such as jukebox control.
        /// </summary>
        /// <param name="username">The name of the user to retrieve. You can only retrieve your own user unless you have admin privileges.</param>
        /// <returns>User</returns>
        public async Task<User> GetUserAsync(string username, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable { { "username", username } };
            return await GetResponseAsync<User>(Methods.getUser, _version130, parameters, cancelToken);
        }

        /// <summary>
        /// Get details about a given user, including which authorization roles it has. Can be used to enable/disable certain features in the client, such as jukebox control.
        /// </summary>
        /// <param name="username">The name of the user to retrieve. You can only retrieve your own user unless you have admin privileges.</param>
        /// <returns>User</returns>
        public User GetUser(string username)
        {
            Hashtable parameters = new Hashtable {{"username", username}};
            return GetResponse<User>(Methods.getUser, _version130, parameters);
        }

        /// <summary>
        /// Returns the avatar (personal image) for a user.
        /// </summary>
        /// <param name="username">The user in question.</param>
        /// <param name="cancelToken"> </param>
        /// <returns>Image</returns>
        public async Task<Image> GetAvatarAsync(string username, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable { { "username", username } };
            return await GetImageResponseAsync(Methods.getAvatar, _version180, parameters, cancelToken);
        }

        /// <summary>
        /// Returns the avatar (personal image) for a user.
        /// </summary>
        /// <param name="username">The user in question.</param>
        /// <returns>Image</returns>
        public Image GetAvatar(string username)
        {
            Hashtable parameters = new Hashtable {{"username", username}};
            return GetImageResponse(Methods.getAvatar, _version180, parameters);
        }

        /// <summary>
        /// Attaches a star to a song, album or artist.
        /// </summary>
        /// <param name="id">The ID of the file (song) or folder (album/artist) to star. Multiple parameters allowed.</param>
        /// <param name="albumId">The ID of an album to star. Use this rather than id if the client accesses the media collection according to ID3 tags rather than file structure. Multiple parameters allowed.</param>
        /// <param name="artistId">The ID of an artist to star. Use this rather than id if the client accesses the media collection according to ID3 tags rather than file structure. Multiple parameters allowed.</param>
        /// <returns>bool</returns>
        public async Task<bool> StarAsync(IEnumerable<string> id = null, IEnumerable<string> albumId = null, IEnumerable<string> artistId = null)
        {
            if (id == null && albumId == null && artistId == null)
                throw new SubsonicApiException("You must provide one of id, albumId or artistId");

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            if (id != null)
                foreach (string ids in id)
                    parameters.Add(new KeyValuePair<string, string>("id", ids));

            if (albumId != null)
                foreach (string albumids in albumId)
                    parameters.Add(new KeyValuePair<string, string>("albumId", albumids));

            if (artistId != null)
                foreach (string artistIds in artistId)
                    parameters.Add(new KeyValuePair<string, string>("artistId", artistIds));

            return await GetResponseAsync(Methods.star, _version180, parameters);
        }

        /// <summary>
        /// Attaches a star to a song, album or artist.
        /// </summary>
        /// <param name="id">The ID of the file (song) or folder (album/artist) to star. Multiple parameters allowed.</param>
        /// <param name="albumId">The ID of an album to star. Use this rather than id if the client accesses the media collection according to ID3 tags rather than file structure. Multiple parameters allowed.</param>
        /// <param name="artistId">The ID of an artist to star. Use this rather than id if the client accesses the media collection according to ID3 tags rather than file structure. Multiple parameters allowed.</param>
        /// <returns>bool</returns>
        public bool Star(IEnumerable<string> id = null, IEnumerable<string> albumId = null, IEnumerable<string> artistId = null)
        {
            if (id == null && albumId == null && artistId == null)
                throw new SubsonicApiException("You must provide one of id, albumId or artistId");

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            if (id != null)
                foreach (string ids in id)
                    parameters.Add(new KeyValuePair<string, string>("id", ids));

            if (albumId != null)
                foreach (string albumids in albumId)
                    parameters.Add(new KeyValuePair<string, string>("albumId", albumids));

            if (artistId != null)
                foreach (string artistIds in artistId)
                    parameters.Add(new KeyValuePair<string, string>("artistId", artistIds));

            return GetResponse(Methods.star, _version180, parameters);
        }

        /// <summary>
        /// Removes the star from a song, album or artist.
        /// </summary>
        /// <param name="id">The ID of the file (song) or folder (album/artist) to unstar. Multiple parameters allowed.</param>
        /// <param name="albumId">The ID of an album to unstar. Use this rather than id if the client accesses the media collection according to ID3 tags rather than file structure. Multiple parameters allowed.</param>
        /// <param name="artistId">The ID of an artist to unstar. Use this rather than id if the client accesses the media collection according to ID3 tags rather than file structure. Multiple parameters allowed.</param>
        /// <returns>bool</returns>
        public async Task<bool> UnStarAsync(IEnumerable<string> id = null, IEnumerable<string> albumId = null, IEnumerable<string> artistId = null)
        {
            if (id == null && albumId == null && artistId == null)
                throw new SubsonicApiException("You must provide one of id, albumId or artistId");

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            if (id != null)
                foreach (string ids in id)
                    parameters.Add(new KeyValuePair<string, string>("id", ids));

            if (albumId != null)
                foreach (string albumids in albumId)
                    parameters.Add(new KeyValuePair<string, string>("albumId", albumids));

            if (artistId != null)
                foreach (string artistIds in artistId)
                    parameters.Add(new KeyValuePair<string, string>("artistId", artistIds));

            return await GetResponseAsync(Methods.unstar, _version180, parameters);
        }

        /// <summary>
        /// Removes the star from a song, album or artist.
        /// </summary>
        /// <param name="id">The ID of the file (song) or folder (album/artist) to unstar. Multiple parameters allowed.</param>
        /// <param name="albumId">The ID of an album to unstar. Use this rather than id if the client accesses the media collection according to ID3 tags rather than file structure. Multiple parameters allowed.</param>
        /// <param name="artistId">The ID of an artist to unstar. Use this rather than id if the client accesses the media collection according to ID3 tags rather than file structure. Multiple parameters allowed.</param>
        /// <returns>bool</returns>
        public bool UnStar(IEnumerable<string> id = null, IEnumerable<string> albumId = null, IEnumerable<string> artistId = null)
        {
            if (id == null && albumId == null && artistId == null)
                throw new SubsonicApiException("You must provide one of id, albumId or artistId");

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            if (id != null)
                foreach (string ids in id)
                    parameters.Add(new KeyValuePair<string, string>("id", ids));

            if (albumId != null)
                foreach (string albumids in albumId)
                    parameters.Add(new KeyValuePair<string, string>("albumId", albumids));

            if (artistId != null)
                foreach (string artistIds in artistId)
                    parameters.Add(new KeyValuePair<string, string>("artistId", artistIds));

            return GetResponse(Methods.unstar, _version180, parameters);
        }

        /// <summary>
        /// Sets the rating for a music file.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the file (song) or folder (album/artist) to rate.</param>
        /// <param name="rating">The rating between 1 and 5 (inclusive), or 0 to remove the rating.</param>
        /// <returns>bool</returns>
        public async Task<bool> SetRatingAsync(string id, int rating)
        {
            Hashtable parameters = new Hashtable { { "id", id }, { "rating", rating } };
            return await GetResponseAsync(Methods.setRating, _version160, parameters);
        }

        /// <summary>
        /// Sets the rating for a music file.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the file (song) or folder (album/artist) to rate.</param>
        /// <param name="rating">The rating between 1 and 5 (inclusive), or 0 to remove the rating.</param>
        /// <returns>bool</returns>
        public bool SetRating(string id, int rating)
        {
            Hashtable parameters = new Hashtable {{"id", id}, {"rating", rating}};
            return GetResponse(Methods.setRating, _version160, parameters);
        }

        /// <summary>
        /// Creates a new Subsonic user.
        /// </summary>
        /// <param name="username">The name of the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="ldapAuthenticated">Whether the user is authenicated in LDAP. [Default = false]</param>
        /// <param name="adminRole">Whether the user is administrator. [Default = false]</param>
        /// <param name="settingsRole">Whether the user is allowed to change settings and password. [Default = true]</param>
        /// <param name="streamRole">Whether the user is allowed to play files. [Default = true]</param>
        /// <param name="jukeboxRole">Whether the user is allowed to play files in jukebox mode. [Default = false]</param>
        /// <param name="downloadRole">Whether the user is allowed to download files. [Default = false]</param>
        /// <param name="uploadRole">Whether the user is allowed to upload files. [Default = false]</param>
        /// <param name="playlistRole">Whether the user is allowed to create and delete playlists. [Default = false]</param>
        /// <param name="coverArtRole">Whether the user is allowed to change cover art and tags. [Default = false]</param>
        /// <param name="commentRole">Whether the user is allowed to create and edit comments and ratings. [Default = false]</param>
        /// <param name="podcastRole">Whether the user is allowed to administrate Podcasts. [Default = false]</param>
        /// <returns>bool</returns>
        public async Task<bool> CreateUserAsync(string username, string password, bool? ldapAuthenticated = null, bool? adminRole = null, bool? settingsRole = null, bool? streamRole = null, bool? jukeboxRole = null, bool? downloadRole = null, bool? uploadRole = null, bool? playlistRole = null, bool? coverArtRole = null, bool? commentRole = null, bool? podcastRole = null)
        {
            Hashtable parameters = new Hashtable { { "username", username } };

            if (EncodePasswords)
                password = string.Format(CultureInfo.InvariantCulture, "enc:{0}", Strings.AsciiToHex(password));

            parameters.Add("password", password);

            if (ldapAuthenticated != null)
                parameters.Add("ldapAuthenticated", ldapAuthenticated);

            if (adminRole != null)
                parameters.Add("adminRole", adminRole);

            if (settingsRole != null)
                parameters.Add("settingsRole", settingsRole);

            if (streamRole != null)
                parameters.Add("streamRole", streamRole);

            if (jukeboxRole != null)
                parameters.Add("jukeboxRole", jukeboxRole);

            if (downloadRole != null)
                parameters.Add("downloadRole", downloadRole);

            if (uploadRole != null)
                parameters.Add("uploadRole", uploadRole);

            if (playlistRole != null)
                parameters.Add("playlistRole", playlistRole);

            if (coverArtRole != null)
                parameters.Add("coverArtRole", coverArtRole);

            if (commentRole != null)
                parameters.Add("commentRole", commentRole);

            if (podcastRole != null)
                parameters.Add("podcastRole", podcastRole);

            return await GetResponseAsync(Methods.createUser, _version130, parameters);
        }

        /// <summary>
        /// Creates a new Subsonic user.
        /// </summary>
        /// <param name="username">The name of the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="ldapAuthenticated">Whether the user is authenicated in LDAP. [Default = false]</param>
        /// <param name="adminRole">Whether the user is administrator. [Default = false]</param>
        /// <param name="settingsRole">Whether the user is allowed to change settings and password. [Default = true]</param>
        /// <param name="streamRole">Whether the user is allowed to play files. [Default = true]</param>
        /// <param name="jukeboxRole">Whether the user is allowed to play files in jukebox mode. [Default = false]</param>
        /// <param name="downloadRole">Whether the user is allowed to download files. [Default = false]</param>
        /// <param name="uploadRole">Whether the user is allowed to upload files. [Default = false]</param>
        /// <param name="playlistRole">Whether the user is allowed to create and delete playlists. [Default = false]</param>
        /// <param name="coverArtRole">Whether the user is allowed to change cover art and tags. [Default = false]</param>
        /// <param name="commentRole">Whether the user is allowed to create and edit comments and ratings. [Default = false]</param>
        /// <param name="podcastRole">Whether the user is allowed to administrate Podcasts. [Default = false]</param>
        /// <returns>bool</returns>
        public bool CreateUser(string username, string password, bool? ldapAuthenticated = null, bool? adminRole = null, bool? settingsRole = null, bool? streamRole = null, bool? jukeboxRole = null, bool? downloadRole = null, bool? uploadRole = null, bool? playlistRole = null, bool? coverArtRole = null, bool? commentRole = null, bool? podcastRole = null)
        {
            Hashtable parameters = new Hashtable {{"username", username}};

            if (EncodePasswords)
                password = string.Format(CultureInfo.InvariantCulture, "enc:{0}", Strings.AsciiToHex(password));

            parameters.Add("password", password);

            if (ldapAuthenticated != null)
                parameters.Add("ldapAuthenticated", ldapAuthenticated);

            if (adminRole != null)
                parameters.Add("adminRole", adminRole);

            if (settingsRole != null)
                parameters.Add("settingsRole", settingsRole);

            if (streamRole != null)
                parameters.Add("streamRole", streamRole);

            if (jukeboxRole != null)
                parameters.Add("jukeboxRole", jukeboxRole);

            if (downloadRole != null)
                parameters.Add("downloadRole", downloadRole);

            if (uploadRole != null)
                parameters.Add("uploadRole", uploadRole);

            if (playlistRole != null)
                parameters.Add("playlistRole", playlistRole);

            if (coverArtRole != null)
                parameters.Add("coverArtRole", coverArtRole);

            if (commentRole != null)
                parameters.Add("commentRole", commentRole);

            if (podcastRole != null)
                parameters.Add("podcastRole", podcastRole);

            return GetResponse(Methods.createUser, _version130, parameters);
        }

        /// <summary>
        /// Deletes an existing Subsonic user.
        /// </summary>
        /// <param name="username">The name of the user to delete.</param>
        /// <returns></returns>
        public async Task<bool> DeleteUserAsync(string username)
        {
            Hashtable parameters = new Hashtable { { "username", username } };
            return await GetResponseAsync(Methods.deleteUser, _version130, parameters);
        }

        /// <summary>
        /// Deletes an existing Subsonic user.
        /// </summary>
        /// <param name="username">The name of the user to delete.</param>
        /// <returns></returns>
        public bool DeleteUser(string username)
        {
            Hashtable parameters = new Hashtable {{"username", username}};
            return GetResponse(Methods.deleteUser, _version130, parameters);
        }

        /// <summary>
        /// Returns the current visible (non-expired) chat messages.
        /// </summary>Only return messages that are newer than this time. Given as milliseconds since Jan 1, 1970.
        /// <param name="since"></param>
        /// <returns>ChatMessages</returns>
        public async Task<ChatMessages> GetChatMessagesAsync(double? since = null, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable();

            if (since != null)
                parameters.Add("since", (long)since);

            return await GetResponseAsync<ChatMessages>(Methods.getChatMessages, _version120, parameters, cancelToken);
        }

        /// <summary>
        /// Returns the current visible (non-expired) chat messages.
        /// </summary>Only return messages that are newer than this time. Given as milliseconds since Jan 1, 1970.
        /// <param name="since"></param>
        /// <returns>ChatMessages</returns>
        public ChatMessages GetChatMessages(long? since = null)
        {
            Hashtable parameters = new Hashtable();

            if (since != null)
                parameters.Add("since", since);

            return GetResponse<ChatMessages>(Methods.getChatMessages, _version120, parameters);
        }

        /// <summary>
        /// Adds a message to the chat log.
        /// </summary>
        /// <param name="message">The chat message.</param>
        /// <returns>bool</returns>
        public async Task<bool> AddChatMessageAsync(string message)
        {
            Hashtable parameters = new Hashtable { { "message", message } };
            return await GetResponseAsync(Methods.addChatMessage, _version120, parameters);
        }

        /// <summary>
        /// Adds a message to the chat log.
        /// </summary>
        /// <param name="message">The chat message.</param>
        /// <returns>bool</returns>
        public bool AddChatMessage(string message)
        {
            Hashtable parameters = new Hashtable {{"message", message}};
            return GetResponse(Methods.addChatMessage, _version120, parameters);
        }

        /// <summary>
        /// Returns a list of random, newest, highest rated etc. albums. Similar to the album lists on the home page of the Subsonic web interface.
        /// </summary>
        /// <param name="type">	The list type. Must be one of the following: random, newest, highest, frequent, recent. Since 1.8.0 you can also use alphabeticalByName or alphabeticalByArtist to page through all albums alphabetically, and starred to retrieve starred albums.</param>
        /// <param name="size">The number of albums to return. Max 500. [Default = 10]</param>
        /// <param name="offset">The list offset. Useful if you for example want to page through the list of newest albums. [Default = 0]</param>
        /// <returns>AlbumList</returns>
        public async Task<AlbumList> GetAlbumListAsync(AlbumListType type, int? size = null, int? offset = null, CancellationToken? cancelToken = null)
        {
            Version methodApiVersion = _version120;

            if (type == AlbumListType.alphabeticalByArtist || type == AlbumListType.alphabeticalByName || type == AlbumListType.starred)
                methodApiVersion = _version180;

            string albumListTypeName = Enum.GetName(typeof(AlbumListType), type);

            Hashtable parameters = new Hashtable { { "type", albumListTypeName } };

            if (size != null)
                parameters.Add("size", size);

            if (offset != null)
                parameters.Add("offset", offset);

            return await GetResponseAsync<AlbumList>(Methods.getAlbumList, methodApiVersion, parameters, cancelToken);
        }

        /// <summary>
        /// Returns a list of random, newest, highest rated etc. albums. Similar to the album lists on the home page of the Subsonic web interface.
        /// </summary>
        /// <param name="type">	The list type. Must be one of the following: random, newest, highest, frequent, recent. Since 1.8.0 you can also use alphabeticalByName or alphabeticalByArtist to page through all albums alphabetically, and starred to retrieve starred albums.</param>
        /// <param name="size">The number of albums to return. Max 500. [Default = 10]</param>
        /// <param name="offset">The list offset. Useful if you for example want to page through the list of newest albums. [Default = 0]</param>
        /// <returns>AlbumList</returns>
        public AlbumList GetAlbumList(AlbumListType type, int? size = null, int? offset = null)
        {
            Version methodApiVersion = _version120;

            if (type == AlbumListType.alphabeticalByArtist || type == AlbumListType.alphabeticalByName || type == AlbumListType.starred)
                methodApiVersion = _version180;

            string albumListTypeName = Enum.GetName(typeof (AlbumListType), type);

            Hashtable parameters = new Hashtable {{"type", albumListTypeName}};

            if (size != null)
                parameters.Add("size", size);

            if (offset != null)
                parameters.Add("offset", offset);

            return GetResponse<AlbumList>(Methods.getAlbumList, methodApiVersion, parameters);
        }

        /// <summary>
        /// Similar to getAlbumList, but organizes music according to ID3 tags.
        /// </summary>
        /// <param name="type">The list type. Must be one of the following: random, newest, frequent, recent, starred, alphabeticalByName or alphabeticalByArtist.</param>
        /// <param name="size">The number of albums to return. Max 500. [Default = 10]</param>
        /// <param name="offset">The list offset. Useful if you for example want to page through the list of newest albums. [Default = 0]</param>
        /// <returns>AlbumList</returns>
        public async Task<AlbumList2> GetAlbumList2Async(AlbumListType type, int? size = null, int? offset = null, CancellationToken? cancelToken = null)
        {
            string albumListTypeName = Enum.GetName(typeof(AlbumListType), type);

            Hashtable parameters = new Hashtable { { "type", albumListTypeName } };

            if (size != null)
                parameters.Add("size", size);

            if (offset != null)
                parameters.Add("offset", offset);

            return await GetResponseAsync<AlbumList2>(Methods.getAlbumList2, _version180, parameters, cancelToken);
        }

        /// <summary>
        /// Similar to getAlbumList, but organizes music according to ID3 tags.
        /// </summary>
        /// <param name="type">The list type. Must be one of the following: random, newest, frequent, recent, starred, alphabeticalByName or alphabeticalByArtist.</param>
        /// <param name="size">The number of albums to return. Max 500. [Default = 10]</param>
        /// <param name="offset">The list offset. Useful if you for example want to page through the list of newest albums. [Default = 0]</param>
        /// <returns>AlbumList</returns>
        public AlbumList2 GetAlbumList2(AlbumListType type, int? size = null, int? offset = null)
        {
            string albumListTypeName = Enum.GetName(typeof (AlbumListType), type);

            Hashtable parameters = new Hashtable {{"type", albumListTypeName}};

            if (size != null)
                parameters.Add("size", size);

            if (offset != null)
                parameters.Add("offset", offset);

            return GetResponse<AlbumList2>(Methods.getAlbumList2, _version180, parameters);
        }

        /// <summary>
        /// Returns random songs matching the given criteria.
        /// </summary>
        /// <param name="size">The maximum number of songs to return. Max 500. [Default = 10]</param>
        /// <param name="genre">Only returns songs belonging to this genre.</param>
        /// <param name="fromYear">Only return songs published after or in this year.</param>
        /// <param name="toYear">Only return songs published before or in this year.</param>
        /// <param name="musicFolderId">Only return songs in the music folder with the given ID. See GetMusicFolders.</param>
        /// <returns>RandomSongs</returns>
        public async Task<RandomSongs> GetRandomSongsAsync(int? size = null, string genre = null, int? fromYear = null, int? toYear = null, string musicFolderId = null, CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable();

            if (size != null)
                parameters.Add("size", size);

            if (!string.IsNullOrWhiteSpace(genre))
                parameters.Add("genre", genre);

            if (fromYear != null)
                parameters.Add("fromYear", fromYear);

            if (toYear != null)
                parameters.Add("toYear", toYear);

            if (!string.IsNullOrWhiteSpace(musicFolderId))
                parameters.Add("musicFolderId", musicFolderId);

            return await GetResponseAsync<RandomSongs>(Methods.getRandomSongs, _version120, parameters, cancelToken);
        }

        /// <summary>
        /// Returns random songs matching the given criteria.
        /// </summary>
        /// <param name="size">The maximum number of songs to return. Max 500. [Default = 10]</param>
        /// <param name="genre">Only returns songs belonging to this genre.</param>
        /// <param name="fromYear">Only return songs published after or in this year.</param>
        /// <param name="toYear">Only return songs published before or in this year.</param>
        /// <param name="musicFolderId">Only return songs in the music folder with the given ID. See GetMusicFolders.</param>
        /// <returns>RandomSongs</returns>
        public RandomSongs GetRandomSongs(int? size = null, string genre = null, int? fromYear = null, int? toYear = null, string musicFolderId = null)
        {
            Hashtable parameters = new Hashtable();

            if (size != null)
                parameters.Add("size", size);

            if (!string.IsNullOrWhiteSpace(genre))
                parameters.Add("genre", genre);

            if (fromYear != null)
                parameters.Add("fromYear", fromYear);

            if (toYear != null)
                parameters.Add("toYear", toYear);

            if (!string.IsNullOrWhiteSpace(musicFolderId))
                parameters.Add("musicFolderId", musicFolderId);

            return GetResponse<RandomSongs>(Methods.getRandomSongs, _version120, parameters);
        }

        /// <summary>
        /// Searches for and returns lyrics for a given song.
        /// </summary>
        /// <param name="artist">The artist name.</param>
        /// <param name="title">The song title.</param>
        /// <returns>Lyrics</returns>
        public async Task<Lyrics> GetLyricsAsync(string artist = null, string title = null, CancellationToken? cancelToken = null)
        {
            if (string.IsNullOrWhiteSpace(artist) && string.IsNullOrWhiteSpace(title))
                throw new SubsonicApiException("You must specify an artist and/or a title");

            Hashtable parameters = new Hashtable();

            if (!string.IsNullOrWhiteSpace(artist))
                parameters.Add("artist", artist);

            if (!string.IsNullOrWhiteSpace(title))
                parameters.Add("title", title);

            return await GetResponseAsync<Lyrics>(Methods.getLyrics, _version120, parameters, cancelToken);
        }

        /// <summary>
        /// Searches for and returns lyrics for a given song.
        /// </summary>
        /// <param name="artist">The artist name.</param>
        /// <param name="title">The song title.</param>
        /// <returns>Lyrics</returns>
        public Lyrics GetLyrics(string artist = null, string title = null)
        {
            if (string.IsNullOrWhiteSpace(artist) && string.IsNullOrWhiteSpace(title))
                throw new SubsonicApiException("You must specify an artist and/or a title");

            Hashtable parameters = new Hashtable();

            if (!string.IsNullOrWhiteSpace(artist))
                parameters.Add("artist", artist);

            if (!string.IsNullOrWhiteSpace(title))
                parameters.Add("title", title);

            return GetResponse<Lyrics>(Methods.getLyrics, _version120, parameters);
        }

        /// <summary>
        /// Retrieves the jukebox playlist. Note: The user must be authorized to control the jukebox (see Settings > Users > User is allowed to play files in jukebox mode).
        /// </summary>
        /// <returns>JukeboxPlaylist</returns>
        public async Task<JukeboxPlaylist> JukeboxControlAsync(CancellationToken? cancelToken = null)
        {
            Hashtable parameters = new Hashtable { { "action", "get" } };
            return await GetResponseAsync<JukeboxPlaylist>(Methods.jukeboxControl, _version120, parameters, cancelToken);
        }

        /// <summary>
        /// Retrieves the jukebox playlist. Note: The user must be authorized to control the jukebox (see Settings > Users > User is allowed to play files in jukebox mode).
        /// </summary>
        /// <returns>JukeboxPlaylist</returns>
        public JukeboxPlaylist JukeboxControl()
        {
            Hashtable parameters = new Hashtable {{"action", "get"}};
            return GetResponse<JukeboxPlaylist>(Methods.jukeboxControl, _version120, parameters);
        }

        /// <summary>
        /// Controls the jukebox, i.e., playback directly on the server's audio hardware. Note: The user must be authorized to control the jukebox (see Settings > Users > User is allowed to play files in jukebox mode).
        /// </summary>
        /// <param name="action">The operation to perform. Must be one of: start, stop, skip, add, clear, remove, shuffle, setGain</param>
        /// <param name="index">Used by skip and remove. Zero-based index of the song to skip to or remove.</param>
        /// <param name="gain">	Used by setGain to control the playback volume. A float value between 0.0 and 1.0.</param>
        /// <param name="id">Used by add. ID of song to add to the jukebox playlist. Use multiple id parameters to add many songs in the same request.</param>
        /// <returns>bool</returns>
        public async Task<bool> JukeboxControlAsync(JukeboxControlAction action, int? index = null, float? gain = null, IEnumerable<string> id = null)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            string actionName = Enum.GetName(typeof(JukeboxControlAction), action);

            if (string.IsNullOrWhiteSpace(actionName))
                throw new SubsonicApiException("You must provide valid action");

            parameters.Add(new KeyValuePair<string, string>("action", actionName));

            if ((action == JukeboxControlAction.skip || action == JukeboxControlAction.remove) && index != null)
                parameters.Add(new KeyValuePair<string, string>("index", index.ToString()));

            if (action == JukeboxControlAction.add)
            {
                if (id == null)
                    throw new SubsonicApiException("You must provide at least 1 ID.");

                foreach (string idparam in id)
                    parameters.Add(new KeyValuePair<string, string>("id", idparam));
            }

            if (action == JukeboxControlAction.setGain)
            {
                if (gain == null || (gain < 0 || gain > 1))
                    throw new SubsonicApiException("Gain value must be >= 0.0 and <= 1.0");

                parameters.Add(new KeyValuePair<string, string>("setGain", gain.ToString()));
            }

            return await GetResponseAsync(Methods.jukeboxControl, _version120, parameters);
        }

        /// <summary>
        /// Controls the jukebox, i.e., playback directly on the server's audio hardware. Note: The user must be authorized to control the jukebox (see Settings > Users > User is allowed to play files in jukebox mode).
        /// </summary>
        /// <param name="action">The operation to perform. Must be one of: start, stop, skip, add, clear, remove, shuffle, setGain</param>
        /// <param name="index">Used by skip and remove. Zero-based index of the song to skip to or remove.</param>
        /// <param name="gain">	Used by setGain to control the playback volume. A float value between 0.0 and 1.0.</param>
        /// <param name="id">Used by add. ID of song to add to the jukebox playlist. Use multiple id parameters to add many songs in the same request.</param>
        /// <returns>bool</returns>
        public bool JukeboxControl(JukeboxControlAction action, int? index = null, float? gain = null, IEnumerable<string> id = null)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            string actionName = Enum.GetName(typeof (JukeboxControlAction), action);

            if (string.IsNullOrWhiteSpace(actionName))
                throw new SubsonicApiException("You must provide valid action");

            parameters.Add(new KeyValuePair<string, string>("action", actionName));

            if ((action == JukeboxControlAction.skip || action == JukeboxControlAction.remove) && index != null)
                parameters.Add(new KeyValuePair<string, string>("index", index.Value.ToString()));

            if (action == JukeboxControlAction.add)
            {
                if (id == null)
                    throw new SubsonicApiException("You must provide at least 1 ID.");

                foreach (string idparam in id)
                    parameters.Add(new KeyValuePair<string, string>("id", idparam));
            }

            if (action == JukeboxControlAction.setGain)
            {
                if (gain == null || (gain < 0 || gain > 1))
                    throw new SubsonicApiException("Gain value must be >= 0.0 and <= 1.0");

                parameters.Add(new KeyValuePair<string, string>("setGain", gain.ToString()));
            }

            return GetResponse(Methods.jukeboxControl, _version120, parameters);
        }

        /// <summary>
        /// Returns all podcast channels the server subscribes to and their episodes.
        /// </summary>
        /// <returns>Podcasts</returns>
        public async Task<Podcasts> GetPodcastsAsync(CancellationToken? cancelToken = null)
        {
            return await GetResponseAsync<Podcasts>(Methods.getPodcasts, _version160, null, cancelToken);
        }

        /// <summary>
        /// Returns all podcast channels the server subscribes to and their episodes.
        /// </summary>
        /// <returns>Podcasts</returns>
        public Podcasts GetPodcasts()
        {
            return GetResponse<Podcasts>(Methods.getPodcasts, _version160);
        }
    }
}