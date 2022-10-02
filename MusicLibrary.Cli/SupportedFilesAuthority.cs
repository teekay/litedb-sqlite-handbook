using MusicLibrary.Metadata.Meta;

namespace MusicLibrary.Cli
{
    /// <summary>
    /// Provides information about supported file extensions
    /// </summary>
    public class SupportedFilesAuthority : ISupportedMediaAuthority
    {
        /// <summary>
        /// List of file extensions.
        /// At some point in the future, this list will be dynamic
        /// based on available file readers and their capabilities.
        /// </summary>
        private static readonly IReadOnlyList<string> SupportedTrackExtensions
            = new List<string> { "wav", "mp3", "ogg", "flac", "wma", "m4a", "aif", "aiff" }
            .AsReadOnly();

        private static readonly IReadOnlyList<string> _supportedPlaylistFormats = new List<string> {"m3u", "m3u8"}
            .AsReadOnly();

        public static IList<string> SupportedExtensions { get; } = SupportedTrackExtensions.ToList();
        
        public static IList<string> SupportedPlaylistExtensions { get; } = _supportedPlaylistFormats.ToList();

        /// <summary>
        /// Tell whether a given file path ends with a supported extension
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsFileExtensionSupported(string path)
        {
            var ext = path.Split('.').LastOrDefault();
            return ext != null && SupportedTrackExtensions.Contains(ext.ToLower());
        }

        public bool IsPlaylistExtensionSupported(string path)
        {
            return _supportedPlaylistFormats.Contains(path.Split('.').Last().ToLower());
        }
    }
}
