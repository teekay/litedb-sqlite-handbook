#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MusicLibrary.Metadata;
using MusicLibrary.Metadata.Meta;
using MusicLibrary.Persistence.SqliteNet.Model;
using MusicLibrary.Playlists;
using SQLite;
using static LanguageExt.Prelude;

namespace MusicLibrary.Persistence.SqliteNet
{
    internal sealed class ServesTracks
    {
        public ServesTracks(SQLiteConnection connection, IComposer composer)
        {
            _composer = composer;
            Connection = connection;
        }

        private readonly IComposer _composer;

        private SQLiteConnection Connection { get; }

        internal ITrack ModelMappedToITrack(Track model)
        {
            var meta = new SongMetadata(
                Nns(model.Year), Nns(model.Title), Nns(model.Artist), Nns(model.Album),
                Nns(model.Comment), Nns(model.Genre), Nns(model.AlbumArtist), Nns(model.Conductor),
                model.BPM, model.ReplayGain, (uint) Math.Abs(model.Rating), model.Grouping ?? string.Empty);
            return new PersistedSong(
                _composer.SongPlayableOnPlatform(
                    Nns(model.Uri),
                    new CachedMetadataSource(meta, TsFromTicks(model.Duration)),
                    model.WaveformData ?? new byte[0]),
                meta,
                new TaggedAudioFileProperties(TsFromTicks(model.Duration)),
                model.Id,
                new DateTime(model.LastScannedOn),
                TsFromTicks(model.StartTime),
                TsFromTicks(model.EndTime),
                model.ConfirmedReadable);
        }

        internal Option<Track> TrackByUri(string filepath)
        {
            return Connection.Query<Track>("select * from Track where Filepath = ?", filepath)
                .FirstOrDefault();
        }

        private TimeSpan TsFromTicks(long ticks) => TimeSpan.FromTicks(ticks);
        private string Nns(string? v) => v ?? string.Empty;

        /// <summary>
        /// Search the database for a track by its path
        /// </summary>
        /// <param name="filepath">Path to track including the filename</param>
        /// <returns>Track if found, otherwise null</returns>
        internal Option<ITrack> ByFilename(string filepath) => 
            TrackByUri(filepath).Match(
                y => Optional(ModelMappedToITrack(y)),
                () => Option<ITrack>.None);


        internal IEnumerable<ITrack> FromPaths(IEnumerable<string> filepaths)
        {
            return ModelsFromPaths(filepaths).Select(ModelMappedToITrack);
        }

        internal IEnumerable<Track> ModelsFromPaths(IEnumerable<string> filepaths)
        {
            var paths = filepaths.ToArray();
            const int maxChunkSize = 11;
            var models = new List<Track>(paths.Length);
            var pars = new object[maxChunkSize];
            for (int i = 0; i*maxChunkSize < paths.Length; i++)
            {
                var chunkStart = i * maxChunkSize;
                var chunkSize = i * maxChunkSize + maxChunkSize > paths.Length 
                    ? paths.Length - i * maxChunkSize
                    : maxChunkSize;
                for (int j = chunkStart, k = 0; j < chunkStart + chunkSize; j++, k++)
                {
                    pars[k] = paths[j];
                }
                var q = $"SELECT * FROM Track WHERE {string.Join(" OR ", Enumerable.Repeat("Filepath=?", chunkSize))}";
                models.AddRange(Connection.Query<Track>(q, pars));
            }

            return models;
        }

        internal IEnumerable<ITrack> FromDirectory(string filepath) =>
            InDirectory(filepath).Select(ModelMappedToITrack);

        internal IEnumerable<Track> InDirectory(string filepath)
            => Connection.Query<Track>("select * from Track where Filepath like ?", $"{filepath}%");

        private string Query(string[] keywords) =>
            $"SELECT * FROM Track WHERE {string.Join(" AND ", keywords.Select(k => $"SearchIndex LIKE '%{Escaped(k)}%'"))} GROUP BY Filepath ORDER BY Genre, Artist, Title";

        private string Escaped(string s) => s.Replace("'", "''");

        private IEnumerable<Track> ByKeywords(string[] keywords)
        {
            if (keywords.Length == 0) return Connection.Table<Track>();
            var sql = Query(keywords);
            return Connection.Query<Track>(sql);
        }

