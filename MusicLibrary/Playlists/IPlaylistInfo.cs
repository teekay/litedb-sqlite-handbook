namespace MusicLibrary.Playlists
{
    public interface IPlaylistInfo
    {
        /// <summary>
        /// The filename of this playlist
        /// </summary>
        string Uri { get; }

        /// <summary>
        /// Free-text comment of the DJ
        /// I will allow this property to be read-write for a little longer.
        /// </summary>
        string Comment { get; set; }
    }
}
