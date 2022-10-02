using TagLib.Ogg;

namespace MusicLibrary.Metadata.Meta.Rating
{
    /// <summary>
    /// Prints rating of an Ogg file.
    /// </summary>
    internal sealed class XiphRating : XiphFlacRatingBase
    {
        public XiphRating(XiphComment tag)
        {
            _tag = tag;
        }

        private readonly XiphComment _tag;
        protected override XiphComment WithComment()
        {
            return _tag;
        }
    }
}
