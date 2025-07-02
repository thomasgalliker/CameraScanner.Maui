using System.Text;
using System.Text.RegularExpressions;

namespace CameraScanner.Maui
{
    public sealed class VCardResultParser : ResultParser
    {
        private static readonly Regex BEGIN_VCARD = new Regex("BEGIN:VCARD", RegexOptions.IgnoreCase);
        private static readonly Regex VCARD_LIKE_DATE = new Regex(@"\d{4}-?\d{2}-?\d{2}");
        private static readonly Regex CR_LF_SPACE_TAB = new Regex("\r\n[ \t]");
        private static readonly Regex NEWLINE_ESCAPE = new Regex(@"\\[nN]");
        private static readonly Regex VCARD_ESCAPES = new Regex(@"\\([,;\\])");
        private static readonly Regex EQUALS = new Regex("=");
        private static readonly Regex SEMICOLON = new Regex(";");
        private static readonly Regex UNESCAPED_SEMICOLONS = new Regex(@"(?<!\\);+");
        private static readonly Regex COMMA = new Regex(",");
        private static readonly Regex SEMICOLON_OR_COMMA = new Regex("[;,]");

        public override ParsedResult Parse(string source)
        {
            var rawText = GetMassagedText(source);
            var m = BEGIN_VCARD.Match(rawText);
            if (!m.Success || m.Index != 0)
            {
                return null;
            }

            var names = MatchVCardPrefixedField("FN", rawText, true, false);
            if (names == null)
            {
                names = MatchVCardPrefixedField("N", rawText, true, false);
                FormatNames(names);
            }

            var nicknameString = MatchSingleVCardPrefixedField("NICKNAME", rawText, true, false);
            var nicknames = nicknameString == null ? null : COMMA.Split(nicknameString[0]);
            var phoneNumbers = MatchVCardPrefixedField("TEL", rawText, true, false);
            var emails = MatchVCardPrefixedField("EMAIL", rawText, true, false);
            var note = MatchSingleVCardPrefixedField("NOTE", rawText, false, false);
            var addresses = MatchVCardPrefixedField("ADR", rawText, true, true);
            var org = MatchSingleVCardPrefixedField("ORG", rawText, true, true);
            var birthday = MatchSingleVCardPrefixedField("BDAY", rawText, true, false);
            if (birthday != null && !IsLikeVCardDate(birthday[0]))
            {
                birthday = null;
            }

            var title = MatchSingleVCardPrefixedField("TITLE", rawText, true, false);
            var urls = MatchVCardPrefixedField("URL", rawText, true, false);
            var instantMessenger = MatchSingleVCardPrefixedField("IMPP", rawText, true, false);
            var geoString = MatchSingleVCardPrefixedField("GEO", rawText, true, false);
            var geo = geoString == null ? null : SEMICOLON_OR_COMMA.Split(geoString[0]);
            if (geo != null && geo.Length != 2)
            {
                geo = null;
            }

            return new AddressBookParsedResult(
                ToPrimaryValues(names),
                nicknames,
                null,
                ToPrimaryValues(phoneNumbers),
                ToTypes(phoneNumbers),
                ToPrimaryValues(emails),
                ToTypes(emails),
                ToPrimaryValue(instantMessenger),
                ToPrimaryValue(note),
                ToPrimaryValues(addresses),
                ToTypes(addresses),
                ToPrimaryValue(org),
                ToPrimaryValue(birthday),
                ToPrimaryValue(title),
                ToPrimaryValues(urls),
                geo
            );
        }

