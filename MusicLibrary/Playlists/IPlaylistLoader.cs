using System.Collections.Generic;

namespace MusicLibrary.Playlists
{
    public interface IPlaylistLoader
    {
        /// <summary>
        /// Reads a stored playlist, with the help of a provided IPlaylistReader,
        /// and return a list of tracks that have been verified to the extent
        /// that we can read metadata from them.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IEnumerable<ITrack> Playlist(string path);
        IEnumerable<ITrack> Playlist(IEnumerable<string> paths);
    }
}