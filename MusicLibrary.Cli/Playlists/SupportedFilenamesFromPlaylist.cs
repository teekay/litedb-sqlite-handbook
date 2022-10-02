#nullable enable
using System;
using System.Collections.Generic;
using MusicLibrary.Metadata.Meta;
using MusicLibrary.Playlists;

namespace MusicLibrary.Cli.Playlists
{
    public sealed class SupportedFilenamesFromPlaylist : IPlaylistReader
    {
        public SupportedFilenamesFromPlaylist(IPlaylistReader source, ISupportedMediaAuthority authority)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _authority = authority ?? throw new ArgumentNullException(nameof(authority));
        }

        private readonly IPlaylistReader _source;
        private readonly ISupportedMediaAuthority _authority;

        public IEnumerable<string> Filenames()
        {
            foreach (var path in _source.Filenames())
            {
                if (_authority.IsFileExtensionSupported(path))
                {
                    yield return path;
                }
            }
        }
    }
}
