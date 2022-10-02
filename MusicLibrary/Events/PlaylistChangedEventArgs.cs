using System;
using System.Collections.Generic;
using MusicLibrary.MusicSources;

namespace MusicLibrary.Events
{
    public class PlaylistChangedEventArgs : EventArgs
    {
        public PlaylistChangedEventArgs(IMusicSourceInfo source, PlaylistAction action, IEnumerable<ITrack> affectedTracks)
        {
            Source = source;
            Action = action;
            AffectedTracks = affectedTracks;
        }

        public IMusicSourceInfo Source { get; }
        public PlaylistAction Action { get; }
        public IEnumerable<ITrack> AffectedTracks { get; }
    }
}
