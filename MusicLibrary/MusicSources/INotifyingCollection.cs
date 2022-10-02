using System.Collections.Generic;
using System.Collections.Specialized;

namespace MusicLibrary.MusicSources
{
    public interface INotifyingCollection<T>: IList<T>, INotifyCollectionChanged
    {
        void AddRange(IEnumerable<T> items);
        void InsertRange(int index, IEnumerable<T> items);
        void RemoveRange(IEnumerable<T> playlistItems);
    }
}
