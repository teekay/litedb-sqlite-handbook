namespace MusicLibrary.Events
{
    /// <summary>
    /// Defines what actions Conductor took to modify playlist
    /// e.g. delete files that are unplayable
    /// </summary>
    public enum PlaylistAction
    {
        Modified,
        Deleted
    }
}
