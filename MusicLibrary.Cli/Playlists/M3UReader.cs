using System;
using System.Collections.Generic;
using System.IO;
using MusicLibrary.Playlists;

namespace MusicLibrary.Cli.Playlists
{
    /// <summary>
    /// Read M3U files
    /// </summary>
    public sealed class M3UReader : IPlaylistReader
    {
        /// <summary>
        /// Reads lines (filenames) from a M3U playlist file, or really any text file that has one filename per row.
        /// Does not do any verification.
        /// </summary>
        /// <param name="playlistPath"></param>
        public M3UReader(string playlistPath)
        {
            _playlistPath = playlistPath ?? throw new ArgumentNullException(nameof(playlistPath));
            _playlistDir = Path.GetDirectoryName(_playlistPath) ?? string.Empty;
        }

        private readonly string _playlistPath;
        private readonly string _playlistDir;

        /// <summary>
        /// Read an M3U file and return a list of paths to supported files
        /// </summary>
        /// <returns>Lists of file names that are supported and exist</returns>
        public IEnumerable<string> Filenames()
        {
            if (!File.Exists(_playlistPath)) yield break;
            StreamReader readsFromMaster;
            try
            {
                readsFromMaster = File.OpenText(_playlistPath);
            }
            catch
            {
                yield break;
            }
            string? fileName;
            while ((fileName = readsFromMaster.ReadLine()) != null)
            {
                var pathOrNothing = FilenameUnlessComment(fileName);
                if (pathOrNothing == string.Empty) continue;
                yield return RelativeToAbsolute(pathOrNothing);                
            }
            readsFromMaster.Dispose();
        }

        private string RelativeToAbsolute(string path)
        {
            if (Path.IsPathFullyQualified(path)) return path;
            var absolutePath = Path.Combine(_playlistDir, path);
            return File.Exists(absolutePath)
                ? absolutePath
                : path;
        }

        private static string FilenameUnlessComment(string fileName)
        {
            fileName = fileName.Trim();
            return string.IsNullOrEmpty(fileName) ||
                   string.IsNullOrWhiteSpace(fileName) ||
                   fileName.StartsWith("#")
                ? string.Empty
                : fileName;
        }
    }
}