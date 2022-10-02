namespace MusicLibrary.Cli
{
    public class DirectoryScannedEventArgs : EventArgs
    {
        public string Directory { get; private set; }
        public double ProgressPercent { get; private set; }

        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null" />.</exception>
        public DirectoryScannedEventArgs(string path, double progress)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            Directory = path;
            ProgressPercent = progress;
        }
    }
}
