using System.Collections.Generic;
using System.Linq;
using MusicLibrary.Persistence.SqliteNet.Model;
using MusicLibrary.Playlists;
using SQLite;
using Playlist = MusicLibrary.Persistence.SqliteNet.Model.Playlist;

namespace MusicLibrary.Persistence.SqliteNet
{
    /// <summary>
    /// Provides playback statistics for tracks - how often and in which playlists they appear
    /// </summary>
    public sealed class SqlitePlaylistStats: IPlaylistStats
    {
        public SqlitePlaylistStats(SQLiteConnection connection)
        {
            _connection = connection;
        }

        private readonly SQLiteConnection _connection;
        private IDictionary<string, int>? _stats;

        public int UsageOf(string uri)
        {
            if (_stats?.ContainsKey(uri) ?? false)
            {
                return _stats[uri];
            }
            var maybe = _connection.FindWithQuery<Track>(@"SELECT * FROM Track WHERE Filepath=?", uri);
            if (maybe == null) return 0;
            var count = _connection.QueryScalars<int>(@"SELECT COUNT(*) FROM PlaylistTracks WHERE TrackId=?", maybe.Id);
            if (_stats != null && count.Count > 0)
            {
                _stats.Add(uri, count[0]);
            }
            return count.Count > 0
                ? count[0]
                : 0;
        }

        public IList<string> UsageIn(string uri)
        {
            var maybe = _connection.FindWithQuery<Track>(@"SELECT * FROM Track WHERE Filepath=?", uri);
            if (maybe == null) return new List<string>(0);
            var playlists = _connection.Query<Playlist>("SELECT DISTINCT * FROM Playlist WHERE Id IN (SELECT DISTINCT PlaylistId FROM PlaylistTracks WHERE TrackId=?) ORDER BY Filename", maybe.Id);
            return playlists.Select(p => p.Uri).ToList();
        }

        public void Preload()
        {
            if (_stats != null) return;
            _stats = FillStats();
        }

        private IDictionary<string, int> FillStats()
        {
            return _connection.Query<TrackAppearances>(@"SELECT a.Filepath as Uri, COUNT(b.TrackId) AS Appearances
                                                                          FROM Track a LEFT JOIN PlaylistTracks b ON (b.TrackId=a.Id)
                                                                          GROUP BY Uri").ToDictionary(t => t.Uri, t => t.Appearances);
        }

        private class TrackAppearances
        {
            public string Uri { get; set; } = string.Empty;
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int Appearances { get; set; }
        }
    }
}
