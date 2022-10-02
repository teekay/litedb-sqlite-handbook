using SQLite;

namespace MusicLibrary.Persistence.SqliteNet.Model
{
    internal class OpenPlaylists
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int PlaylistId { get; set; }

        public string? SearchTerm { get; set; }
    }

}
