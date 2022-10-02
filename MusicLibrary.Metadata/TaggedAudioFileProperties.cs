using System;

namespace MusicLibrary.Metadata
{
    /// <summary>
    /// This class encapsulates the duration of something, but does not really offer
    /// any functionality on top of the encapsulation... what to do here?
    /// Oh yes: we could encapsulate additional information like bitrate, codecs...
    /// </summary>
    public sealed class TaggedAudioFileProperties : IAudioFileProperties
    {
        public TaggedAudioFileProperties(TimeSpan duration)
        {
            Duration = duration;
        }

        public TimeSpan Duration { get; }
    }
}
