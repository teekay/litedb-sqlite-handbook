#nullable enable
using System;
using System.IO;
using MusicLibrary.Metadata;

namespace MusicLibrary
{
    public sealed class PersistedSong : IPersistedTrack
    {
        public PersistedSong(ITrack source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _metadata = source.Meta;
            _properties = source.Properties();
            source.WaveformUpdated += (sender, args) => WaveformUpdated?.Invoke(this, args);
            source.DurationChanged += (sender, args) => DurationChanged?.Invoke(this, args);
        }

        public PersistedSong(ITrack source, int id) : this(source)
        {
            Id = id;
        }

        public PersistedSong(ITrack source, int id, TimeSpan startTime, TimeSpan endTime, bool isReadable) : this(source, id)
        {
            _confirmedReadable = isReadable;
            LastScannedOn = DateTime.Now;
            TrimStart(startTime);
            TrimEnd(endTime);
        }

        public PersistedSong(ITrack source, IMetadata metadata, IAudioFileProperties properties) : this(source)
        {
            _metadata = metadata;
            _properties = properties;
        }

        public PersistedSong(ITrack source, IMetadata metadata, IAudioFileProperties properties, int id) 
            : this(source, metadata, properties)
        {
            Id = id;
        }

        public PersistedSong(ITrack source, IMetadata metadata, IAudioFileProperties properties, int id, DateTime lastScannedOn) 
            : this(source, metadata, properties, id)
        {
            LastScannedOn = lastScannedOn;
        }

        public PersistedSong(ITrack source, IMetadata metadata, IAudioFileProperties properties, int id, DateTime lastScannedOn, TimeSpan startTime, TimeSpan endTime, bool readable = false) 
            : this(source, metadata, properties, id, lastScannedOn)
        {
            TrimStart(startTime);
            TrimEnd(endTime);
            _confirmedReadable = readable;
        }

        private readonly ITrack _source;
        private readonly IMetadata? _metadata;
        private readonly IAudioFileProperties _properties;
        private bool? _confirmedReadable;

        public string Uri => _source.Uri;
        public TimeSpan Duration => _properties.Duration;

        public int Id { get; }
        public DateTime LastScannedOn { get; set; }

        public TimeSpan StartTime
        {
            get => _source.StartTime;
            set => TrimStart(value);
        }

        public TimeSpan EndTime
        {
            get => _source.EndTime;
            set => TrimEnd(value);
        }

        public void TrimSilence(float zeroVolume) => _source.TrimSilence(zeroVolume);

        public IMetadata Meta => _metadata ?? _source.Meta;

        public byte[] WaveformData => _source.WaveformData;

        public IObservable<float> WaveformStream() => _source.WaveformStream();
        
        public event EventHandler<EventArgs>? WaveformUpdated;

        public IMetadata Lyrics() => _metadata ?? _source.Lyrics();
        public IAudioFileProperties Properties() => _properties;

        public Stream Notes() => _source.Notes();
        public bool ConfirmedReadable => _confirmedReadable ?? _source.ConfirmedReadable;

        public void CheckIfReadable()
        {
            _source.CheckIfReadable();
            _confirmedReadable = _source.ConfirmedReadable;
        }

        public event EventHandler<EventArgs>? ReadFailed
        {
            add => _source.ReadFailed += value;
            remove => _source.ReadFailed -= value;
        }

        public void TrimStart(TimeSpan newStart) => _source.TrimStart(newStart);

        public void TrimEnd(TimeSpan newEnd) => _source.TrimEnd(newEnd);

        public event EventHandler<EventArgs>? DurationChanged;

        public override string ToString() => $"PersistedSong: {Meta.Title}*{Meta.Artist}";
    }
}
