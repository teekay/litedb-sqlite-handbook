using System;
using System.Diagnostics;
using System.IO;
using MusicLibrary.Metadata;
using MusicLibrary.Playlists;

namespace MusicLibrary
{
    /// <summary>
    /// Class to wrap Track instances so that one track can be present in the playlist more than once.
    /// This is a bindable object without a corresponding model class or any other Dto.
    /// </summary>
    [DebuggerDisplay("PlaylistItem: {Meta.Title}*{Meta.Artist}")]
    public class PlaylistItem : IPlaylistItem
    {
        /// <summary>
        /// Initialize with a Track
        /// </summary>
        /// <param name="track"></param>
        public PlaylistItem(ITrack track)
        {
            SourceTrack = track ?? throw new ArgumentException("Track cannot be null");
            SourceTrack.DurationChanged += OnTrackDurationChanged;
            SourceTrack.WaveformUpdated += OnTrackWaveformUpdated;
        }

        public static PlaylistItem Of(ITrack source) => new PlaylistItem(source);

        public PlaylistItem(ITrack track, bool isCortina) : this(track)
        {
            IsCortina = isCortina;
        }

        protected readonly ITrack SourceTrack;

        /// <summary>
        /// Position of this playlist item within the playlist
        /// </summary>
        public virtual int Index { get; set; }

        /// <summary>
        /// The actual Track
        /// </summary>

        /// <summary>
        /// The item is selected in GUI for some operation such as delete, move...
        /// </summary>
        public virtual bool IsSelected { get; set; }

        /// <summary>
        /// Tracks that are readable / playable have this set to true, which is the default.
        /// </summary>
        public virtual bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Set this flag to indicate whether the playlist item is enqueued in a playlist
        /// </summary>
        public virtual bool IsEnqueued { get; set; }

        /// <summary>
        /// Flag to mark the track as cortina (which may be handled differently by the consumer)
        /// </summary>
        public bool IsCortina { get; }

        /// <summary>
        /// Tempo adjustment in % of the original
        /// </summary>
        public virtual int TempoAdjustment { get; set; } = 100;

        /// <summary>
        /// The resulting BPM as adjusted by the DJ
        /// </summary>
        public int AdjustedBPM => (int) Math.Round(SourceTrack.Meta.BPM * (TempoAdjustment / 100d), 0);

        /// <summary>
        /// Get the Track length adjusted for the BPM % change
        /// </summary>
        public TimeSpan AdjustedDuration => TimeSpan.FromTicks((long) (SourceTrack.Duration.Ticks / (TempoAdjustment / 100.0d)));

        public virtual IPlaylistItem Clone()
        {
            return new PlaylistItem(this.SourceTrack, this.IsCortina);
        }

        /// <summary>
        /// Pitch adjustment - 1.0f is the original
        /// </summary>
        public float PitchAdjustment { get; set; } = 0.0f;

        // ITrack properties, not really needed
        public string Uri => SourceTrack.Uri;

        public virtual TimeSpan StartTime
        {
            get => SourceTrack.StartTime;
            set => TrimStart(value);
        }

        public virtual TimeSpan EndTime
        {
            get => SourceTrack.EndTime;
            set => TrimEnd(value);
        }

        public TimeSpan Duration => SourceTrack.Duration;

        public void TrimSilence(float zeroVolume = 0.01f) => SourceTrack.TrimSilence(zeroVolume);

        public IMetadata Meta => SourceTrack.Meta;

        public byte[] WaveformData => SourceTrack.WaveformData;
        public event EventHandler<EventArgs>? WaveformUpdated;

        protected virtual void OnTrackWaveformUpdated(object sender, EventArgs e) => WaveformUpdated?.Invoke(this, e);
        protected virtual void OnTrackDurationChanged(object sender, EventArgs e) => DurationChanged?.Invoke(this, e);

        public IMetadata Lyrics() => SourceTrack.Lyrics();
        public IAudioFileProperties Properties() => SourceTrack.Properties();

        public Stream Notes() => SourceTrack.Notes();
        public bool ConfirmedReadable => SourceTrack.ConfirmedReadable;

        public void CheckIfReadable() => SourceTrack.CheckIfReadable();

        public event EventHandler<EventArgs>? ReadFailed
        {
            add => SourceTrack.ReadFailed += value;
            remove => SourceTrack.ReadFailed -= value;
        }

        public event EventHandler<EventArgs>? DurationChanged;

        public void TrimStart(TimeSpan newStart) => SourceTrack.TrimStart(newStart);

        public void TrimEnd(TimeSpan newEnd) => SourceTrack.TrimEnd(newEnd);

        public IObservable<float> WaveformStream() => SourceTrack.WaveformStream();
    }
}