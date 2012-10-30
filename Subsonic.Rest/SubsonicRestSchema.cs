using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Subsonic.Rest.Api
{
    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    [XmlRoot("subsonic-response", Namespace = "http://subsonic.org/restapi", IsNullable = false)]
    public class Response
    {
        [XmlElement("album", typeof (AlbumWithSongsID3))]
        [XmlElement("albumList", typeof (AlbumList))]
        [XmlElement("albumList2", typeof (AlbumList2))]
        [XmlElement("artist", typeof (ArtistWithAlbumsID3))]
        [XmlElement("artists", typeof (ArtistsID3))]
        [XmlElement("chatMessages", typeof (ChatMessages))]
        [XmlElement("directory", typeof (Directory))]
        [XmlElement("error", typeof (Error))]
        [XmlElement("indexes", typeof (Indexes))]
        [XmlElement("jukeboxPlaylist", typeof (JukeboxPlaylist))]
        [XmlElement("jukeboxStatus", typeof (JukeboxStatus))]
        [XmlElement("license", typeof (License))]
        [XmlElement("lyrics", typeof (Lyrics))]
        [XmlElement("musicFolders", typeof (MusicFolders))]
        [XmlElement("nowPlaying", typeof (NowPlaying))]
        [XmlElement("playlist", typeof (PlaylistWithSongs))]
        [XmlElement("playlists", typeof (Playlists))]
        [XmlElement("podcasts", typeof (Podcasts))]
        [XmlElement("randomSongs", typeof (RandomSongs))]
        [XmlElement("searchResult", typeof (SearchResult))]
        [XmlElement("searchResult2", typeof (SearchResult2))]
        [XmlElement("searchResult3", typeof (SearchResult3))]
        [XmlElement("shares", typeof (Shares))]
        [XmlElement("song", typeof (Child))]
        [XmlElement("starred", typeof (Starred))]
        [XmlElement("starred2", typeof (Starred2))]
        [XmlElement("user", typeof (User))]
        [XmlElement("users", typeof (Users))]
        [XmlElement("videos", typeof (Videos))]
        [XmlChoiceIdentifier("ItemElementName")]
        public object Item;

        [XmlIgnore] public ItemChoiceType ItemElementName;
        [XmlAttribute("status")] public ResponseStatus Status;
        [XmlAttribute("version")] public string Version;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class AlbumWithSongsID3 : AlbumID3
    {
        [XmlElement("song")] public List<Child> Song;
    }

    [XmlInclude(typeof (PodcastEpisode))]
    [XmlInclude(typeof (NowPlayingEntry))]
    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Child
    {
        [XmlAttribute("album")] public string Album;
        [XmlAttribute("albumId")] public string AlbumId;
        [XmlAttribute("artist")] public string Artist;
        [XmlAttribute("artistId")] public string ArtistId;
        [XmlAttribute("averageRating")] public double AverageRating;
        [XmlAttribute("bitRate")] public int BitRate;
        [XmlAttribute("contentType")] public string ContentType;
        [XmlAttribute("coverArt")] public string CoverArt;
        [XmlAttribute("created")] public DateTime Created;
        [XmlAttribute("discNumber")] public int DiscNumber;
        [XmlAttribute("duration")] public int Duration;
        [XmlAttribute("genre")] public string Genre;
        [XmlAttribute("id")] public string Id;
        [XmlAttribute("isDir")] public bool IsDir;
        [XmlAttribute("isVideo")] public bool IsVideo;
        [XmlAttribute("parent")] public string Parent;
        [XmlAttribute("path")] public string Path;
        [XmlAttribute("size")] public long Size;
        [XmlAttribute("starred")] public DateTime Starred;
        [XmlAttribute("suffix")] public string Suffix;
        [XmlAttribute("title")] public string Title;
        [XmlAttribute("track")] public int Track;
        [XmlAttribute("transcodedContentType")] public string TranscodedContentType;
        [XmlAttribute("transcodedSuffix")] public string TranscodedSuffix;
        [XmlAttribute("type")] public MediaType Type;
        [XmlAttribute("userRating")] public int UserRating;
        [XmlAttribute("year")] public int Year;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Error
    {
        [XmlAttribute("code")] public int Code;
        [XmlAttribute("message")] public string Message;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Starred2
    {
        [XmlElement("album")] public List<AlbumID3> Album;
        [XmlElement("artist")] public List<ArtistID3> Artist;
        [XmlElement("song")] public List<Child> Song;
    }

    [XmlInclude(typeof (ArtistWithAlbumsID3))]
    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class ArtistID3
    {
        [XmlAttribute("albumCount")] public int AlbumCount;
        [XmlAttribute("coverArt")] public string CoverArt;
        [XmlAttribute("id")] public string Id;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("starred")] public DateTime Starred;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class ArtistWithAlbumsID3 : ArtistID3
    {
        [XmlElement("album")] public List<AlbumID3> Album;
    }

    [XmlInclude(typeof (AlbumWithSongsID3))]
    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class AlbumID3
    {
        [XmlAttribute("artist")] public string Artist;
        [XmlAttribute("artistId")] public string ArtistId;
        [XmlAttribute("coverArt")] public string CoverArt;
        [XmlAttribute("created")] public DateTime Created;
        [XmlAttribute("duration")] public int Duration;
        [XmlAttribute("id")] public string Id;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("songCount")] public int SongCount;
        [XmlAttribute("starred")] public DateTime Starred;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Starred
    {
        [XmlElement("album")] public List<Child> Album;
        [XmlElement("artist")] public List<Artist> Artist;
        [XmlElement("song")] public List<Child> Song;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Artist
    {
        [XmlAttribute("id")] public string Id;
        [XmlAttribute("name")] public string Name;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Shares
    {
        [XmlElement("share")] public List<Share> Share;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Share
    {
        [XmlAttribute("created")] public DateTime Created;
        [XmlAttribute("description")] public string Description;
        [XmlElement("entry")] public List<Child> Entry;
        [XmlAttribute("expires")] public DateTime Expires;
        [XmlAttribute("id")] public string Id;
        [XmlAttribute("lastVisited")] public DateTime LastVisited;
        [XmlAttribute("url")] public string Url;
        [XmlAttribute("username")] public string Username;
        [XmlAttribute("visitCount")] public int VisitCount;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Podcasts
    {
        [XmlElement("channel")] public List<PodcastChannel> Channel;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class PodcastChannel
    {
        [XmlAttribute("description")] public string Description;
        [XmlElement("episode")] public List<PodcastEpisode> Episode;
        [XmlAttribute("errorMessage")] public string ErrorMessage;
        [XmlAttribute("id")] public string Id;
        [XmlAttribute("status")] public PodcastStatus Status;
        [XmlAttribute("title")] public string Title;
        [XmlAttribute("url")] public string Url;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class PodcastEpisode : Child
    {
        [XmlAttribute("description")] public string Description;
        [XmlAttribute("publishDate")] public DateTime PublishDate;
        [XmlAttribute("status")] public PodcastStatus Status;
        [XmlAttribute("streamId")] public string StreamId;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Lyrics
    {
        [XmlAttribute("artist")] public string Artist;
        [XmlText] public List<string> Text;
        [XmlAttribute("title")] public string Title;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class RandomSongs
    {
        [XmlElement("song")] public List<Child> Song;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class AlbumList2
    {
        [XmlElement("album")] public List<AlbumID3> Album;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class AlbumList
    {
        [XmlElement("album")] public List<Child> Album;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class ChatMessages
    {
        [XmlElement("chatMessage")] public List<ChatMessage> ChatMessage;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class ChatMessage
    {
        [XmlAttribute("message")] public string Message;
        [XmlAttribute("time")] public long Time;
        [XmlAttribute("username")] public string Username;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Users
    {
        [XmlElement("user")] public List<User> User;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class User
    {
        [XmlAttribute("adminRole")] public bool AdminRole;
        [XmlAttribute("commentRole")] public bool CommentRole;
        [XmlAttribute("coverArtRole")] public bool CoverArtRole;
        [XmlAttribute("downloadRole")] public bool DownloadRole;
        [XmlAttribute("email")] public string Email;
        [XmlAttribute("jukeboxRole")] public bool JukeboxRole;
        [XmlAttribute("playlistRole")] public bool PlaylistRole;
        [XmlAttribute("podcastRole")] public bool PodcastRole;
        [XmlAttribute("scrobblingEnabled")] public bool ScrobblingEnabled;
        [XmlAttribute("settingsRole")] public bool SettingsRole;
        [XmlAttribute("shareRole")] public bool ShareRole;
        [XmlAttribute("streamRole")] public bool StreamRole;
        [XmlAttribute("uploadRole")] public bool UploadRole;
        [XmlAttribute("username")] public string Username;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class License
    {
        [XmlAttribute("date")] public DateTime Date;
        [XmlAttribute("email")] public string Email;
        [XmlAttribute("key")] public string Key;
        [XmlAttribute("valid")] public bool Valid;
    }

    [XmlInclude(typeof (JukeboxPlaylist))]
    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class JukeboxStatus
    {
        [XmlAttribute("currentIndex")] public int CurrentIndex;
        [XmlAttribute("gain")] public float Gain;
        [XmlAttribute("playing")] public bool Playing;
        [XmlAttribute("position")] public int Position;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class JukeboxPlaylist : JukeboxStatus
    {
        [XmlElement("entry")] public List<Child> Entry;
    }


    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Playlists
    {
        [XmlElement("playlist")] public List<Playlist> Playlist;
    }

    [XmlInclude(typeof (PlaylistWithSongs))]
    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Playlist
    {
        [XmlElement("allowedUser")] public List<string> AllowedUser;
        [XmlAttribute("comment")] public string Comment;
        [XmlAttribute("created")] public DateTime Created;
        [XmlAttribute("duration")] public int Duration;
        [XmlAttribute("id")] public string Id;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("owner")] public string Owner;
        [XmlAttribute("public")] public bool Public;
        [XmlAttribute("songCount")] public int SongCount;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class PlaylistWithSongs : Playlist
    {
        [XmlElement("entry")] public List<Child> Entry;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class SearchResult3
    {
        [XmlElement("album")] public List<AlbumID3> Album;
        [XmlElement("artist")] public List<ArtistID3> Artist;
        [XmlElement("song")] public List<Child> Song;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class SearchResult2
    {
        [XmlElement("album")] public List<Child> Album;
        [XmlElement("artist")] public List<Artist> Artist;
        [XmlElement("song")] public List<Child> Song;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class SearchResult
    {
        [XmlElement("match")] public List<Child> Match;
        [XmlAttribute("offset")] public int Offset;
        [XmlAttribute("totalHits")] public int TotalHits;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class NowPlaying
    {
        [XmlElement("entry")] public List<NowPlayingEntry> Entry;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class NowPlayingEntry : Child
    {
        [XmlAttribute("minutesAgo")] public int MinutesAgo;
        [XmlAttribute("playerId")] public int PlayerId;
        [XmlAttribute("playerName")] public string PlayerName;
        [XmlAttribute("username")] public string Username;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Videos
    {
        [XmlElement("video")] public List<Child> Video;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class ArtistsID3
    {
        [XmlElement("index")] public List<IndexID3> Index;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class IndexID3
    {
        [XmlElement("artist")] public List<ArtistID3> Artist;
        [XmlAttribute("name")] public string Name;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Directory
    {
        [XmlElement("child")] public List<Child> Child;
        [XmlAttribute("id")] public string Id;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("parent")] public string Parent;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Index
    {
        [XmlElement("artist")] public List<Artist> Artist;
        [XmlAttribute("name")] public string Name;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class Indexes
    {
        [XmlElement("child")] public List<Child> Child;
        [XmlElement("index")] public List<Index> Index;
        [XmlAttribute("lastModified")] public long LastModified;
        [XmlElement("shortcut")] public List<Artist> Shortcut;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class MusicFolders
    {
        [XmlElement("musicFolder")] public List<MusicFolder> MusicFolder;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public class MusicFolder
    {
        [XmlAttribute("id")] public int Id;
        [XmlAttribute("name")] public string Name;
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public enum MediaType
    {
        [XmlEnum(Name = "music")] Music,
        [XmlEnum(Name = "podcast")] Podcast,
        [XmlEnum(Name = "audiobook")] Audiobook,
        [XmlEnum(Name = "video")] Video,
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public enum PodcastStatus
    {
        [XmlEnum(Name = "new")] New,
        [XmlEnum(Name = "downloading")] Downloading,
        [XmlEnum(Name = "completed")] Completed,
        [XmlEnum(Name = "error")] Error,
        [XmlEnum(Name = "deleted")] Deleted,
        [XmlEnum(Name = "skipped")] Skipped,
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi", IncludeInSchema = false)]
    public enum ItemChoiceType
    {
        [XmlEnum(Name = "album")] Album,
        [XmlEnum(Name = "albumList")] AlbumList,
        [XmlEnum(Name = "albumList2")] AlbumList2,
        [XmlEnum(Name = "artist")] Artist,
        [XmlEnum(Name = "artists")] Artists,
        [XmlEnum(Name = "chatMessages")] ChatMessages,
        [XmlEnum(Name = "directory")] Directory,
        [XmlEnum(Name = "error")] Error,
        [XmlEnum(Name = "indexes")] Indexes,
        [XmlEnum(Name = "jukeboxPlaylist")] JukeboxPlaylist,
        [XmlEnum(Name = "jukeboxStatus")] JukeboxStatus,
        [XmlEnum(Name = "license")] License,
        [XmlEnum(Name = "lyrics")] Lyrics,
        [XmlEnum(Name = "musicFolders")] MusicFolders,
        [XmlEnum(Name = "nowPlaying")] NowPlaying,
        [XmlEnum(Name = "playlist")] Playlist,
        [XmlEnum(Name = "playlists")] Playlists,
        [XmlEnum(Name = "podcasts")] Podcasts,
        [XmlEnum(Name = "randomSongs")] RandomSongs,
        [XmlEnum(Name = "searchResult")] SearchResult,
        [XmlEnum(Name = "searchResult2")] SearchResult2,
        [XmlEnum(Name = "searchResult3")] SearchResult3,
        [XmlEnum(Name = "shares")] Shares,
        [XmlEnum(Name = "song")] Song,
        [XmlEnum(Name = "starred")] Starred,
        [XmlEnum(Name = "starred2")] Starred2,
        [XmlEnum(Name = "user")] User,
        [XmlEnum(Name = "users")] Users,
        [XmlEnum(Name = "videos")] Videos,
    }

    [Serializable]
    [XmlType(Namespace = "http://subsonic.org/restapi")]
    public enum ResponseStatus
    {
        [XmlEnum(Name = "ok")] Ok,
        [XmlEnum(Name = "failed")] Failed,
    }
}