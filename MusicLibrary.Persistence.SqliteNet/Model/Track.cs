using SQLite;

namespace MusicLibrary.Persistence.SqliteNet.Model
{
    [Table("Track")]
    internal class Track
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        [Collation("NOCASE")]
        [Column("Filepath")]
        public string? Uri { get; set; }

        [Indexed]
        public string? Title { get; set; }

        [Indexed]
        public string? Artist { get; set; }
        public string? AlbumArtist { get; set; }
        public string? Conductor { get; set; }
        public string? Album { get; set; }

        [Indexed]
        public string? Genre { get; set; }

        public string? Year { get; set; }
        public long Duration { get; set; }

        [Indexed]
        public string? Comment { get; set; }
        public double BPM { get; set; }
        public double ReplayGain { get; set; }
        public int Rating { get; set; }

        public long StartTime { get; set; }
        public long EndTime { get; set; }

        public byte[]? WaveformData { get; set; }

        public long LastScannedOn { get; set; }

        public bool ConfirmedReadable { get; set; }
        
        [Indexed]
        [Collation("NOCASE")]
        public string? SearchIndex { get; set; }

        [Indexed]
        public string? Grouping { get; set; }
    }
}
