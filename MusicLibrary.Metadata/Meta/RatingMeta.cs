using System;
using MusicLibrary.Metadata.Meta.Rating;
using TagLib;
using TagLib.Ogg;

namespace MusicLibrary.Metadata.Meta
{
    /// <summary>
    /// Prints rating based on Taglib's opinion. Implementation dependent, it seems to be.
    /// </summary>
    internal sealed class RatingMeta : ISongMeta
    {
        public RatingMeta(uint rating)
        {
            Rating = (int) Math.Abs(rating);
        }

        public RatingMeta(int rating)
        {
            Rating = Math.Abs(rating);
        }

        public RatingMeta(float rating)
        {
            // assume 0-1f
            Rating = (int) (rating * 100);
        }

        public RatingMeta(Tag tag)
        {
            var rating = tag switch
            {
                TagLib.Id3v2.Tag id3 => (IMetadataRating)new Id3V2Rating(id3),
                XiphComment xiph => new XiphRating(xiph),
                TagLib.Flac.Metadata flac => new FlacRating(flac),
                _ => new ZeroRating()
            };
            Rating = (int)rating.Value();
        }

        public int Rating { get; }
    }
}
