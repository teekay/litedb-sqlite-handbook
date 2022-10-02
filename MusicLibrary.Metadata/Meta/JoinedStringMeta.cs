namespace MusicLibrary.Metadata.Meta
{
    /// <summary>
    /// Metadata printer that concatenates an array of string-valued metadata
    /// using the provided separator.
    /// </summary>
    internal class JoinedStringMeta : ISongMeta
    {
        public JoinedStringMeta(string[] source, string separator)
        {
            _source = source ?? new string[0];
            _separator = separator;
        }
        private readonly string[] _source;
        private readonly string _separator;

        public override string ToString()
        {
            return string.Join(_separator, _source);
        }
    }
}