using SQLite;

namespace MusicLibrary.Persistence.SqliteNet.Migrations
{
    public class GroupingTagMigration : IDbMigration
    {
        public GroupingTagMigration(SQLiteConnection database)
        {
            _database = database;
        }

        private readonly SQLiteConnection _database;

        public bool IsPending()
        {
            string query = "SELECT COUNT(*) as NonEmpty FROM Track WHERE Grouping IS NOT NULL";
            var result = _database.QueryScalars<int>(query);
            return result.Count == 1 && result[0] == 0;
        }

        public void Migrate()
        {
            // no-op, we just force DbUpdater to rescan the library
        }
    }
}
