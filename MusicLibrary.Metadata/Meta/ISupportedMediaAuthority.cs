namespace MusicLibrary.Metadata.Meta
{
    public interface ISupportedMediaAuthority
    {
        /// <summary>
        /// Tell whether a given file path ends with a known music file extension
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool IsFileExtensionSupported(string path);

        /// <summary>
        /// Tell whether a give file path ends with a known playlist extension
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool IsPlaylistExtensionSupported(string path);
    }
}