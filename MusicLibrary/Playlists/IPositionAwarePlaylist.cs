namespace MusicLibrary.Playlists
{
    public interface IPositionAwarePlaylist
    {
        /// <summary>
        /// Item preceding Current.
        /// </summary>
        IPlaylistItem? Previous { get; }

        /// <summary>
        /// Item marked as "current" in the sense that it is an object
        /// of immediate interest, as in "to be played back" or "playing" etc.
        /// </summary>
        IPlaylistItem? Current { get; }

        /// <summary>
        /// Item immediately following Current.
        /// </summary>
        IPlaylistItem? Forthcoming { get; }

        /// <summary>
        /// An item to which the playback shall skip.
        /// </summary>
        IPlaylistItem? ToBeSkippedTo { get; }

        /// <summary>
        /// Item that the DJ intends to skip to.
        /// Since this action is cancellable and takes some time, the data item needs to be stored somewhere,
        /// and this is as good a place as any. Or is it?
        /// </summary>
        void SkipTo(IPlaylistItem playlistItem);
    }
}
