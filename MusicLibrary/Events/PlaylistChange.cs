namespace MusicLibrary.Events
{
    /// <summary>
    /// What happened to the playlist?
    /// </summary>
    public enum PlaylistChange
    {
        Added,
        Copied,
        Moved,
        Replaced,
        Deleted
    }
}
