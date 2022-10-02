using System;
using System.Linq;
using MusicLibrary.Metadata.Meta;

namespace MusicLibrary.Metadata
{
    /// <summary>
    /// Merges metadata from encapsulated ITrack sources - it takes non-null values and picks the last one
    /// for each property.
    /// </summary>
    public sealed class MergesMetadataFromTracks
    {
        public MergesMetadataFromTracks(params ITrack[] sources)
        {
            if (!AreFromTheSameSource(sources)) throw new InvalidOperationException("Some of the sources point to different Uris");
            _sources = sources;
        }

        public IMetadata MergedMeta()
        {
            return new SongMetadata(
                _sources.Map(s => s.Meta.Year).Filter(NonNull).LastOrDefault() ?? string.Empty,
                _sources.Map(s => s.Meta.Title).Filter(NonNull).LastOrDefault() ?? string.Empty,
                _sources.Map(s => s.Meta.Artist).Filter(NonNull).LastOrDefault() ?? string.Empty,
                _sources.Map(s => s.Meta.Album).Filter(NonNull).LastOrDefault() ?? string.Empty,
                _sources.Map(s => s.Meta.Comment).Filter(NonNull).LastOrDefault() ?? string.Empty,
                _sources.Map(s => s.Meta.Genre).Filter(NonNull).LastOrDefault() ?? string.Empty,
                _sources.Map(s => s.Meta.AlbumArtist).Filter(NonNull).LastOrDefault() ?? string.Empty,
                _sources.Map(s => s.Meta.Conductor).Filter(NonNull).LastOrDefault() ?? string.Empty,
                _sources.Map(s => s.Meta.BPM).Filter(x => x != 0).LastOrDefault(),
                _sources.Map(s => s.Meta.ReplayGain).Filter(x => x != 0).LastOrDefault(),
                (uint)Math.Abs(_sources.Map(s => s.Meta.Rating).Filter(x => x != 0).LastOrDefault()),
                _sources.Map(s => s.Meta.Grouping).Filter(NonNull).LastOrDefault() ?? string.Empty);
        }

        public byte[] LastKnownWaveform() => _sources.Map(s => s.WaveformData)
                    .Filter(wd => wd != null && wd.Length > 0)
                    .LastOrDefault() ?? new byte[0];

        private readonly ITrack[] _sources;

        private static bool AreFromTheSameSource(params ITrack[] sources) =>
            sources?.Map(s => s.Uri).Distinct().Length() <= 1;

        private static bool NonNull(string x) => !string.IsNullOrEmpty(x);

    }
}
