namespace MusicLibrary.Metadata
{
    public class EmptyMetadata : IMetadata
    {
        public string Title => string.Empty;
        public string Artist => string.Empty;
        public string Conductor => string.Empty;
        public string Genre => string.Empty;
        public string Year => string.Empty;
        public string Album => string.Empty;
        public string AlbumArtist => string.Empty;
        public string Comment => string.Empty;
        public int Rating => 0;
        public string Grouping => string.Empty;
        public double BPM => 0;
        public double ReplayGain => 0;

        public void Update(IMetadata source)
        {
            // no-op    
        }

        public override string ToString() => @"EmptyMetadata";
    }
}
