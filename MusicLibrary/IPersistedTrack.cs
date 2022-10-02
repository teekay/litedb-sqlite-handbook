using System;

namespace MusicLibrary
{
    public interface IPersistedTrack : ITrack
    {
        /// <summary>
        /// Represents a database Id
        /// </summary>
        int Id { get; }

        /// <summary>
        /// When was the file scanned last time
        /// </summary>
        DateTime LastScannedOn { get; set; }
    }
}
