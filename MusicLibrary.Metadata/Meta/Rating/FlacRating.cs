using TagLib.Ogg;

namespace MusicLibrary.Metadata.Meta.Rating
{
    /// <summary>
    /// Print the rating of a Flac file.
    /// </summary>
    internal sealed class FlacRating : XiphFlacRatingBase
    {
        public FlacRating(TagLib.Flac.Metadata tag)
        {
            _tag = tag;
        }

        private readonly TagLib.Flac.Metadata _tag;

        protected override XiphComment WithComment() => 
            _tag.GetComment(false, _tag);
    }
}
