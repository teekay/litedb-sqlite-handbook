using System;

namespace MusicLibrary.Metadata
{
    public interface IAudioFileProperties
    {
        TimeSpan Duration { get; }
    }
}
