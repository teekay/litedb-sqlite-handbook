#nullable enable
using System;
using System.IO;
using MusicLibrary.Metadata;

namespace MusicLibrary
{
    public abstract class Song : ITrack
    {
        protected Song(string uri)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public string Uri { get; }

        public TimeSpan Duration { get; protected set; }

        public TimeSpan StartTime { get; protected set; }
        public abstract void TrimStart(TimeSpan newStart);

        public TimeSpan EndTime { get; protected set; }
        public abstract void TrimEnd(TimeSpan newEnd);

        public abstract void TrimSilence(float zeroVolume);

        public abstract IMetadata Meta { get; protected set; }

        public abstract Stream Notes();

        public abstract bool ConfirmedReadable { get; protected set; }

        public abstract IMetadata Lyrics();
        public abstract IAudioFileProperties Properties();

        public virtual byte[] WaveformData { get; protected set; } = new byte[0];

        public abstract IObservable<float> WaveformStream();
        public event EventHandler<EventArgs>? WaveformUpdated;
        public event EventHandler<EventArgs>? DurationChanged;
        public abstract void CheckIfReadable();
        public event EventHandler<EventArgs>? ReadFailed;

        protected void OnDurationChanged() => DurationChanged?.Invoke(this, EventArgs.Empty);

        protected void OnWaveformUpdated() => WaveformUpdated?.Invoke(this, EventArgs.Empty);

        protected void OnReadFailed() => ReadFailed?.Invoke(this, EventArgs.Empty);
    }
}
