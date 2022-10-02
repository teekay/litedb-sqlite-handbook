using System.Diagnostics;
using MusicLibrary.Persistence.SqliteNet.Model;
using SQLite;

namespace MusicLibrary.Persistence.SqliteNet.Migrations
{
    /// <summary>
    /// Fixes the collation of the Filepath column of the Track table
    /// </summary>
    public sealed class TrackFilepathMigration: IDbMigration
    {
        public TrackFilepathMigration(SQLiteConnection database)
        {
            _database = database;
        }

        private readonly SQLiteConnection _database;

        public bool IsPending()
        {
            var res = _database.QueryScalars<string>(@"SELECT sql FROM sqlite_master WHERE name = 'Track'");
            if (res.Count != 1) return true;
            var sql = res[0];
            return !sql.Contains("\"Filepath\" varchar collate NOCASE");
        }

        public void Migrate()
        {
            _database.BeginTransaction();
            const string alterSql = @"ALTER TABLE Track RENAME TO Track_Copy";
            _database.Execute(alterSql);
            _database.CreateTable<Track>();
            const string copySql = @"INSERT INTO Track(Id, Filepath, Title, Artist, AlbumArtist, Conductor, Album, Genre, Year, Duration, Comment, BPM, ReplayGain, Rating, StartTime, EndTime, WaveformData, LastScannedOn, ConfirmedReadable, SearchIndex, Grouping) SELECT Id, Filepath, Title, Artist, AlbumArtist, Conductor, Album, Genre, Year, Duration, Comment, BPM, ReplayGain, Rating, StartTime, EndTime, WaveformData, LastScannedOn, ConfirmedReadable, SearchIndex, Grouping FROM Track_Copy";
            _database.Execute(copySql);
            const string dropSql = @"DROP TABLE Track_Copy";
            _database.Execute(dropSql);
            _database.Commit();

            _database.Execute("VACUUM main");
            Debug.WriteLine("File paths fixed to be case insensitive");
        }
    }
}
