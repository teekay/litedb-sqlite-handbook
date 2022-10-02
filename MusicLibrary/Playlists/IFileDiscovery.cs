using System.Collections.Generic;

namespace MusicLibrary.Playlists
{
    public interface IFileDiscovery
    {
        /// <summary>
        /// Method accepts file names and does the following:
        /// 1) Discovers files in directories
        /// 2) Discovers files in M3U playlists
        /// 3) Filters and returns only those that are supported
        /// </summary>
        /// <param name="paths"></param>
        IEnumerable<string> Discover(IEnumerable<string> paths);

        /// <summary>
        /// Discover tracks from a single path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IEnumerable<string> Discover(string path);
    }
}