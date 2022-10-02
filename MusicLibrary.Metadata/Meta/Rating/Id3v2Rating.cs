using TagLib.Id3v2;

namespace MusicLibrary.Metadata.Meta.Rating
{
    /// <summary>
    /// Reads Rating from a Id3V2 tag
    /// </summary>
    internal sealed class Id3V2Rating : IMetadataRating
    {
        public Id3V2Rating(TagLib.Id3v2.Tag tag)
        {
            _tag = tag;
        }

        private readonly TagLib.Id3v2.Tag _tag;

        public double Value()
        {
            byte rating = WMP9Rating().maybeByteValue().IfNone(MusicBeeRating().maybeByteValue().IfNone(0));
            return (double)rating / 255 * 100;
        }

        private NonZeroByte WMP9Rating() => new NonZeroByte(PopularimeterFrame.Get(
            _tag, RatingTagWin, true).Rating);

        private NonZeroByte MusicBeeRating() => new NonZeroByte(PopularimeterFrame.Get(
            _tag, RatingTagMusicbee, true).Rating);


        /// <summary>
        /// The rating tag MusicBee uses
        /// </summary>
        private const string RatingTagMusicbee = @"MusicBee";

        /// <summary>
        /// The rating tag Windows Media Player uses
        /// </summary>
        private const string RatingTagWin = @"Windows Media Player 9 Series";

    }
}
