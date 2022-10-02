using System;
using System.Collections.Generic;

namespace MusicLibrary
{
    public static class ListEx
    {
        public static List<List<T>> Chunk<T>(this List<T> me, int size = 50)
        {
            var list = new List<List<T>>();
            for (int i = 0; i < me.Count; i += size)
                list.Add(me.GetRange(i, Math.Min(size, me.Count - i)));
            return list;
        }
    }
}
