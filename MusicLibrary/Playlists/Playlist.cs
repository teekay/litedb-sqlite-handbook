using System;
using System.Collections.Generic;
using System.Linq;
using MusicLibrary.MusicSources;

namespace MusicLibrary.Playlists
{
    public class Playlist : IPlaylist
    {
        public Playlist()
        {
            Uri = string.Empty;
            MusicSource = new MusicSource();
            _sourceWithCount = MusicSource;
        }

        public Playlist(string uri) : this()
        {
            Uri = uri;
        }

        public Playlist(IMusicSource contents) : this()
        {
            MusicSource = contents;
            _sourceWithCount = MusicSource;
        }

        public Playlist(string uri, IMusicSource contents) : this(contents)
        {
            Uri = uri;
        }

        public string Uri { get; }
        public virtual string Comment { get; set; } = string.Empty;
        public IMusicSourceInfo Contents => MusicSource;

        protected readonly IMusicSource MusicSource;

        private readonly ICollection<IPlaylistItem> _sourceWithCount;

        public IList<IPlaylistItem> SelectedItems() => (from item in MusicSource where item.IsSelected select item).ToList();

        public void Deselect() => SelectedItems().Iter(pi => pi.IsSelected = false);

        public void Replace(IPlaylistItem source, IPlaylistItem replacement) => MusicSource.Replace(source, replacement);

        public void Import(IEnumerable<IPlaylistItem> playlistItems)
        {
            if (playlistItems == null) throw new ArgumentNullException(nameof(playlistItems));
            MusicSource.AddRange(playlistItems);
        }

        private bool AreUnknown(IEnumerable<IPlaylistItem> playlistItems) =>
            playlistItems.Any(t => MusicSource.IndexOf(t) == -1);

        private void RemoveSingle(IPlaylistItem candidate) => MusicSource.Remove(candidate);

        public void MoveMany(IEnumerable<IPlaylistItem> playlistItems, int moveToIndex)
        {
            if (playlistItems == null) throw new ArgumentNullException(nameof(playlistItems));
            var items = playlistItems.ToList();
            if (_sourceWithCount.Count <= 1 || AreUnknown(items)) return; // nothing to do, cannot move a single item
            var indexOfFirstItem = MusicSource.IndexOf(items.First());
            if (moveToIndex > indexOfFirstItem)
                moveToIndex -= items.Count;
            items.ForEach(RemoveSingle);
            AddOrInsert(items, moveToIndex);
        }

        public void AddOrInsert(IEnumerable<IPlaylistItem> playlistItems, int moveToIndex)
        {
            if (playlistItems == null) throw new ArgumentNullException(nameof(playlistItems));
            var trackList = playlistItems.ToList();
            void AddItems() => MusicSource.AddRange(trackList);
            void InsertItems() => MusicSource.InsertRange(moveToIndex, trackList);
            Action addOrInsert = moveToIndex >= _sourceWithCount.Count
                ? AddItems
                : (Action) InsertItems;
            addOrInsert.Invoke();
        }

        private static IEnumerable<IPlaylistItem> Clone(IEnumerable<IPlaylistItem> playlistItems) => playlistItems.ToList()
            .Select(playlistItem => playlistItem.Clone())
            .ToList();

        public void CloneMany(IEnumerable<IPlaylistItem> playlistItems, int moveToIndex)
        {
            if (playlistItems == null) throw new ArgumentNullException(nameof(playlistItems));
            AddOrInsert(Clone(playlistItems), moveToIndex);
        }

        public virtual void ClearPlaylist()
        {
            MusicSource.Clear();
        }

        public void SelectAll()
        {
            MusicSource.Iter(o => o.IsSelected = true);
        }
    }
}