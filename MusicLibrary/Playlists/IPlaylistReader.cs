using System.Collections.Generic;

namespace MusicLibrary.Playlists
{
    public interface IPlaylistReader
    {
        IEnumerable<string> Filenames();
    }
}
