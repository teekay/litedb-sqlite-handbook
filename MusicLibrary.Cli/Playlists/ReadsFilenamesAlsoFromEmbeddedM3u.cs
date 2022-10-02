using System;
using System.Collections.Generic;
using MusicLibrary.Metadata.Meta;
using MusicLibrary.Playlists;

namespace MusicLibrary.Cli.Playlists
{
    public sealed class ReadsFilenamesAlsoFromEmbeddedM3u: IPlaylistReader
    {
        public ReadsFilenamesAlsoFromEmbeddedM3u(M3UReader source, ISupportedMediaAuthority authority)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _authority = authority ?? throw new ArgumentNullException(nameof(authority));
        }

        private readonly M3UReader _source;
        private readonly ISupportedMediaAuthority _authority;

        public IEnumerable<string> Filenames()
        {
            foreach (var path in _source.Filenames())
            {
                if (!_authority.IsPlaylistExtensionSupported(path))
                {
                    yield return path;
                    continue;
                }

                var reader = new M3UReader(path);
                foreach (var embeddedPath in reader.Filenames())
                {
                    yield return embeddedPath;
                }
            }
        }
    }
}
