namespace MusicLibrary.Metadata
{
    /// <summary>
    /// Provides platform-independent (in theory) access to a file on a filesystem,
    /// for use by TaglibSharp.
    /// </summary>
    public interface IFileAbstractionFactory
    {
        TagLib.File.IFileAbstraction GetFileAbstraction(string path, bool readWrite);
    }
}
