using MusicLibrary.Metadata;

namespace MusicLibrary
{
    public interface IComposer
    {
        ITrack SongPlayableOnPlatform(string uri, IMetadataSource metadataSource, byte[] waveform);
    }
}
