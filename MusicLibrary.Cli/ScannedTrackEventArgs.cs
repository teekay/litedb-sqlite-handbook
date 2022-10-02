namespace MusicLibrary.Cli
{
    public class ScannedTrackEventArgs : EventArgs
    {
        public ScannedTrackEventArgs(ITrack track)
        {
            ScannedTrack = track;
        }

        public ITrack ScannedTrack { get; }
    }
}
