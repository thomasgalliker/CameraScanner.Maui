namespace CameraScanner.Maui
{
    public interface IBarcodeParser
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IBarcodeParser"/>.
        /// </summary>
        public static IBarcodeParser Current { get; } = BarcodeParser.Current;

        ParsedResult Parse(string source);

        /// <summary>
        /// The collection of <see cref="ResultParser"/> used to parse a barcode string into a <see cref="ParsedResult"/>.
        /// </summary>
        ICollection<ResultParser> Parsers { get; }
    }
}