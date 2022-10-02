using MusicLibrary.Metadata;

namespace MusicLibrary.Cli
{
    internal class TrackStub: ITrack
    {
        public TrackStub(string uri, IMetadataSource metaSource)
        {
            Uri = uri;
            Meta = metaSource.Lyrics();
            _properties = metaSource.Properties();
        }
        public TrackStub(string uri, IMetadataSource metaSource, byte[] waveform): this(uri, metaSource)
        {
            WaveformData = waveform;
        }

        public TrackStub(string uri, IMetadataSource metaSource, byte[] waveform, TimeSpan start, TimeSpan end, bool isReadable): this(uri, metaSource)
        {
            WaveformData = waveform;
            StartTime = start;
            EndTime = end;
            ConfirmedReadable = isReadable;
        }

        private readonly IAudioFileProperties _properties;

        public string Uri { get; }

        public TimeSpan Duration { get; }

        public TimeSpan StartTime { get; }

        public TimeSpan EndTime { get; }

        public IMetadata Meta { get; }

        public byte[] WaveformData { get; } = new byte[0];

        public bool ConfirmedReadable { get; } = false;

        public event EventHandler<EventArgs>? DurationChanged;
        public event EventHandler<EventArgs>? WaveformUpdated;
        public event EventHandler<EventArgs>? ReadFailed;

        public void CheckIfReadable()
        {
            // no-op
        }

        public IMetadata Lyrics()
        {
            return Meta;
        }

        public Stream Notes()
        {
            throw new NotImplementedException();
        }

        public IAudioFileProperties Properties()
        {
            return _properties;
        }

        public void TrimEnd(TimeSpan newEnd)
        {
            // no-op
        }

        public void TrimSilence(float zeroVolume)
        {
            // no-op
        }

        public void TrimStart(TimeSpan newStart)
        {
            // no-op
        }

        public IObservable<float> WaveformStream()
        {
            throw new NotImplementedException();
        }
    }
}
