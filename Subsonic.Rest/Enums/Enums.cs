namespace Subsonic.Rest.Api.Enums
{
    /// <summary>
    /// Query type to be performed when calling GetAlbumList.
    /// </summary>
    public enum AlbumListType
    {
        random,
        newest,
        highest,
        frequent,
        recent,
        alphabeticalByName,
        alphabeticalByArtist,
        starred
    }

    /// <summary>
    /// Action to be performed when calling JukeboxControl.
    /// </summary>
    public enum JukeboxControlAction
    {
        get,
        status,
        set,
        start,
        stop,
        skip,
        add,
        clear,
        remove,
        shuffle,
        setGain
    }

    /// <summary>
    /// Stream formats.
    /// </summary>
    public enum StreamFormat
    {
        mp3,
        flv
    }

    /// <summary>
    /// Methods available in the supported Subsonic API version.
    /// </summary>
    internal enum Methods
    {
        ping,
        getLicense,
        getMusicFolders,
        getNowPlaying,
        getIndexes,
        getMusicDirectory,
        getArtists,
        getArtist,
        getAlbum,
        getSong,
        getVideos,
        search,
        search2,
        getPlaylists,
        getPlaylist,
        createPlaylist,
        deletePlaylist,
        download,
        stream,
        getCoverArt,
        scrobble,
        changePassword,
        getUser,
        createUser,
        deleteUser,
        getChatMessages,
        addChatMessage,
        getAlbumList,
        getAlbumList2,
        getRandomSongs,
        getLyrics,
        jukeboxControl,
        getPodcasts,
        getShareUrl,
        getStarred,
        getStarred2,
        updatePlaylist,
        search3,
        getAvatar,
        star,
        unstar,
        setRating,
        getShares,
    }
}