        private static List<List<string>> MatchVCardPrefixedField(string prefix, string rawText, bool trim, bool parseFieldDivider)
        {
            List<List<string>> matches = null;
            var i = 0;
            var max = rawText.Length;

            while (i < max)
            {
                var pattern = new Regex($"(?:^|\n){prefix}(?:;([^:]*))?:", RegexOptions.IgnoreCase);
                if (i > 0)
                {
                    i--;
                }

                var matcher = pattern.Match(rawText, i);
                if (!matcher.Success)
                {
                    break;
                }

                i = matcher.Index + matcher.Length;

                var metadataString = matcher.Groups[1].Value;
                List<string> metadata = null;
                var quotedPrintable = false;
                string quotedPrintableCharset = null;

                if (!string.IsNullOrEmpty(metadataString))
                {
                    foreach (var metadatum in SEMICOLON.Split(metadataString))
                    {
                        metadata ??= new List<string>();
                        metadata.Add(metadatum);

                        var metadatumTokens = EQUALS.Split(metadatum, 2);
                        if (metadatumTokens.Length > 1)
                        {
                            var key = metadatumTokens[0];
                            var value = metadatumTokens[1];
                            if ("ENCODING".Equals(key, StringComparison.OrdinalIgnoreCase) &&
                                "QUOTED-PRINTABLE".Equals(value, StringComparison.OrdinalIgnoreCase))
                            {
                                quotedPrintable = true;
                            }
                            else if ("CHARSET".Equals(key, StringComparison.OrdinalIgnoreCase))
                            {
                                quotedPrintableCharset = value;
                            }
                        }
                    }
                }

                var matchStart = i;

                while ((i = rawText.IndexOf('\n', i)) >= 0)
                {
                    if (i < rawText.Length - 1 && (rawText[i + 1] == ' ' || rawText[i + 1] == '\t'))
                    {
                        i += 2;
                    }
                    else if (quotedPrintable &&
                             ((i >= 1 && rawText[i - 1] == '=') ||
                              (i >= 2 && rawText[i - 2] == '=')))
                    {
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (i < 0)
                {
                    i = max;
                }
                else if (i > matchStart)
                {
                    matches ??= new List<List<string>>();

                    if (i >= 1 && rawText[i - 1] == '\r')
                    {
                        i--;
                    }

                    var element = rawText.Substring(matchStart, i - matchStart);
                    if (trim)
                    {
                        element = element.Trim();
                    }

                    if (quotedPrintable)
                    {
                        element = DecodeQuotedPrintable(element, quotedPrintableCharset);
                        if (parseFieldDivider)
                        {
                            element = UNESCAPED_SEMICOLONS.Replace(element, "\n").Trim();
                        }
                    }
                    else
                    {
                        if (parseFieldDivider)
                        {
                            element = UNESCAPED_SEMICOLONS.Replace(element, "\n").Trim();
                        }

                        element = CR_LF_SPACE_TAB.Replace(element, "");
                        element = NEWLINE_ESCAPE.Replace(element, "\n");
                        element = VCARD_ESCAPES.Replace(element, "$1");
                    }

                    if (metadata == null)
                    {
                        matches.Add(new List<string> { element });
                    }
                    else
                    {
                        metadata.Insert(0, element);
                        matches.Add(metadata);
                    }

                    i++;
                }
                else
                {
                    i++;
                }
            }

            return matches;
        }

        private static string DecodeQuotedPrintable(string value, string charset)
        {
            var length = value.Length;
            var result = new StringBuilder(length);
            var fragmentBuffer = new MemoryStream();

            for (var i = 0; i < length; i++)
            {
                var c = value[i];
                switch (c)
                {
                    case '\r':
                    case '\n':
                        break;
                    case '=':
                        if (i < length - 2)
                        {
                            var nextChar = value[i + 1];
                            if (nextChar != '\r' && nextChar != '\n')
                            {
                                var nextNextChar = value[i + 2];
                                var firstDigit = ParseHexDigit(nextChar);
                                var secondDigit = ParseHexDigit(nextNextChar);
                                if (firstDigit >= 0 && secondDigit >= 0)
                                {
                                    fragmentBuffer.WriteByte((byte)((firstDigit << 4) + secondDigit));
                                }

                                i += 2;
                            }
                        }

                        break;
                    default:
                        MaybeAppendFragment(fragmentBuffer, charset, result);
                        result.Append(c);
                        break;
                }
            }

            MaybeAppendFragment(fragmentBuffer, charset, result);
            return result.ToString();
        }

        private static void MaybeAppendFragment(MemoryStream fragmentBuffer, string charset, StringBuilder result)
        {
            if (fragmentBuffer.Length > 0)
            {
                string fragment;
                var fragmentBytes = fragmentBuffer.ToArray();
                try
                {
                    fragment = Encoding.GetEncoding(charset ?? "UTF-8").GetString(fragmentBytes);
                }
                catch
                {
                    fragment = Encoding.UTF8.GetString(fragmentBytes);
                }

                fragmentBuffer.SetLength(0);
                result.Append(fragment);
            }
        }

        private static int ParseHexDigit(char c)
        {
            if (char.IsDigit(c))
            {
                return c - '0';
            }

            if (char.IsLetter(c))
            {
                return char.ToUpper(c) - 'A' + 10;
            }

            return -1;
        }

        private static List<string> MatchSingleVCardPrefixedField(string prefix, string rawText, bool trim, bool parseFieldDivider)
        {
            var values = MatchVCardPrefixedField(prefix, rawText, trim, parseFieldDivider);
            return values == null || values.Count == 0 ? null : values[0];
        }

        private static string ToPrimaryValue(List<string> list)
        {
            return list == null || list.Count == 0 ? null : list[0];
        }

        private static string[] ToPrimaryValues(IEnumerable<List<string>> lists)
        {
            return lists?
                .Where(l => l.Count > 0 && !string.IsNullOrEmpty(l[0]))
                .Select(l => l[0])
                .ToArray();
        }

        private static string[] ToTypes(IEnumerable<List<string>> lists)
        {
            return lists?.Select(list =>
            {
                for (var i = 1; i < list.Count; i++)
                {
                    var metadatum = list[i];
                    var equals = metadatum.IndexOf('=');
                    if (equals < 0)
                    {
                        return metadatum;
                    }

                    if (metadatum.Substring(0, equals).Equals("TYPE", StringComparison.OrdinalIgnoreCase))
                    {
                        return metadatum.Substring(equals + 1);
                    }
                }

                return null;
            }).ToArray();
        }

        private static bool IsLikeVCardDate(string value)
        {
            return value == null || VCARD_LIKE_DATE.IsMatch(value);
        }

        private static void FormatNames(IEnumerable<List<string>> names)
        {
            if (names != null)
            {
                foreach (var list in names)
                {
                    var name = list[0];
                    var components = new string[5];
                    var start = 0;
                    var componentIndex = 0;

                    for (int end; componentIndex < 4 && (end = name.IndexOf(';', start)) >= 0; componentIndex++)
                    {
                        components[componentIndex] = name.Substring(start, end - start);
                        start = end + 1;
                    }

                    components[componentIndex] = name.Substring(start);

                    var newName = new StringBuilder();
                    MaybeAppendComponent(components, 3, newName);
                    MaybeAppendComponent(components, 1, newName);
                    MaybeAppendComponent(components, 2, newName);
                    MaybeAppendComponent(components, 0, newName);
                    MaybeAppendComponent(components, 4, newName);

                    list[0] = newName.ToString().Trim();
                }
            }
        }

        private static void MaybeAppendComponent(string[] components, int index, StringBuilder result)
        {
            if (!string.IsNullOrEmpty(components[index]))
            {
                if (result.Length > 0)
                {
                    result.Append(' ');
                }

                result.Append(components[index]);
            }
        }
    }
}