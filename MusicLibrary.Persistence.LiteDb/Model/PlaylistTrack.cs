namespace MusicLibrary.Persistence.LiteDb.Model
{
    internal class PlaylistTrack
    {
        public int PlaylistId { get; set; }
        public Track Track { get; set; }
        public int Position { get; set; }
        public long CreatedAt { get; set; }
    }
}
