namespace MusicLibrary.Metadata.Meta
{
    internal struct TrimmedStringMeta : ISongMeta
    {
        public TrimmedStringMeta(string source)
        {
            _value = source != null ? source.Trim() : string.Empty;
        }

        private readonly string _value;

        public override string ToString() => _value;
    }
}