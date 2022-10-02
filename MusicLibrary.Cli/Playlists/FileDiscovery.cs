#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LanguageExt;
using MusicLibrary.Metadata.Meta;
using MusicLibrary.Playlists;

namespace MusicLibrary.Cli.Playlists
{
    public sealed class FileDiscovery : IFileDiscovery
    {
        public FileDiscovery(ISupportedMediaAuthority supportedMediaAuthority)
        {
            _mediaAuthority = supportedMediaAuthority ?? throw new ArgumentNullException(nameof(supportedMediaAuthority));
        }

        private readonly ISupportedMediaAuthority _mediaAuthority;

        /// <summary>
        /// Method accepts file names and does the following:
        /// 1) Discovers files in directories
        /// 2) Discovers files in M3U playlists
        /// 3) Filters and returns only those that are supported
        /// </summary>
        /// <param name="paths"></param>
        public IEnumerable<string> Discover(IEnumerable<string> paths) => paths.SelectMany(Discover);

        public IEnumerable<string> Discover(string path)
        {
            return Directory.Exists(path)
                ? FromDirectory(path)
                : IsM3U(path)
                    ? AllFromM3UPlaylist(path)
                    : _mediaAuthority.IsFileExtensionSupported(path)
                        ? new List<string>(1) {path}
                        : new Lst<string>();
        }

        private IEnumerable<string> FromDirectory(string path)
        {
            return Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)
                .Where(filename => _mediaAuthority.IsFileExtensionSupported(filename))
                .Select(filename => Path.Combine(path, filename));
        }

        private IEnumerable<string> AllFromM3UPlaylist(string path)
        {
            return new SupportedFilenamesFromPlaylist(
                    new ReadsFilenamesAlsoFromEmbeddedM3u(
                        new M3UReader(path),
                        _mediaAuthority),
                _mediaAuthority).Filenames();
        }

        private bool IsM3U(string path) => path.EndsWith(".m3u", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase);

    }
}
