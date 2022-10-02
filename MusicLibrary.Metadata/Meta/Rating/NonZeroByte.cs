using LanguageExt;
using static LanguageExt.Prelude;

namespace MusicLibrary.Metadata.Meta.Rating
{
    /// <summary>
    /// Could be a way to provide back byte values but only if they are non-zero,
    /// since zero often indicates the absence of value rather than the real zero.
    /// </summary>
    internal sealed class NonZeroByte
    {
        public NonZeroByte(byte value)
        {
            _value = value;
        }
        private readonly byte _value;

        public Option<byte> maybeByteValue() =>
            _value != 0
                ? Optional(_value)
                : Option<byte>.None;
    }
}
