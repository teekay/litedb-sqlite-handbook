using System.Diagnostics;
using System.Linq;
using MusicLibrary.Persistence.SqliteNet.Model;
using SQLite;

namespace MusicLibrary.Persistence.SqliteNet.Migrations
{
    /// <summary>
    /// De-duplicates the Track table
    /// </summary>
    public sealed class TrackDupesMigration: IDbMigration
    {
        public TrackDupesMigration(SQLiteConnection connection)
        {
            _connection = connection;
        }

        private readonly SQLiteConnection _connection;
        const string sql = "SELECT Filepath, COUNT(Filepath) As Cnt FROM Track GROUP BY Filepath HAVING Cnt > 1";

        public bool IsPending()
        {
            var dupes = _connection.Query<DupesCount>(sql);
            return dupes.Count > 0;
        }

        public void Migrate()
        {
            var dupes = _connection.Query<DupesCount>(sql);
            _connection.BeginTransaction();
            foreach (var dupe in dupes)
            {
                var tracks = _connection.Query<Track>("SELECT * FROM Track WHERE Filepath=?", dupe.Filepath);
                Debug.Assert(tracks.Count == dupe.Cnt);
                if (tracks.Count < 2) continue;
                var winner = tracks[0];
                Debug.WriteLine($"De-duping {winner.Uri}");
                if (winner.WaveformData == null)
                {
                    winner.WaveformData = tracks.FirstOrDefault(t => t.WaveformData != null)?.WaveformData;
                }

                if (string.IsNullOrEmpty(winner.Grouping))
                {
                    winner.Grouping = tracks.FirstOrDefault(t => !string.IsNullOrEmpty(t.Grouping))?.Grouping;
                }

                winner.LastScannedOn = tracks.Select(t => t.LastScannedOn).Max();

                winner.ConfirmedReadable = tracks.Any(t => t.ConfirmedReadable);

                _connection.Update(winner);

                foreach (var loser in tracks.Skip(1).ToList())
                {
                    _connection.Execute(@"UPDATE PlaylistTracks SET TrackId=? WHERE TrackId=?", winner.Id, loser.Id);
                    _connection.Execute(@"DELETE FROM Track WHERE Id=?", loser.Id);
                }
            }
            _connection.Commit();
        }

        private class DupesCount
        {
            public string? Filepath { get; set; }
            public int Cnt { get; set; }
        }
    }
}
