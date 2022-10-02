namespace MusicLibrary.Metadata
{
    public interface IMetadata
    {
        // Descriptive metadata. Does not affect functionality.
        // If any of the below is not there, the track can still be playable.
        string Title { get; }
        string Artist { get; }
        string AlbumArtist { get; }
        string Conductor { get; }
        string Genre { get; }
        string Year { get; }
        string Album { get; }
        string Comment { get; }
        int Rating { get; }
        string Grouping { get; }
        /// <summary>
        /// Metadata - it guides the DJ about how to slow down / speed up the track when played back.
        /// </summary>
        double BPM { get; }

        /// <summary>
        /// Property that guides the playback apparatus to adjust the loudness of the track when played back.
        /// </summary>
        double ReplayGain { get; }

        /// <summary>
        /// Allow update of metadata from another source
        /// </summary>
        /// <param name="source"></param>
        void Update(IMetadata source);
    }
}
