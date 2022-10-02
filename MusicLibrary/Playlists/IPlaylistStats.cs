using System.Collections.Generic;

namespace MusicLibrary.Playlists
{
    public interface IPlaylistStats
    {
        int UsageOf(string uri);
        IList<string> UsageIn(string uri);
    }

    public class NullStats : IPlaylistStats
    {
        public int UsageOf(string uri) => 0;
        public IList<string> UsageIn(string uri) => new List<string>(0);
    }
}
