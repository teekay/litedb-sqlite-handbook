using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MusicLibrary.Persistence.SqliteNet.Model;
using MusicLibrary;
using MusicLibrary.Playlists;
using SQLite;
using Playlist = MusicLibrary.Persistence.SqliteNet.Model.Playlist;

namespace MusicLibrary.Persistence.SqliteNet
{
    public sealed class SqliteNetDbManager : IDBManager, IDeviceCache
    {
        /// <summary>
        /// Initialize this database service
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="composer"></param>
        /// <param name="dbWriter"></param>
        /// <exception cref="ArgumentException">Path to database cannot be empty</exception>
        public SqliteNetDbManager(SQLiteConnection connection, IComposer composer, IDbWriter dbWriter)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            AsyncConnection = new SQLiteAsyncConnection(Connection.DatabasePath);
            Connection.EnableWriteAheadLogging();
            EnsureTablesExist();
            _dbWriter = dbWriter;
            _deviceCache = new DeviceCache(Connection, _dbWriter);
            _servesTracks = new ServesTracks(Connection, composer);
            _servesPlaylists = new ServesPlaylists(Connection, dbWriter);
            Task.Run(RebuildSearchIndex);            
        }

        public SqliteNetDbManager(string databasePath, IComposer composer, IDbWriter dbWriter): this(new SQLiteConnection(databasePath), composer, dbWriter) 
        {
            _ownConnection = true;
        }

        private SQLiteConnection Connection { get; }
        private SQLiteAsyncConnection AsyncConnection { get; }
        private readonly ServesTracks _servesTracks;
        private readonly ServesPlaylists _servesPlaylists;
        private readonly IDeviceCache _deviceCache;
        private readonly IDbWriter _dbWriter;
        private readonly bool _ownConnection = false;

        private void EnsureTablesExist()
        {
            // ReSharper disable once RedundantNameQualifier
            Connection.CreateTable<Model.Track>();
            // ReSharper disable once RedundantNameQualifier
            Connection.CreateTable<Model.Playlist>();
            // ReSharper disable once RedundantNameQualifier
            Connection.CreateTable<Model.OpenPlaylists>();
        }

        private void RebuildSearchIndex()
        {
            Connection.Query<Track>("SELECT * FROM Track WHERE SearchIndex='' OR SearchIndex IS NULL")
                .Select(model => _servesTracks.ModelMappedToITrack(model))
                .Select(track => _servesTracks.MappedToModel(track)) // this is where SearchIndex is populated
                .Iter(AddToCommitQueue);
        }


        public IEnumerable<string> Genres()
        {
            return Connection.QueryScalars<string>(
                    "SELECT DISTINCT Genre FROM Track WHERE Genre IS NOT NULL ORDER BY Genre");
        }

        /// <summary>
        /// Search the database for a track by its path
        /// </summary>
        /// <param name="filepath">Path to track including the filename</param>
        /// <returns>Track if found, otherwise null</returns>
        public Option<ITrack> ByFilename(string filepath) => _servesTracks.ByFilename(filepath); 

        public IEnumerable<ITrack> FromDirectory(string filepath) => _servesTracks.FromDirectory(filepath);
        
        public IEnumerable<ITrack> ByGrouping(string grouping) => _servesTracks.ByGrouping(grouping);

        public async Task<IEnumerable<ITrack>> ByGroupingAsync(string grouping) => await _servesTracks.ByGroupingAsync(AsyncConnection, grouping);

        public IEnumerable<ITrack> FromPaths(IEnumerable<string> filepaths) => _servesTracks.FromPaths(filepaths);

        public IEnumerable<ITrack> Search(string keywords) => _servesTracks.Search(keywords);
        
        public async Task<IEnumerable<ITrack>> SearchAsync(string keywords) => await _servesTracks.SearchAsync(AsyncConnection, keywords);

        public IEnumerable<ITrack> ByGenre(string genreName) => _servesTracks.ByGenre(genreName);
        
        public async Task<IEnumerable<ITrack>> ByGenreAsync(string genreName) => await _servesTracks.ByGenreAsync(AsyncConnection, genreName);
        
        public async Task<IEnumerable<ITrack>> ByGenreAsync(string genreName, int offset, int limit) => await _servesTracks.ByGenreAsync(AsyncConnection, genreName, offset, limit);

        /// <summary>
        /// Save Track info to database. Inserts or updates as necessary.
        /// </summary>
        /// <param name="source">Track to save</param>
        public void Save(ITrack source)
        {
            var model = _servesTracks.MappedToModel(source);
            AddToCommitQueue(model);
        }

        public void Save(IEnumerable<ITrack> tracks)
        {
            var toCreate = new List<Track>();
            var toUpdate = new List<Track>();
            foreach (var track in tracks)
            {
                var model = _servesTracks.MappedToModel(track);
                if (model.Id == 0)
                {
                    toCreate.Add(model);
                }
                else
                {
                    toUpdate.Add(model);
                }
            }

            _dbWriter.Commit(() =>
            {
                Connection.InsertAll(toCreate);
                Connection.UpdateAll(toUpdate);
            });
        }

        private void AddToCommitQueue(Track model)
        {
            _dbWriter.Commit(model.Id == 0
                ? (Action) (() => Connection.Insert(model))
                : () => Connection.Update(model));
        }

        /// <summary>
        /// Remove track from the database
        /// </summary>
        /// <param name="path"></param>
        public void Forget(string path)
        {
            //ForgetUsingSql(path);
            ForgetUsingTableMethods(path);
        }

