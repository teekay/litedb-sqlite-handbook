using System.Globalization;
using System.Text;

namespace MusicLibrary.Metadata
{
    public sealed class InternationalizedString
    {
        public InternationalizedString(string? value)
        {
            _value = value ?? string.Empty;
        }

        private readonly string _value;

        public static implicit operator string(InternationalizedString str) => str.Value();
        public static implicit operator InternationalizedString(string str) => new InternationalizedString(str);

        public string Value() => AsInternational(_value);

        public override string ToString() => Value();

        public static string AsInternational(string? value) => 
            string.Concat((value ?? string.Empty).Normalize(NormalizationForm.FormD)
                .ToCharArray()
                .Filter(s => CharUnicodeInfo.GetUnicodeCategory(s) != UnicodeCategory.NonSpacingMark))
            .Normalize(NormalizationForm.FormC);
    }
}
