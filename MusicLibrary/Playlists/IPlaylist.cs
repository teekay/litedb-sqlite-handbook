using System.Collections.Generic;
using MusicLibrary.MusicSources;

namespace MusicLibrary.Playlists
{
    public interface IPlaylist: IPlaylistInfo
    {
        /// <summary>
        /// A playlist has some contents - duh
        /// </summary>
        IMusicSourceInfo Contents { get; }

        IList<IPlaylistItem> SelectedItems();
        void Deselect();

        void AddOrInsert(IEnumerable<IPlaylistItem> playlistItems, int moveToIndex);
        void Replace(IPlaylistItem source, IPlaylistItem replacement);

        void Import(IEnumerable<IPlaylistItem> playlistItems);
        void MoveMany(IEnumerable<IPlaylistItem> playlistItems, int moveToIndex);
        void CloneMany(IEnumerable<IPlaylistItem> playlistItems, int moveToIndex);
        void ClearPlaylist();
        void SelectAll();
    }
}