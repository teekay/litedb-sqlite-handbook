using System;
using System.Collections.Generic;
using MusicLibrary.Playlists;

namespace MusicLibrary.Events
{
    public interface IPlaylistItemSecondarySource
    {
        event EventHandler<IEnumerable<IPlaylistItem>> Enqueued;
    }
}
