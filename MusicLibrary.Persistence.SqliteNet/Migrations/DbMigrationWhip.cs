using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace MusicLibrary.Persistence.SqliteNet.Migrations
{
    /// <summary>
    /// Runs all pending database migrations
    /// </summary>
    public class DbMigrationWhip
    {
        public DbMigrationWhip(SQLiteConnection connection)
        {
            _connection = connection;
        }

        private readonly SQLiteConnection _connection;

        public void Migrate()
        {
            new List<IDbMigration> {new TrackFilepathMigration(_connection), new TrackDupesMigration(_connection)}
                .Where(m => m.IsPending())
                .Iter(m => m.Migrate());
        }
    }
}
