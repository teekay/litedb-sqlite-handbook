using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using TagLib;

namespace MusicLibrary.Metadata
{
    /// <summary>
    /// Static class with one extension method for the File class, which we cannot extend.
    /// </summary>
    public static class TagHelper
    {
        /// <summary>
        /// Supported TagTypes
        /// </summary>
        private static readonly IList<TagTypes> TagTypesPref = new List<TagTypes>
        {
            TagTypes.FlacMetadata,
            TagTypes.Xiph, TagTypes.Apple, 
            TagTypes.Id3v2, TagTypes.Id3v1,
            TagTypes.Ape
        };
        
        /// <summary>
        /// Returns a Tag from File if supported
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Option<Tag> MaybeTag(this File file)
        {
            var firstTagType = TagTypesPref.FirstOrDefault(tt => file.TagTypes.HasFlag(tt));
            var tag = firstTagType != TagTypes.None
                ? file.GetTag(firstTagType)
                : Option<Tag>.None;
            return tag;
        }

        public static Lst<Tag> AllTags(this File file)
        {
            var allTags = TagTypesPref.Where(tt => file.TagTypes.HasFlag(tt));
            return new Lst<Tag>(allTags.Map(file.GetTag));
        }
    }
}
