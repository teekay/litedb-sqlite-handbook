using SQLite;

namespace MusicLibrary.Persistence.SqliteNet.Model
{
    [Table("PlaylistTracks")]
    internal class PlaylistTrack
    {
        [PrimaryKey]
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public int TrackId { get; set; }
        public int Position { get; set; }
        public long CreatedAt { get; set; }
    }
}
