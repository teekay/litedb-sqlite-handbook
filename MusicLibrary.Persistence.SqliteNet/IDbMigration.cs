namespace MusicLibrary.Persistence.SqliteNet
{
    public interface IDbMigration
    {
        bool IsPending();
        void Migrate();
    }
}