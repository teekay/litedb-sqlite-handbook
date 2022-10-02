namespace MusicLibrary.Cli
{
    public class DirectDbWriter : IDbWriter
    {
        public void Commit(Action commitAction)
        {
            commitAction.Invoke();
        }
    }
}
