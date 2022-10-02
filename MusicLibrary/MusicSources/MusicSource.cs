using System;
using System.Collections.Generic;
using System.Linq;
using MusicLibrary.Playlists;
using static LanguageExt.Prelude;

namespace MusicLibrary.MusicSources
{
    /// <summary>
    /// Positional music source with a few decorating properties and convenience methods.
    /// </summary>
    public sealed class MusicSource : BaseMusicSource, IMusicSource
    {
        // ReSharper disable once RedundantBaseConstructorCall
        public MusicSource() : base()
        {
        }

        public MusicSource(bool isCortinaSource = false)
        {
            IsCortinaSource = isCortinaSource;
        }

        public MusicSource(bool isCortinaSource, params ITrack[] bareTracks) : this(bareTracks)
        {
            IsCortinaSource = isCortinaSource;
        }

        public MusicSource(params ITrack[] bareTracks) : this(bareTracks.Map(PlaylistItem.Of))
        {
        }

        public MusicSource(params IPlaylistItem[] playlistItems) : this(playlistItems.ToList())
        {
        }

        public MusicSource(IEnumerable<IPlaylistItem> seed) : base(seed)
        {
            ReIndex();
        }

        public MusicSource(bool isCortinaSource, IEnumerable<IPlaylistItem> seed) : base(seed)
        {
            IsCortinaSource = isCortinaSource;
            ReIndex();
        }

        public MusicSource(bool isCortinaSource, params IPlaylistItem[] seed) : base(seed)
        {
            IsCortinaSource = isCortinaSource;
            ReIndex();
        }

        public event EventHandler<RangeChangeEventArgs<IPlaylistItem>>? Emptied;
        public event EventHandler<RangeChangeEventArgs<IPlaylistItem>>? AddedRange;
        public event EventHandler<RangeChangeEventArgs<IPlaylistItem>>? InsertedRange;
        public event EventHandler<RangeChangeEventArgs<IPlaylistItem>>? RemovedRange;

        private readonly object _reindexLock = new object();

        /// <summary>
        /// Whether this is a source of cortinas. Smells funny.
        /// </summary>
        public bool IsCortinaSource { get; }

        /// <summary>
        /// Provides a grand total of remaining time in the playlist
        /// not taking into account cortina length
        /// and current position in playback, of which it knows nothing.
        /// </summary>
        public TimeSpan TotalTimeLeft => Count == 0 
            ? TimeSpan.Zero
            : TimeSpan.FromTicks((Current == null ? this : this.Skip(IndexOf(Current)))
              .Select(pt => pt.EndTime > TimeSpan.Zero ? pt.EndTime.Ticks : pt.Duration.Ticks)
              .Aggregate((a,b) => a+b));


        private void ReIndexMember(int index) => this[index].Index = index + 1;

        /// <summary>
        /// A hack to keep track of "index" numbers of containing elements for display in the UI.
        /// </summary>
        private void ReIndex()
        {
            lock (_reindexLock)
            {
                Range(0, Count).Iter(ReIndexMember);
            }
        }

        /// <summary>
        /// Dubious method of questionable value. Clears Current.
        /// </summary>
        public void Unload() => Current = null;

        /// <summary>
        /// Resets Current to the first item on the playlist, if any.
        /// </summary>
        public void Reload()
        {
            if (Count == 0)
                return;
            Current = this[0];
        }

        /// <summary>
        /// Internal callback used after new items are added.
        /// </summary>
        /// <param name="value"></param>
        protected override void Added(IPlaylistItem value)
        {
            base.Added(value);
            ReIndex();
            Current ??= value;
            NotifyOfPropertyChanged(nameof(Forthcoming));
            NotifyOfPropertyChanged(nameof(Count));
        }

        protected override void AddedMany(IList<IPlaylistItem> values)
        {
            base.AddedMany(values);
            Added(values.First());
            Notify();
            AddedRange?.Invoke(this, new RangeChangeEventArgs<IPlaylistItem>(values));
        }

        protected override void InsertedMany(IList<IPlaylistItem> values)
        {
            base.InsertedMany(values);
            Notify();
            InsertedRange?.Invoke(this, new RangeChangeEventArgs<IPlaylistItem>(values));
        }

        protected override void RemovedMany(IList<IPlaylistItem> values)
        {
            base.RemovedMany(values);
            Notify();
            RemovedRange?.Invoke(this, new RangeChangeEventArgs<IPlaylistItem>(values));
        }

        /// <summary>
        /// Internal callback used after new items are removed.
        /// </summary>
        /// <param name="value"></param>
        protected override void Removed(IPlaylistItem value)
        {
            base.Removed(value);
            ReIndex();
            if (value == Current)
            {
                Current = Count == 0
                    ? null
                    : this[0];
            }
            Notify();
        }

        /// <summary>
        /// Internal callback used after new items are all removed.
        /// </summary>
        protected override void Cleared(IList<IPlaylistItem> cleared)
        {
            base.Cleared(cleared);
            Current = null;
            ToBeSkippedTo = null;
            NotifyOfPropertyChanged(nameof(Forthcoming));
            NotifyOfPropertyChanged(nameof(Count));
            Emptied?.Invoke(this, new RangeChangeEventArgs<IPlaylistItem>(cleared));
        }

        public override string ToString() => $"MusicSource with {Count} tracks";

        private void Notify()
        {
            NotifyOfPropertyChanged(nameof(Current));
            NotifyOfPropertyChanged(nameof(Forthcoming));
            NotifyOfPropertyChanged(nameof(Count));
        }
    }


    public class RangeChangeEventArgs<T> : EventArgs
    {
        public RangeChangeEventArgs(IList<T> items)
        {
            Items = items;
        }

        public IList<T> Items { get; }
    }
}