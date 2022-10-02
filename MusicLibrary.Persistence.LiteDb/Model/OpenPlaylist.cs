#nullable enable

namespace MusicLibrary.Persistence.LiteDb.Model
{
    internal class OpenPlaylist
    {
        public int Id { get; set; }

        public int PlaylistId { get; set; }

        public string? SearchTerm { get; set; }
    }
}
