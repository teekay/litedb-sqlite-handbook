using System;
using MusicLibrary.Playlists;

namespace MusicLibrary.MusicSources
{
    public interface IMusicSource : INotifyingCollection<IPlaylistItem>, IMusicSourceInfo
    {
        void Replace(IPlaylistItem source, IPlaylistItem target);
        
        /// <summary>
        /// Invoked when the entire playlist is cleared
        /// </summary>
        event EventHandler<RangeChangeEventArgs<IPlaylistItem>>? Emptied;

        /// <summary>
        /// Invoked when many items are added at once
        /// </summary>
        event EventHandler<RangeChangeEventArgs<IPlaylistItem>>? AddedRange;

        /// <summary>
        /// Invoked when many items are inserted at once
        /// </summary>
        event EventHandler<RangeChangeEventArgs<IPlaylistItem>>? InsertedRange;

        /// <summary>
        /// Invoked when many items are removed at once
        /// </summary>
        event EventHandler<RangeChangeEventArgs<IPlaylistItem>>? RemovedRange;
    }
}
