namespace MusicLibrary.Metadata
{
    public interface IMetadataSource
    {
        /// <summary>
        /// Read the song metadata
        /// </summary>
        /// <returns></returns>
        IMetadata Lyrics();

        IAudioFileProperties Properties();
    }
}
