#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LiteDB;
using MusicLibrary.Metadata;
using MusicLibrary.Metadata.Meta;
using MusicLibrary.Persistence.LiteDb.Model;
using MusicLibrary.Playlists;
using static LanguageExt.Prelude;
using Playlist = MusicLibrary.Persistence.LiteDb.Model.Playlist;

namespace MusicLibrary.Persistence.LiteDb
{
    public class LiteDbMusicLibrary : IDBManager, IDeviceCache
    {
        public LiteDbMusicLibrary(string pathToDb, IComposer composer, IDbWriter dbWriter)
        {
            _composer = composer;
            _dbWriter = dbWriter;
            db = new LiteDatabase(pathToDb);
            CreateIndices();
            
            BsonMapper.Global.RegisterType<Track>(
                serialize: t => t.AsDocument(),
                deserialize: bson => new Track(bson.AsDocument)
            );
            BsonMapper.Global.Entity<PlaylistTrack>()
                .DbRef(pt => pt.Track, "tracks");

            _tracks = db.GetCollection<Track>("tracks");
            _playlists = db.GetCollection<Playlist>("playlists");
            _openPlaylists = db.GetCollection<OpenPlaylist>("open_playlists");
            _playlistTracks = db.GetCollection<PlaylistTrack>("playlist_tracks");
            _devices = db.GetCollection<CachedDeviceInfo>("cached_devices");
        }

        private readonly IComposer _composer;
        private readonly IDbWriter _dbWriter;
        private readonly LiteDatabase db;
        private readonly ILiteCollection<Track> _tracks;
        private readonly ILiteCollection<Playlist> _playlists;
        private readonly ILiteCollection<OpenPlaylist> _openPlaylists;
        private readonly ILiteCollection<PlaylistTrack> _playlistTracks;
        private readonly ILiteCollection<CachedDeviceInfo> _devices;

        public IEnumerable<string> Genres()
        {
            return db.Execute(@"select distinct(*.Genre) as genres from tracks where $.Genre != null order by $.Genre")
                .Current["genres"]
                .AsArray
                .Select(x => x.AsString);
        }

        public Option<ITrack> ByFilename(string filepath)
        {
            return TrackByUri(filepath).Match(
                y => Optional(MappedToDomain(y)),
                () => Option<ITrack>.None);
        }

        internal Option<Track> TrackByUri(string filepath)
        {
            var path = filepath.ToLowerInvariant();

            return _tracks.FindOne(t => t.Uri == path);
        }

        internal ITrack MappedToDomain(Track model)
        {
            var meta = new SongMetadata(
                Nns(model.Year), Nns(model.Title), Nns(model.Artist), Nns(model.Album),
                Nns(model.Comment), Nns(model.Genre), Nns(model.AlbumArtist), Nns(model.Conductor),
                model.BPM, model.ReplayGain, (uint)Math.Abs(model.Rating), model.Grouping ?? string.Empty);
            return new PersistedSong(
                _composer.SongPlayableOnPlatform(
                    Nns(model.Uri),
                    new CachedMetadataSource(meta, TsFromTicks(model.Duration)),
                    model.WaveformData ?? System.Array.Empty<byte>()),
                meta,
                new TaggedAudioFileProperties(TsFromTicks(model.Duration)),
                model.Id,
                new DateTime(model.LastScannedOn),
                TsFromTicks(model.StartTime),
                TsFromTicks(model.EndTime),
                model.ConfirmedReadable);
        }

        private string Nns(string? v) => v ?? string.Empty;
        private TimeSpan TsFromTicks(long ticks) => TimeSpan.FromTicks(ticks);


        public IEnumerable<ITrack> FromDirectory(string filepath)
        {
            var pathCi = filepath.ToLowerInvariant();

            return _tracks.Find(t => t.Uri != null && t.Uri.StartsWith(pathCi))
                .Select(MappedToDomain);
        }

        public IEnumerable<ITrack> ByGrouping(string grouping)
        {
            return _tracks.Find(t => t.Grouping != null && t.Grouping == grouping)
                .Select(MappedToDomain)
                .OrderBy(t => $"{t.Meta.Artist}{t.Meta.Title}");
        }

