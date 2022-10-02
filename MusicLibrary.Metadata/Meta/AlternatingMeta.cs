using System.Linq;
using static LanguageExt.Prelude;

namespace MusicLibrary.Metadata.Meta
{
    /// <summary>
    /// Given a list of strings, selects the first one that is non-null and non-empty
    /// and prints it when asked to perform ToString();
    /// </summary>
    internal class AlternatingMeta : ISongMeta
    {
        public AlternatingMeta(params string[] alternatives)
        {
            _firstNonEmptyAlternative = match(
                Optional(alternatives.ToList()
                    .FirstOrDefault(t => !string.IsNullOrEmpty(t) && !string.IsNullOrWhiteSpace(t))),
                t => t, () => string.Empty);
        }

        private readonly string _firstNonEmptyAlternative;

        public override string ToString() => _firstNonEmptyAlternative;
    }
}