        internal void ForgetUsingSql(string path)
        {
            _servesTracks.InDirectory(path).ToList()
            .ForEach(found =>
            {
                _dbWriter.Commit(() => {
                    Connection.Execute(@"delete from PlaylistTracks where TrackId=?", found.Id);
                    Connection.Execute(@"delete from Track where Id=?", found.Id);
                });
            });
        }

        internal void ForgetUsingTableMethods(string path)
        {
            _servesTracks.InDirectory(path).ToList()
            .ForEach(found =>
            {
                _dbWriter.Commit(() => {
                    var trackInPlaylists = Connection.Table<PlaylistTrack>().Where(pt => pt.TrackId == found.Id).ToList();
                    trackInPlaylists.ForEach(pt => Connection.Delete<PlaylistTrack>(pt.Id));
                    Connection.Delete<Track>(found.Id);
                });
            });
        }

        public void Forget(IPersistedPlaylist playlist) => _servesPlaylists.Forget(playlist);

        /// <summary>
        /// Remove track from the database
        /// </summary>
        /// <param name="source"></param>
        public void Forget(ITrack source)
        {
            Forget(source.Uri);
        }

        public void Forget(IEnumerable<ITrack> tracks)
        {
            var paths = tracks.Select(t => t.Uri).ToList();
            _dbWriter.Commit(() =>
            {
                foreach (var chunk in paths.Chunk(50))
                {
                    var @params = chunk.ToArray();
                    var trackIds = Connection.QueryScalars<int>($"select Id from Track where Filepath in ({string.Join(", ", chunk.Select(p => @"?"))})", @params).Select(id => (object)id).ToArray();
                    var deleteFromPlaylistSql = $"delete from PlaylistTracks where TrackId in ({string.Join(", ", trackIds.Select(p => @"?"))})";
                    Connection.Execute(deleteFromPlaylistSql, trackIds);
                    Connection.Execute($"delete from Track where Id in ({string.Join(", ", trackIds.Select(p => @"?"))})", trackIds);
                }
            });
        }

        public IEnumerable<IPersistedPlaylist> AllPlaylists() => _servesPlaylists.AllPlaylists();

        public IPersistedPlaylist GetOrCreate(string path) => _servesPlaylists.ExistingOrCreated(path);
        
        public IPersistedPlaylist? ByPlaylistId(int id) => 
            Connection.Table<Playlist>().FirstOrDefault(p => p.Id == id);

        public IEnumerable<(int PlaylistId, string? SearchTerm)> PlaylistSession() => 
            Connection.Table<OpenPlaylists>().Select(p => (p.PlaylistId, p.SearchTerm));

        public void PlaylistSession(IEnumerable<(int PlaylistId, string SearchTerm)> openPlaylists)
        {
            _dbWriter.Commit(() =>
            {
                var existing = Connection.Table<OpenPlaylists>().ToList();
                existing.Where(e => 
                        openPlaylists.All(op => op.PlaylistId != e.PlaylistId))
                    .Iter(p => Connection.Delete<OpenPlaylists>(p.Id));
                var toSave = openPlaylists.Where(op => 
                        existing.All(e => e.PlaylistId != op.PlaylistId))
                    .Select(p => new OpenPlaylists
                {
                    PlaylistId = p.PlaylistId,
                    SearchTerm = !string.IsNullOrEmpty(p.SearchTerm)
                        ? p.SearchTerm
                        : null
                });
                Connection.InsertAll(toSave);
            });
        }

        /// <summary>
        /// Save this playlist
        /// </summary>
        /// <exception cref="ArgumentException">Playlist filename cannot be null</exception>
        public void Save(IPlaylist plist)
        {
            var model = ServesPlaylists.PreSaved(plist);
            _dbWriter.Commit(model.Id == 0
                    ? (Action)(() => Connection.Insert(model))
                    : () => Connection.Update(model));
        }

        public void PopulatePlaylist(IPersistedPlaylist playlist, IEnumerable<ITrack> tracks)
        {
            var workingSet = tracks.ToList();
            var sortOrder = workingSet.Select((t, i) => (i, t.Uri));
            // fetch Tracks that exist in DB
            var existing = _servesTracks.ModelsFromPaths(workingSet.Select(t => t.Uri)).ToList();
            var toCreate = workingSet.Where(t => 
                    !existing.Any(e => e.Uri == t.Uri))
                .Select(t => _servesTracks.MappedToModel(t))
                .ToList();
            _dbWriter.Commit(() =>
            {
                var sortedTracks = existing;
                // insert those that do not
                if (toCreate.Count > 0)
                {
                    Connection.InsertAll(toCreate);
                    var created = _servesTracks.ModelsFromPaths(toCreate.Select(t => t.Uri!)).ToList();
                    var allTracks = existing.Concat(created);
                    // sort by the original order
                    sortedTracks = sortOrder.Select(tup => allTracks.First(t => t.Uri == tup.Uri)).ToList();
                }
                // then populate the playlist
                _servesPlaylists.Populate(playlist, sortedTracks);
            });
        }

        public IEnumerable<ITrack> Contents(IPersistedPlaylist playlist) => _servesTracks.Contents(playlist);

        public void Dispose()
        {
            AsyncConnection.CloseAsync();
            if (_ownConnection)
            {
                Connection.Close();
            }
        }

        public string DeviceName(object id) => _deviceCache.DeviceName(id);

        public void Cache(object id, string name) => _deviceCache.Cache(id, name);
    }
}
