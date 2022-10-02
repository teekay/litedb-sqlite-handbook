using MusicLibrary.Playlists;
using SQLite;

namespace MusicLibrary.Persistence.SqliteNet.Model
{
    internal class Playlist: IPersistedPlaylist
    {
        private string _uri = string.Empty;
        private string _comment = string.Empty;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        [Column("Filename")]
        public string Uri
        {
            get => _uri;
            set => _uri = value ?? string.Empty;
        }

        public long LastScannedOn { get; set; }

        [Indexed]
        public string Comment
        {
            get => _comment;
            set => _comment = value ?? string.Empty;
        }
    }
}
