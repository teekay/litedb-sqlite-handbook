using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using MusicLibrary.Playlists;

namespace MusicLibrary.Cli.Playlists
{
    /// <summary>
    /// Writes m3u files
    /// </summary>
    public class M3UWriter : IPlaylistWriter
    {
        /// <summary>
        /// Write a list of file paths to a M3U file
        /// </summary>
        /// <param name="filePaths"></param>
        /// <param name="playlistFilename"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="filePaths"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="playlistFilename"/> is <see langword="null" />.</exception>
        public bool Write(IEnumerable<string> filePaths, string playlistFilename)
        {
            if (filePaths == null) throw new ArgumentNullException(nameof(filePaths));
            if (playlistFilename == null) throw new ArgumentNullException(nameof(playlistFilename));
            try
            {
                File.WriteAllLines(playlistFilename, filePaths, Encoding.UTF8);
                return true;
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (IOException)
            {
            }
            catch (SecurityException)
            { 
            }
            catch (UnauthorizedAccessException)
            {
            }
            return false;
        }
    }
}
