using System.Linq;
using MoreLinq;

namespace MusicLibrary.Metadata
{
    internal sealed class MergedMetadata: IMetadata
    {
        public MergedMetadata(params IMetadata[] sources)
        {
            Year = sources.Select(s => s.Year).Filter(NonNull)
                .MaxBy(s => s.Length) // I saw Taglib truncate 1938 to 193
                .LastOrDefault() ?? string.Empty;
            Title = sources.Select(s => s.Title).Filter(NonNull).LastOrDefault() ?? string.Empty;
            Artist = sources.Select(s => s.Artist).Filter(NonNull).LastOrDefault() ?? string.Empty;
            Album = sources.Select(s => s.Album).Filter(NonNull).LastOrDefault() ?? string.Empty;
            var c = sources.Select(s => s.Comment).Filter(NonNull)
                .MaxBy(s => s.Length); // a hack to take the longest comment available, due to a bug in TagLib that discards anything after the first semicolon
            Comment = c.FirstOrDefault() ?? string.Empty;
            Genre = sources.Select(s => s.Genre).Filter(NonNull).LastOrDefault() ?? string.Empty;
            AlbumArtist = sources.Select(s => s.AlbumArtist).Filter(NonNull).LastOrDefault() ?? string.Empty;
            Conductor = sources.Select(s => s.Conductor).Filter(NonNull).LastOrDefault() ?? string.Empty;
            BPM = sources.Select(s => s.BPM).Filter(x => x != 0).LastOrDefault();
            ReplayGain = sources.Select(s => s.ReplayGain).Filter(x => x != 0).LastOrDefault();
            Rating = sources.Select(s => s.Rating).Filter(x => x != 0).LastOrDefault();
            Grouping = sources.Select(s => s.Grouping).Filter(NonNull).LastOrDefault() ?? string.Empty;
        }

        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string AlbumArtist { get; private set; }
        public string Conductor { get; private set; }
        public string Genre { get; private set; }
        public string Year { get; private set; }
        public string Album { get; private set; }
        public string Comment { get; private set; }
        public int Rating { get; private set; }
        public string Grouping { get; private set; }
        public double BPM { get; private set; }
        public double ReplayGain { get; private set; }
        
        public void Update(IMetadata source)
        {
            Title = source.Title;
            Artist = source.Artist;
            Conductor = source.Conductor;
            Genre = source.Genre;
            Year = source.Year;
            Album = source.Album;
            AlbumArtist = source.AlbumArtist;
            Comment = source.Comment;
            Rating = source.Rating;
            BPM = source.BPM;
            ReplayGain = source.ReplayGain;
            Grouping = source.Grouping;
        }

        public override string ToString()
        {
            return $"{Artist};{AlbumArtist};{Title};{Genre};{Album};{Comment}";
        }

        private static bool NonNull(string x) => !string.IsNullOrEmpty(x);
    }
}
