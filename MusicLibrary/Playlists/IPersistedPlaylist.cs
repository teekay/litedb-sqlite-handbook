namespace MusicLibrary.Playlists
{
    /// <summary>
    /// A playlist that has been saved in the database or somewhere
    /// </summary>
    public interface IPersistedPlaylist : IPlaylistInfo
    {
        /// <summary>
        /// Just a record Id
        /// </summary>
        int Id { get; }
        long LastScannedOn { get; }
    }
}
