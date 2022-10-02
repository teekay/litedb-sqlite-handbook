using LanguageExt;
using static LanguageExt.Prelude;

namespace MusicLibrary.Metadata.Meta.Rating
{
    /// <summary>
    /// Could be a way to provide back int values but only if they are non-zero,
    /// since zero often indicates the absence of value rather than the real zero.
    /// </summary>
    internal sealed class NonZeroInt
    {
        public NonZeroInt(int value)
        {
            _value = value;
        }
        private readonly int _value;

        public Option<int> maybeIntValue() =>
            _value != 0
                ? Optional(_value)
                : Option<int>.None;
    }
}
