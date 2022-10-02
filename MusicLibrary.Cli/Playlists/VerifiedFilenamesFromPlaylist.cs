#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using MusicLibrary.Playlists;

namespace Embrace.Playback.Library
{
    public sealed class VerifiedFilenamesFromPlaylist: IPlaylistReader
    {
        public VerifiedFilenamesFromPlaylist(IPlaylistReader source, string basePath)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        private readonly IPlaylistReader _source;
        private readonly string _basePath;

        public IEnumerable<string> Filenames()
        {
            foreach (var path in _source.Filenames())
            {
                var maybe = new MaybeFile(path, _basePath).VerifiedOrEmpty();
                if (string.IsNullOrWhiteSpace(maybe)) continue;
                yield return maybe;
            }
        }

        private class MaybeFile
        {
            public MaybeFile(string path, string basePath)
            {
                _path = path;
                _combiPath = basePath == string.Empty ? _path : Path.Combine(basePath, _path);
            }

            private readonly string _path;
            private readonly string _combiPath;

            public string VerifiedOrEmpty()
            {
                return File.Exists(_path)
                    ? _path
                    : File.Exists(_combiPath)
                        ? _combiPath
                        : string.Empty;
            }
        }
    }
}
