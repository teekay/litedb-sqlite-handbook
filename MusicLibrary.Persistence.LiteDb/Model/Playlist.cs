using MusicLibrary.Playlists;

namespace MusicLibrary.Persistence.LiteDb.Model
{
    internal class Playlist : IPersistedPlaylist
    {
        private string _uri = string.Empty;
        private string _comment = string.Empty;

        public int Id { get; set; }

        public string Uri
        {
            get => _uri;
            set => _uri = value ?? string.Empty;
        }

        public string Comment
        {
            get => _comment;
            set => _comment = value ?? string.Empty;
        }

        public long LastScannedOn { get; set; }
    }
}
