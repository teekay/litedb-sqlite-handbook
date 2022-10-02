#nullable enable
using MusicLibrary;
using MusicLibrary.Metadata;
using MusicLibrary.Playlists;

//using MoreLinq;

namespace MusicLibrary.Cli.Playlists
{
    internal sealed class PlaylistFromFiles
    {
        public PlaylistFromFiles(IDBManager dataService)
        {
            _dataService = dataService;
            _fileDiscovery = new FileDiscovery(_authority);
        }


        public PlaylistFromFiles(IDBManager dataService, IFileDiscovery fileDiscovery)
        : this(dataService)
        {
            _fileDiscovery = fileDiscovery;
        }

        private readonly IDBManager _dataService;
        private readonly IFileDiscovery _fileDiscovery;
        private readonly SupportedFilesAuthority _authority = new();
        private readonly Dictionary<string, ITrack> _tracksInDb = new ();

        public IEnumerable<IPlaylistItem> Playlist(params string[] paths)
        {
            _tracksInDb.Clear();
            foreach (var dbPlaylist in paths.Where(_authority.IsPlaylistExtensionSupported)
                .Select(_dataService.GetOrCreate))
            {
                _dataService.Contents(dbPlaylist)
                    .DistinctBy(t => t.Uri)
                    .Iter(track => _tracksInDb.Add(track.Uri, track));
            }
            return FilepathsToPlaylistItems(_fileDiscovery.Discover(paths));
        }

        public async Task<IEnumerable<IPlaylistItem>> PlaylistAsync(params string[] paths) =>
            await Task.Run( () => Playlist(paths));

        /// <summary>
        /// Helper method to create Track instances from the provided import list.
        /// </summary>
        /// <param name="listOfFilePaths"></param>
        private IEnumerable<IPlaylistItem> FilepathsToPlaylistItems(IEnumerable<string> listOfFilePaths) =>
            listOfFilePaths.Select(OnlyTracksWithMetadata);

        private IPlaylistItem OnlyTracksWithMetadata(string path)
        {
            IMetadataSource? metaSource = null;
            _tracksInDb.TryGetValue(path, out var maybeCachedTrack);
            if (maybeCachedTrack != null)
            {
                return new PlaylistItem(maybeCachedTrack);
            }

            var maybeFromDb = _dataService.ByFilename(path);
            var notFound = false;
            
            maybeFromDb.WhenNone(() =>
            {
                metaSource = MetaSourceFromFile(path, ref notFound);
            });

            if (notFound)
            {
                // Bad outcome #1 - file does not exist
                return new PlaylistItem(new TrackStub(path, new MetadataSource()));
            }
            if (metaSource?.Lyrics() is EmptyMetadata)
            {
                // Bad outcome #2 - cannot read metadata from the file
                return new PlaylistItem(new TrackStub(path, new MetadataSource()));
            }
            if (metaSource != null && string.IsNullOrEmpty(metaSource.Lyrics().Title))
            {
                // Try to guess the title from the file name and keep all other properties
                metaSource = new GuessingMetadataSource(path, metaSource.Lyrics(), metaSource.Properties());
            }

            return new PlaylistItem(maybeFromDb.IfNone(() => new TrackStub(path, metaSource!)));
        }

        private static IMetadataSource MetaSourceFromFile(string path, ref bool notFound)
        {
            try
            {
                return new MetadataSource(new LocalFileAbstraction(path));
            }
            catch
            {
                notFound = true;
                return new GuessingMetadataSource(path);
            }
        }
    }
}