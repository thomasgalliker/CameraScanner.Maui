using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CameraScanner.Maui
{
    public abstract class ResultParser
    {
        private static readonly Regex DIGITS = new Regex("\\d+");
        private static readonly Regex AMPERSAND = new Regex("&");
        private static readonly Regex EQUALS = new Regex("=");
        private const char ByteOrderMark = '\uFEFF';

        public abstract ParsedResult Parse(string source);

        protected static string GetMassagedText(string text)
        {
            if (text.StartsWith(ByteOrderMark))
            {
                text = text[1..];
            }

            return text;
        }

        protected static string[] MaybeWrap(string value)
        {
            return value == null ? null : new[] { value };
        }

        protected static string UnescapeBackslash(string escaped)
        {
            var backslash = escaped.IndexOf('\\');
            if (backslash < 0)
            {
                return escaped;
            }

            var max = escaped.Length;
            var unescaped = new StringBuilder(max - 1);
            unescaped.Append(escaped.Substring(0, backslash));
            var nextIsEscaped = false;

            for (var i = backslash; i < max; i++)
            {
                var c = escaped[i];
                if (nextIsEscaped || c != '\\')
                {
                    unescaped.Append(c);
                    nextIsEscaped = false;
                }
                else
                {
                    nextIsEscaped = true;
                }
            }

            return unescaped.ToString();
        }

        protected static int ParseHexDigit(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }

            if (c >= 'a' && c <= 'f')
            {
                return 10 + (c - 'a');
            }

            if (c >= 'A' && c <= 'F')
            {
                return 10 + (c - 'A');
            }

            return -1;
        }

        protected static bool IsStringOfDigits(string value, int length)
        {
            return value != null && length > 0 && value.Length == length && DIGITS.IsMatch(value);
        }

        protected static bool IsSubstringOfDigits(string value, int offset, int length)
        {
            if (value == null || length <= 0)
            {
                return false;
            }

            var max = offset + length;
            return value.Length >= max && DIGITS.IsMatch(value.Substring(offset, length));
        }

        protected static Dictionary<string, string> ParseNameValuePairs(string uri)
        {
            var paramStart = uri.IndexOf('?');
            if (paramStart < 0)
            {
                return null;
            }

            var result = new Dictionary<string, string>(3);
            var keyValues = AMPERSAND.Split(uri.Substring(paramStart + 1));
            foreach (var keyValue in keyValues)
            {
                AppendKeyValue(keyValue, result);
            }

            return result;
        }

        private static void AppendKeyValue(string keyValue, Dictionary<string, string> result)
        {
            var tokens = EQUALS.Split(keyValue, 2);
            if (tokens.Length == 2)
            {
                var key = tokens[0];
                var value = tokens[1];
                try
                {
                    value = UrlDecode(value);
                    result[key] = value;
                }
                catch (ArgumentException)
                {
                    // Invalid encoding, ignore
                }
            }
        }

        protected static string UrlDecode(string encoded)
        {
            try
            {
                return WebUtility.UrlDecode(encoded); // UTF-8 by default
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Unexpected error during URL decoding", e);
            }
        }

        protected static string[] MatchPrefixedField(string prefix, string rawText, char endChar, bool trim)
        {
            List<string> matches = null;
            var i = 0;
            var max = rawText.Length;

            while (i < max)
            {
                i = rawText.IndexOf(prefix, i);
                if (i < 0)
                {
                    break;
                }

                i += prefix.Length;
                var start = i;
                var more = true;

                while (more)
                {
                    i = rawText.IndexOf(endChar, i);
                    if (i < 0)
                    {
                        i = rawText.Length;
                        more = false;
                    }
                    else if (CountPrecedingBackslashes(rawText, i) % 2 != 0)
                    {
                        i++;
                    }
                    else
                    {
                        matches ??= new List<string>(3);
                        var element = UnescapeBackslash(rawText.Substring(start, i - start));
                        if (trim)
                        {
                            element = element.Trim();
                        }

                        if (!string.IsNullOrEmpty(element))
                        {
                            matches.Add(element);
                        }

                        i++;
                        more = false;
                    }
                }
            }

            return matches == null || matches.Count == 0 ? null : matches.ToArray();
        }

        private static int CountPrecedingBackslashes(string s, int pos)
        {
            var count = 0;
            for (var i = pos - 1; i >= 0; i--)
            {
                if (s[i] == '\\')
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            return count;
        }

        protected static string MatchSinglePrefixedField(string prefix, string rawText, char endChar, bool trim)
        {
            var matches = MatchPrefixedField(prefix, rawText, endChar, trim);
            return matches?[0];
        }
    }
}