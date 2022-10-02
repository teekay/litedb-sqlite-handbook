namespace MusicLibrary.Metadata.Meta.Rating
{
    /// <summary>
    /// Prints the default (empty, zero) rating.
    /// </summary>
    internal sealed class ZeroRating : IMetadataRating
    {
        public double Value() => 0d;
    }
}
