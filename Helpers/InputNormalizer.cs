using System.Text.RegularExpressions;

namespace ECOMMERCE_NEXOSOFT.Helpers
{
    public static class InputNormalizer
    {
        public static string NormalizeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var text = value.Trim();
            text = Regex.Replace(text, @"\s+", " ");
            return text;
        }

        public static string NormalizeEmail(string? value)
        {
            return NormalizeText(value).ToLowerInvariant();
        }

        public static string NormalizeIdentificationNumber(string? value)
        {
            var text = NormalizeText(value);
            return Regex.Replace(text, @"[\s\.\-]", "");
        }

        public static string NormalizePhone(string? value)
        {
            var text = NormalizeText(value);
            return Regex.Replace(text, @"[\s\-\+]", "");
        }

        public static string NormalizeAddress(string? value)
        {
            return NormalizeText(value);
        }

        public static string NormalizeStoreName(string? value)
        {
            return NormalizeText(value);
        }

        public static string NormalizeSku(string? value)
        {
            var text = NormalizeText(value).ToUpperInvariant();
            return Regex.Replace(text, @"\s+", "");
        }

        public static string NormalizeBarcode(string? value)
        {
            var text = NormalizeText(value);
            return Regex.Replace(text, @"\D", "");
        }

        public static string NormalizeProductName(string? value)
        {
            return NormalizeText(value);
        }

        public static string NormalizeBrand(string? value)
        {
            return NormalizeText(value);
        }
    }
}