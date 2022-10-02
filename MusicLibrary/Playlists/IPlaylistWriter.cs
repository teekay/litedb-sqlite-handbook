using System.Collections.Generic;

namespace MusicLibrary.Playlists
{
    public interface IPlaylistWriter
    {
        bool Write(IEnumerable<string> filePaths, string playlistFilename);
    }
}
