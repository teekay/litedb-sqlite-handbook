using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using MusicLibrary.Playlists;

namespace MusicLibrary.MusicSources
{
    public interface IMusicSourceInfo : IPositionAwarePlaylist, INotifyPropertyChanged, INotifyCollectionChanged, IReadOnlyList<IPlaylistItem>
    {
        /// <summary>
        /// Is this playlist a source of cortinas?
        /// </summary>
        bool IsCortinaSource { get; }

        /// <summary>
        /// Convenience method that returns the computed total
        /// of duration of all tracks contained herein.
        /// </summary>
        TimeSpan TotalTimeLeft { get; }

        /// <summary>
        /// Clears the Current property, and consequently also Previous and Forthcoming
        /// </summary>
        void Unload();

        /// <summary>
        /// Returns to the first track in the playlist
        /// </summary>
        void Reload();

        /// <summary>
        /// Advances to the next item
        /// </summary>
        void Next();

        /// <summary>
        /// Cancelles the request to skip to another track
        /// TODO does it belong here?
        /// </summary>
        void CancelSkip();
    }
}