        private bool IsMinLength(string s) => s.Length >= 3;

        internal IEnumerable<ITrack> Search(string keywords) =>
            ByKeywords(keywords.ToLower()
                    .Split(' ')
                    .Filter(IsMinLength).ToArray())
                .Select(ModelMappedToITrack);

        internal async Task<IEnumerable<ITrack>> SearchAsync(SQLiteAsyncConnection asyncConn, string keywords)
        {
            if (keywords.Length == 0)
            {
                var query = asyncConn.Table<Track>(); // TODO order by... ?
                return (await query.ToListAsync()).Select(ModelMappedToITrack);
            }
            var searchWords = keywords.ToLower()
                .Split(' ')
                .Filter(IsMinLength).ToArray();
            var sql = Query(searchWords);
            return (await asyncConn.QueryAsync<Track>(sql))
                .Select(ModelMappedToITrack);
        }

        public IEnumerable<ITrack> ByGenre(string genreName)
        {
            return ByGenreUsingSql(genreName);
        }

        internal IEnumerable<ITrack> ByGenreUsingSql(string genreName)
        {
            return (string.IsNullOrWhiteSpace(genreName) || string.IsNullOrEmpty(genreName)
                    ? Connection.Query<Track>(ByNoGenreQuery(), string.Empty, @" ")
                    : Connection.Query<Track>(ByGenreQuery(), genreName))
                    .Select(ModelMappedToITrack);
        }

        public IEnumerable<ITrack> ByGenreWithLinq(string genreName)
        {
            return (string.IsNullOrWhiteSpace(genreName) || string.IsNullOrEmpty(genreName)
                    ? Connection.Table<Track>().Where(t => t.Genre == null || t.Genre == @"" || t.Genre == @" ")
                    : Connection.Table<Track>().Where(t => t.Genre == genreName))
                    .OrderBy(t => t.Artist).ThenBy(t => t.Title)
                    .Select(ModelMappedToITrack);
        }

        public async Task<IEnumerable<ITrack>> ByGenreAsync(SQLiteAsyncConnection asyncConn, string genreName)
        {
            return (string.IsNullOrWhiteSpace(genreName) || string.IsNullOrEmpty(genreName)
                ? await asyncConn.QueryAsync<Track>(ByNoGenreQuery(), string.Empty, @" ")
                : await asyncConn.QueryAsync<Track>(ByGenreQuery(),
                    genreName))
                .Select(ModelMappedToITrack);
        }

        private string ByGenreQuery() => "SELECT * FROM Track WHERE Genre=? ORDER BY Artist, Title";

        private string ByNoGenreQuery() => "SELECT * FROM Track WHERE Genre IS NULL OR Genre=? OR Genre=? ORDER BY Artist, Title";

        public async Task<IEnumerable<ITrack>> ByGenreAsync(SQLiteAsyncConnection asyncConn, string genreName, int offset, int limit)
        {
            return (await asyncConn.QueryAsync<Track>(
                    $"SELECT * FROM Track WHERE Genre=? ORDER BY Artist, Title LIMIT {limit} OFFSET {offset}", genreName))
                .Select(ModelMappedToITrack);
        }

        public IEnumerable<ITrack> ByGrouping(string grouping) =>
            Connection.Query<Track>("SELECT * FROM Track WHERE Grouping=? ORDER BY Artist, Title", grouping)
                .Select(ModelMappedToITrack);

        public async Task<IEnumerable<ITrack>> ByGroupingAsync(SQLiteAsyncConnection asyncConn, string grouping)
        {
            return (await asyncConn.QueryAsync<Track>("SELECT * FROM Track WHERE Grouping=? ORDER BY Artist, Title",
                    grouping))
                .Select(ModelMappedToITrack);
        }

        internal IEnumerable<ITrack> Contents(IPersistedPlaylist playlist)
        {
            const string sql = @"SELECT a.* FROM Track a JOIN PlaylistTracks b WHERE b.PlaylistId=? AND a.Id=b.TrackId ORDER BY b.Position";
            return Connection.Query<Track>(sql, playlist.Id).Select(ModelMappedToITrack);
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
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (source.WaveformData != null && source.WaveformData.Length > 0)
            {
                model.WaveformData = source.WaveformData;
            }

            model.Uri = source.Uri;
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

    }
}
