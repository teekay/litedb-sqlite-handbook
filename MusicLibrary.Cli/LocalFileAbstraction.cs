using MusicLibrary.Metadata;

namespace MusicLibrary.Cli
{
    public sealed class LocalFileAbstraction : TagLib.File.IFileAbstraction, IDisposable
    {
        public LocalFileAbstraction(string path, bool openForWrite = false)
        {
            Name = Path.GetFullPath(path);
            var fileStream = openForWrite ? File.Open(path, FileMode.Open, FileAccess.ReadWrite) : File.OpenRead(path);
            _allowClose = !openForWrite;
            ReadStream = WriteStream = fileStream;
        }

        private readonly bool _allowClose;

        public string Name { get; }

        public Stream ReadStream { get; }

        public Stream WriteStream { get; }

        public void CloseStream(Stream stream)
        {
            if (_allowClose)
            {
                stream?.Close();
            }
        }

        public void Dispose()
        {
            WriteStream.Close();
        }
    }

    public class LocalFileAbstractionFactory : IFileAbstractionFactory
    {
        public TagLib.File.IFileAbstraction GetFileAbstraction(string path, bool readWrite = false)
        {
            return new LocalFileAbstraction(path, readWrite);
        }
    }

}
