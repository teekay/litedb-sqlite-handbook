using System;

namespace MusicLibrary
{
    public interface IDbWriter
    {
        /// <summary>
        /// Performs an arbitrary write operation
        /// Would be nice if the caller could require that an action shall run in a transaction
        /// </summary>
        /// <param name="commitAction"></param>
        void Commit(Action commitAction);
    }
}
