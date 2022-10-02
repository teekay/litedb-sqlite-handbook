using System;

namespace MusicLibrary.Metadata
{
    /// <summary>
    /// Provides cached metadata for an ITrack in transient operations.
    /// </summary>
    public class CachedMetadataSource : IMetadataSource
    {
        public CachedMetadataSource(IMetadata cache, TimeSpan duration)
        {
            _cache = cache;
            _properties = new TaggedAudioFileProperties(duration);
        }

        private readonly IMetadata _cache;
        private readonly IAudioFileProperties _properties;

        public IMetadata Lyrics() => _cache;

        public IAudioFileProperties Properties() => _properties;
    }
}
