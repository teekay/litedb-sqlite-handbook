#nullable enable
using System;
using System.Diagnostics;
using System.Linq;
using IdSharp.Tagging.ID3v2;
using LanguageExt;
using MusicLibrary.Metadata.Meta;
using TagLib;
using static LanguageExt.Prelude;

namespace MusicLibrary.Metadata
{
    /// <summary>
    /// Given an IFileAbstraction, it attempts to read the metadata and returns it if available.
    /// </summary>
    public class MetadataSource : IMetadataSource
    {
        /// <summary>
        /// Default constructor - no metadata will be provided
        /// </summary>
        public MetadataSource() { }

        public MetadataSource(File.IFileAbstraction fileAbstraction)
        {
            _fileAbstraction = fileAbstraction;
        }

        private readonly File.IFileAbstraction? _fileAbstraction;
        private File? _taglibFile;
        private static readonly IMetadata EmptyMetadata = new EmptyMetadata();

        private Option<File> MaybeFile()
        {
            if (_fileAbstraction == null)
            {
                return Option<File>.None;
            }

            try
            {
                return _taglibFile ??= File.Create(_fileAbstraction);
            }
            catch
            {
                return Option<File>.None;
            }
        }

        /// <summary>
        /// Attempts to provide metadata for the encapsulated IFileAbstraction.
        /// </summary>
        public IMetadata Lyrics()
        {
            if (_fileAbstraction == null)
            {
                return EmptyMetadata;
            }

            if (_fileAbstraction.Name.EndsWith(".m4a", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    var atlMeta = new SongMetadata(new ATL.Track(_fileAbstraction.Name));
                    var taglibMetas = TaglibMetas();
                    if (taglibMetas.Count == 0)
                    {
                        return atlMeta;
                    }
                    // sometimes the ATL meta is empty but for the title that is presumably guessed from the filename
                    // so, merge it with the TaglibMetas and hope for the best
                    return new MergedMetadata(new[] { atlMeta }.Concat(taglibMetas).ToArray());
                }
                catch
                {
                    return EmptyMetadata;
                }
            }

            if (_fileAbstraction.Name.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    var tag = new ID3v2Tag(_fileAbstraction.Name);
                    if (tag.Title == null)
                    {
                        // IdSharp.Core won't throw when the metadata is invalid
                        // if a Title is missing, there's a good chance it is.
                        // We let TaglibSharp pick it up and decide for sure.
                        // default
                        return TaglibMeta();
                    }
                    
                    var mp3Meta = new SongMetadata(tag);
                    if (mp3Meta.ReplayGain != 0d) return mp3Meta;

                    var taglibMetas = TaglibMetas().Concat(new []{ mp3Meta }).ToArray();
                    return taglibMetas.Length > 0
                        ? new MergedMetadata(taglibMetas)
                        : EmptyMetadata;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error reading Id3V2 tag with IdSharp.Core: {e.Message}");
                }
            }
            // default
            return TaglibMeta();
        }

        private IMetadata TaglibMeta()
        { 
            var maybeTag = match(MaybeFile(),
                    f => f.MaybeTag(), 
                    () => Option<Tag>.None);
            return match(maybeTag,
                tag => new SongMetadata(tag),
                () => EmptyMetadata);
        }

        private Lst<IMetadata> TaglibMetas()
        { 
            var maybeTags = match(MaybeFile(),
                    f => f.AllTags(), 
                    () => new Lst<Tag>());
            return maybeTags.Select(t => (IMetadata)new SongMetadata(t));
        }

        /// <summary>
        /// Provides additional properties for the encapsulated IFileAbstraction.
        /// </summary>
        /// <returns></returns>
        public IAudioFileProperties Properties()
        {
            return new TaggedAudioFileProperties(
                match(MaybeFile(), 
                    file => file.Properties.Duration, 
                    () => TimeSpan.Zero));
        }
    }
}
