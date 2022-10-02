using System;

namespace MusicLibrary.Metadata
{
    public sealed class ZeroProperties : IAudioFileProperties
    {
        public TimeSpan Duration { get; } = TimeSpan.Zero;
    }
}
