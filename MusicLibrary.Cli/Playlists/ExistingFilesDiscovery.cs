using System.Collections.Generic;
using System.IO;
using MusicLibrary.Playlists;

namespace Embrace.Playback.Library
{
    public sealed class ExistingFilesDiscovery: IFileDiscovery
    {
        public ExistingFilesDiscovery(IFileDiscovery baseFileDiscovery)
        {
            _baseFileDiscovery = baseFileDiscovery;
        }

        private readonly IFileDiscovery _baseFileDiscovery;

        public IEnumerable<string> Discover(IEnumerable<string> paths) => 
            _baseFileDiscovery.Discover(paths).Filter(File.Exists);

        public IEnumerable<string> Discover(string path) => 
            _baseFileDiscovery.Discover(path).Filter(File.Exists);
    }
}