        public Task<IEnumerable<ITrack>> ByGroupingAsync(string grouping)
        {
            return Task.Factory.StartNew(() =>
            {
                return ByGrouping(grouping);
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        public IEnumerable<ITrack> FromPaths(IEnumerable<string> filepaths)
        {
            return filepaths.Select(ByFilename).Somes();
        }

        public IEnumerable<ITrack> Search(string keywords)
        {
            var searchCriteria = keywords.ToLower()
                .Split(' ')
                .Filter(IsMinLength)
                .ToList();
            if (searchCriteria.Count == 0)
            {
                return _tracks.FindAll().Select(MappedToDomain);
            }

            var sql = $"select $ from tracks where {string.Join(" and ", searchCriteria.Select(c => $"indexof($.SearchIndex, '{c}') >= 0"))} order by join([$.Genre, $.Artist, $.Title])";
            return db.Execute(sql)
                .ToEnumerable()
                .Select(bsonValue => new Track(bsonValue.AsDocument))
                .Select(MappedToDomain);
        }

        private bool IsMinLength(string s) => s.Length >= 3;

        public Task<IEnumerable<ITrack>> SearchAsync(string keywords)
        {
            return Task.Factory.StartNew(() =>
            {
                return Search(keywords);
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);   
        }

        public IEnumerable<ITrack> ByGenre(string genreName)
        {
            return ByGenreUsingCollectionMethods(genreName);
        }

        internal IEnumerable<ITrack> ByGenreUsingCollectionMethods(string genreName)
        {
            return (string.IsNullOrWhiteSpace(genreName)
                ? _tracks.Query()
                    .Where(t => t.Genre == null || t.Genre == string.Empty || t.Genre == " ")
                : _tracks.Query()
                    .Where(t => t.Genre == genreName))
                .ToList()
                .OrderBy(t => t.Artist)
                .ThenBy(t => t.Title)
                .Select(MappedToDomain);
        }

        internal IEnumerable<ITrack> ByGenreUsingSql(string genreName)
        {
            var condition = string.IsNullOrWhiteSpace(genreName) ? " is null or $.Genre = \"\" or $.Genre = \" \"" : $" = \"{genreName}\"";
            var sql = $"select $ from tracks where $.Genre {condition}";
            return db.Execute(sql)
                .ToEnumerable()
                .Select(doc => new Track(doc.AsDocument))
                .ToList()
                .OrderBy(t => t.Artist)
                .ThenBy(t => t.Title)
                .Select(MappedToDomain);
        }

        public async Task<IEnumerable<ITrack>> ByGenreAsync(string genreName)
        {
            return await Task.Run(() =>
            {
                return ByGenre(genreName);
            });            
        }

        public async Task<IEnumerable<ITrack>> ByGenreAsync(string genreName, int offset, int limit)
        {
            return await Task.Run(() =>
            {
                return ByGenreWithLimitAndOffset(genreName, offset, limit);
            });
        }

        private IEnumerable<ITrack> ByGenreWithLimitAndOffset(string genreName, int offset, int limit)
        {
            return (string.IsNullOrWhiteSpace(genreName)
                ? _tracks.Query()
                    .Where(t => t.Genre == null || t.Genre == string.Empty)
                : _tracks.Query()
                    .Where(t => t.Genre != null && t.Genre == genreName))
                .OrderBy(t => $"join($.Artist, $.Title)")
                .Skip(offset)
                .Limit(limit)
                .ToList()
                .Select(MappedToDomain);
        }

        public void Save(ITrack track)
        {
            var model = MappedToModel(track);
            _dbWriter.Commit(model.Id == 0
                ? (Action)(() => _tracks.Insert(model))
                : () => _tracks.Update(model));            
        }

        public void Save(IEnumerable<ITrack> tracks)
        {
            var models = tracks.Select(MappedToModel).ToList();
            _dbWriter.Commit(() =>
            {
                _tracks.Upsert(models);
            });
        }

        internal Track MappedToModel(ITrack source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Track model = TrackByUri(source.Uri).IfNone(() => new Track());
            if (source is IPersistedTrack persistedTrack)
            {
                model.Id = persistedTrack.Id;
                model.LastScannedOn = persistedTrack.LastScannedOn.Ticks;
            }
            else
            {
                model.LastScannedOn = DateTime.Now.Ticks;
            }
            if (source.WaveformData != null && source.WaveformData.Length > 0)
            {
                model.WaveformData = source.WaveformData;
            }

            model.Uri = source.Uri.ToLowerInvariant();
            model.Title = source.Meta.Title;
            model.Artist = source.Meta.Artist;
            model.Album = source.Meta.Album;
            model.AlbumArtist = source.Meta.AlbumArtist;
            model.Conductor = source.Meta.Conductor;
            model.Year = source.Meta.Year;
            model.BPM = source.Meta.BPM;
            model.Comment = source.Meta.Comment;
            model.Duration = source.Duration.Ticks;
            model.StartTime = source.StartTime.Ticks;
            model.EndTime = source.EndTime.Ticks;
            model.Genre = source.Meta.Genre;
            model.Rating = source.Meta.Rating;
            model.Grouping = source.Meta.Grouping;
            model.ReplayGain = source.Meta.ReplayGain;
            model.SearchIndex = new InternationalizedString(source.Meta.ToString().ToLowerInvariant());
            model.ConfirmedReadable = source.ConfirmedReadable;

            return model;
        }

        public void Forget(string path)
        {
            ForgetUsingSql(path);
            //ForgetUsingCollectionMethods(path);
        }

        private void ForgetUsingCollectionMethods(string path)
        {
            var tracks = _tracks.Query().Where(t => t.Uri == path).ToList();
            tracks.ForEach(track =>
            _dbWriter.Commit(() =>
                {
                    _playlistTracks.DeleteMany($"$.TrackId = {track.Id}");
                    var tracksDeleted = _tracks.DeleteMany($"$._id = {track.Id}");
                    Debug.Assert(tracksDeleted > 0);
                })
            );
        }

        private void ForgetUsingSql(string path)
        {
            var pathCi = path.ToLowerInvariant();
            var findSql = $"select $._id from tracks where $.Uri = \"{EscapePath(pathCi)}\"";
            var trackIds = db.Execute(findSql)
                        .ToEnumerable()
                        .Select(doc => doc["_id"].AsInt32)
                        .ToList();
            if (!trackIds.Any())
            {
                return;
            }

            trackIds.ForEach(trackId =>
                _dbWriter.Commit(() =>
                {
                    var deleteFromPlaylistsSql = $"delete playlist_tracks where $.TrackId = {trackId}";
                    db.Execute(deleteFromPlaylistsSql);
                    var deleteFromTracksSql = $"delete tracks where $._id = {trackId}";
                    db.Execute(deleteFromTracksSql);
                })
            );
        }

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
                    var findSql = $"select $._id from tracks where $.Uri in ([{string.Join(", ", chunk.Select(p => $"\"{EscapePath(p)}\""))}])";
                    var trackIds = db.Execute(findSql)
                        .ToEnumerable()
                        .Select(doc => doc["_id"].AsInt32)
                        .ToList();
                    var deleteFromPlaylistsSql = $"delete playlist_tracks where $.TrackId in ([{string.Join(", ", trackIds)}])";
                    db.Execute(deleteFromPlaylistsSql);
                    var deleteFromTracksSql = $"delete tracks where $._id in ([{string.Join(", ", trackIds)}])";
                    db.Execute(deleteFromTracksSql);
                }
            });
        }

        public void Forget(IPersistedPlaylist playlist)
        {
            var pathCi = playlist.Uri.ToLowerInvariant();

            _dbWriter.Commit(() => _playlists.DeleteMany($"$.Uri = '{pathCi}'"));
        }

        public IEnumerable<IPersistedPlaylist> AllPlaylists()
        {
            return _playlists.FindAll();
        }

        public IPersistedPlaylist GetOrCreate(string path)
        {
            var pathCi = path.ToLowerInvariant();

            var maybe = _playlists.FindOne(p => p.Uri == pathCi);
            if (maybe != null)
            {
                return maybe;
            }

            var playlist = new Playlist {  Uri = path };
            var id = _playlists.Insert(playlist);
            playlist.Id = id.AsInt32;
            
            return playlist;
        }

        public IPersistedPlaylist? ByPlaylistId(int id)
        {
            return _playlists.FindById(id);
        }

        public IEnumerable<(int PlaylistId, string? SearchTerm)> PlaylistSession()
        {
            return _openPlaylists.FindAll()
                .Select(op => (op.Id, op.SearchTerm));
        }

        public void PlaylistSession(IEnumerable<(int PlaylistId, string SearchTerm)> openPlaylists)
        {
            _dbWriter.Commit(() =>
            {
                db.BeginTrans();
                var existing = _openPlaylists.FindAll().ToList();
                existing.Where(e =>
                        openPlaylists.All(op => op.PlaylistId != e.PlaylistId))
                    .Iter(p => _openPlaylists.Delete(p.Id));
                var toSave = openPlaylists.Where(op =>
                        existing.All(e => e.PlaylistId != op.PlaylistId))
                    .Select(p => new OpenPlaylist
                    {
                        PlaylistId = p.PlaylistId,
                        SearchTerm = !string.IsNullOrEmpty(p.SearchTerm)
                            ? p.SearchTerm
                            : null
                    });
                _openPlaylists.InsertBulk(toSave);
                db.Commit();
            });
        }

        public void Save(IPlaylist plist)
        {
            if (string.IsNullOrEmpty(plist.Uri) || string.IsNullOrWhiteSpace(plist.Uri))
                throw new ArgumentException("Playlist filename cannot be null");

            var model = new Playlist
            {
                Uri = plist.Uri,
                Comment = plist.Comment
            };
            if (plist is IPersistedPlaylist playlist)
            {
                model.Id = playlist.Id;
            }

            var existing = _playlists.FindById(model.Id);
            if (existing != null)
            {
                existing.Comment = model.Comment;
                _dbWriter.Commit(() => _playlists.Update(existing));
                return;
            }

            _dbWriter.Commit(() => _playlists.Insert(model));
        }

        public void PopulatePlaylist(IPersistedPlaylist playlist, IEnumerable<ITrack> tracks)
        {
            var workingSet = tracks.ToList();
            var sortOrder = workingSet.Select((t, i) => (i, t.Uri));
            // fetch Tracks that exist in DB
            var existing = ModelsFromPaths(workingSet.Select(t => t.Uri)).ToList();
            var toCreate = workingSet.Where(t =>
                    !existing.Any(e => e.Uri != null && e.Uri.Equals(t.Uri, StringComparison.InvariantCultureIgnoreCase)))
                .Select(t => MappedToModel(t))
                .ToList();

            _dbWriter.Commit(() =>
            {
                var sortedTracks = existing;
                if (toCreate.Count > 0)
                {
                    // save those that do not exist in tracks collection
                    var inserted = _tracks.InsertBulk(toCreate);
                    var created = ModelsFromPaths(toCreate.Select(t => t.Uri!)).ToList();
                    Debug.Assert(created.Count == inserted);
                    var allTracks = existing.Concat(created);
                    // sort by the original order
                    sortedTracks = sortOrder.Select(tup => allTracks.First(t => t.Uri != null && t.Uri.Equals(tup.Uri, StringComparison.InvariantCultureIgnoreCase))).ToList();
                }
                _playlistTracks.DeleteMany($"$.PlaylistId = {playlist.Id}");
                var toInsert = sortedTracks.Select((t, i) =>
                     new PlaylistTrack
                    {
                        PlaylistId = playlist.Id, Track = t, Position = i,
                        CreatedAt = DateTime.UtcNow.Ticks
                    });
                _playlistTracks.InsertBulk(toInsert);
            });
        }

        private IEnumerable<Track> ModelsFromPaths(IEnumerable<string> paths)
        {
            var @params = paths.ToList();
            return _tracks.Query().Where($"$.Uri in ([{string.Join(",", @params.Select(p => $"\"{EscapePath(p)}\""))}])").ToEnumerable();
            //var sql = $"select $ from tracks where $.Uri in ([{string.Join(",", @params.Select(p => $"\"{EscapePath(p)}\""))}])";
            //var intermediate = db.Execute(sql)
            //    .ToEnumerable();

            //return query.Select(bson => new Track(bson.AsDocument));
        }

        private static string EscapePath(string p) => p.Replace(@"\", @"\\");

        public IEnumerable<ITrack> Contents(IPersistedPlaylist playlist)
        {
            return ContentsUsingLinq(playlist);
        }

        private IEnumerable<ITrack> ContentsUsingLinq(IPersistedPlaylist playlist)
        {
            var list = _playlistTracks.Query()
                .Include(pt => pt.Track)
                .Where(pt => pt.PlaylistId == playlist.Id)
                .OrderBy(pt => pt.Position)
                .ToList();
            return list.Where(pt => pt.Track != null).Select(pt => MappedToDomain(pt.Track));
        }

        private IEnumerable<ITrack> ContentsUsingSql(IPersistedPlaylist playlist)
        {
            return db.Execute($"SELECT $.Track FROM playlist_tracks INCLUDE $.Track WHERE $.PlaylistId={playlist.Id} ORDER BY $.Position")
                .ToEnumerable()
                .Select(doc => MappedToDomain(new Track(doc["Track"].AsDocument)));
        }

        public string DeviceName(object id)
        {
            return _devices.FindOne(d => d.Identifier == id.ToString())?.Name ?? string.Empty;
        }

        public void Cache(object id, string name)
        {
            var maybe = _devices.FindOne(d => d.Identifier == id.ToString());
            if (maybe == null) return;

            _dbWriter.Commit(() => _devices.Insert(new CachedDeviceInfo { Identifier = id.ToString(), Name = name }));
        }
        public void Dispose()
        {
            db.Dispose();
        }

        private void CreateIndices()
        {
            var tracks = db.GetCollection<Track>("tracks");
            tracks.EnsureIndex(t => t.Uri);
            tracks.EnsureIndex(t => t.Title);
            tracks.EnsureIndex(t => t.Artist);
            tracks.EnsureIndex(t => t.Genre);
            tracks.EnsureIndex(t => t.Comment);
            tracks.EnsureIndex(t => t.SearchIndex);
            tracks.EnsureIndex(t => t.Grouping);

            var playlists = db.GetCollection<Playlist>("playlists");
            playlists.EnsureIndex(t => t.Uri);
            playlists.EnsureIndex(t => t.Comment);

            var playlistTracks = db.GetCollection<PlaylistTrack>("playlist_tracks");
            playlistTracks.EnsureIndex(t => t.PlaylistId);
            playlistTracks.EnsureIndex(t => t.Track);
        }
    }
}
