using TagLib.Ogg;
using static LanguageExt.Prelude;

namespace MusicLibrary.Metadata.Meta.Rating
{
    /// <summary>
    /// Basis for the rating printer of Ogg and Flac files.
    /// </summary>
    internal abstract class XiphFlacRatingBase : IMetadataRating
    {
        protected abstract XiphComment WithComment();

        public double Value()
        {
            var ratings = WithComment().GetField("RATING");
            var maybeComment = ratings.Length > 0 ? ratings[0] : "";
            return parseDouble(maybeComment).IfNone(0d);
        }
    }
}
