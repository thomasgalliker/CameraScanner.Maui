using System.Text.RegularExpressions;

namespace CameraScanner.Maui
{
    /// <summary>
    /// Source: https://github.com/ig-emediplan/specification/blob/main/chtransmissionformat/README.md
    /// </summary>
    public sealed class EmediplanParser : ResultParser
    {
        private static readonly Regex ChmedPattern = new Regex("^CHMED([0-9]{2})([A-Z]+)(.*)$");

        public override ParsedResult Parse(string source)
        {
            var rawText = GetMassagedText(source);

            var match = ChmedPattern.Match(rawText);
            if (!match.Success)
            {
                return null;
            }

            var releaseYear = match.Groups[1].Value;
            var subVersion = match.Groups[2].Value;
            var additionalMetaData = match.Groups[3].Value;
            var data = match.Groups[4].Value;

            return new EmediplanParsedResult(rawText, releaseYear, subVersion, additionalMetaData, data);
        }
    }
}