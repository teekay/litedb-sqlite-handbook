using SQLite;

namespace MusicLibrary.Persistence.SqliteNet.Migrations
{
    internal class PlaylistTracksIdMigration : IDbMigration
    {
        public PlaylistTracksIdMigration(SQLiteConnection connection)
        {
            Connection = connection;
        }

        private SQLiteConnection Connection { get; }

        public bool IsPending()
        {
            try
            {
                Connection.QueryScalars<string>("SELECT Id FROM PlaylistTracks LIMIT 1");
                return false;
            }
            catch (SQLiteException)
            {
                return true;
            }
        }

        public void Migrate()
        {
            Connection.Execute("create table if not exists \"_PlaylistTracks\" (\"Id\" integer primary key autoincrement not null, \"PlaylistId\" integer not null, \"TrackId\" integer not null, \"Position\" integer not null, \"CreatedAt\" integer not null)");
            Connection.Execute("insert into \"_PlaylistTracks\" (\"PlaylistId\", \"TrackId\", \"Position\", \"CreatedAt\") select * from \"PlaylistTracks\"");
            Connection.Execute(
                "drop table \"PlaylistTracks\"");
            Connection.Execute(
                "alter table \"_PlaylistTracks\" rename to \"PlaylistTracks\"");
            Connection.Execute(
                "create index if not exists \"PlaylistTracks_PlaylistId\" on \"PlaylistTracks\"(\"PlaylistId\")");
            Connection.Execute(
                "create index if not exists \"PlaylistTracks_TrackId\" on \"PlaylistTracks\"(\"TrackId\")");
        }


    }
}
