#nullable enable
using System;
using System.Linq;
using MusicLibrary.Metadata.Meta;
using TagLib.Id3v2;

namespace MusicLibrary.Metadata
{
    /// <summary>
    /// Tried to guess some metadata from an audio file that is not tagged.
    /// </summary>
    public class GuessingMetadataSource : IMetadataSource
    {
        public GuessingMetadataSource(string uri)
        {
            var title = GuessedTitle(uri);
            _guessedMetadata = new SongMetadata(new Tag {Title = title});
            _guessedProperties = new TaggedAudioFileProperties(TimeSpan.Zero);
        }

        private static string GuessedTitle(string uri)
        {
            var tokens = uri.Split('\\').Last().Split('.').ToList();
            var title = string.Join(" ", tokens.GetRange(0, tokens.Count == 1 ? 1 : tokens.Count - 1));
            return title;
        }

        public GuessingMetadataSource(string uri, IMetadata otherMetadata, IAudioFileProperties audoAudioFileProperties)
        {
            _guessedMetadata = new SongMetadata(otherMetadata.Year, GuessedTitle(uri), otherMetadata.Artist,
                otherMetadata.Album, otherMetadata.Comment, otherMetadata.Genre, otherMetadata.AlbumArtist,
                otherMetadata.Conductor, otherMetadata.BPM, otherMetadata.ReplayGain,
                (uint)Math.Abs(otherMetadata.Rating), otherMetadata.Grouping);
            _guessedProperties = audoAudioFileProperties;
        }


        private readonly IMetadata _guessedMetadata;
        private readonly IAudioFileProperties _guessedProperties;
        
        public IMetadata Lyrics() => _guessedMetadata;

        public IAudioFileProperties Properties() => _guessedProperties;
    }
}
