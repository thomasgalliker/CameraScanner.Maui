namespace CameraScanner.Maui
{
    public interface IBarcodeParser
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IBarcodeParser"/>.
        /// </summary>
        public static IBarcodeParser Current { get; } = BarcodeParser.Current;

        ParsedResult Parse(string source);
    }
}