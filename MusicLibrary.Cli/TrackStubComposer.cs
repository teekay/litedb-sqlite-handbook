using MusicLibrary.Metadata;

namespace MusicLibrary.Cli
{
    internal class TrackStubComposer : IComposer
    {
        public ITrack SongPlayableOnPlatform(string uri, IMetadataSource metadataSource, byte[] waveform)
        {
            return new TrackStub(uri, metadataSource, waveform);
        }
    }
}
