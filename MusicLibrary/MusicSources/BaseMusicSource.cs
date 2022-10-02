using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MusicLibrary.Playlists;

namespace MusicLibrary.MusicSources
{
    /// <summary>
    /// Notifying collection that keeps track of current, previous, and next item.
    /// </summary>
    public abstract class BaseMusicSource : NotifyingCollection<IPlaylistItem>,
        INotifyPropertyChanged, 
        IPositionAwarePlaylist
    {
        protected BaseMusicSource()
        {
        }

        protected BaseMusicSource(IEnumerable<IPlaylistItem> seed) : base(seed)
        {
            _current = Count > 0 ? this[0] : null;
        }

        private IPlaylistItem? _current;
        private IPlaylistItem? _scheduledTrack;

        public IPlaylistItem? Previous => Current == null || IndexOf(Current) <= 0
            ? null
            : this[IndexOf(Current) - 1];

        public IPlaylistItem? Current
        {
            get => _current;
            protected set
            {
                _current = value;
                if (value != null && !Contains(value))
                {
                    throw new InvalidOperationException("Weird: Current not part of collection!");
                }

                NotifyOfPropertyChanged(nameof(Current));
                NotifyOfPropertyChanged(nameof(Previous));
                NotifyOfPropertyChanged(nameof(Forthcoming));
            }
        }

        public IPlaylistItem? Forthcoming
        {
            get
            {
                if (Count == 0 || Current == null || IndexOf(Current) == (Count - 1))
                {
                    return null;
                }

                var startIndex = IndexOf(Current) + 1;
                return this.Skip(startIndex).FirstOrDefault(t => t.IsEnabled);
            }
        }

        public IPlaylistItem? ToBeSkippedTo
        {
            get => _scheduledTrack;
            protected set
            {
                if (value != null && !Contains(value))
                    throw new InvalidOperationException("Argument not present in the collection");
                if (value != null && !value.IsEnabled)
                {
#if DEBUG
                    throw new InvalidOperationException("Selected item is not enabled");
#else
                    return;
#endif
                }
                _scheduledTrack = value;
                NotifyOfPropertyChanged();
            }
        }

        public void SkipTo(IPlaylistItem playlistItem)
        {
            ToBeSkippedTo = playlistItem ?? throw new ArgumentNullException(nameof(playlistItem));
        }

        public void CancelSkip()
        {
            ToBeSkippedTo = null;
        }

        public void Next()
        {
            Current = ToBeSkippedTo ?? Forthcoming;
            ToBeSkippedTo = null;
        }

        public override void RemoveRange(IEnumerable<IPlaylistItem> playlistItems)
        {
            var source = playlistItems.ToList();
            var toRemove = Current == null ? source : source.Except(new[] {Current});
            base.RemoveRange(toRemove);
            if (Current != null && source.Contains(Current)) 
            {
                // remove it last, otherwise the MusicSource would try to re-set its Current property every time it loses it
                Remove(Current);
            }
            if (source.Count > 0)
            {
                Removed(source.Last());
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotifyOfPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
