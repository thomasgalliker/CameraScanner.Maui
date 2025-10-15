namespace CameraScanner.Maui.Extensions
{
    internal static class StreamExtensions
    {
        public static T Rewind<T>(this T stream) where T : Stream
        {
            if (stream.Position != 0 && stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            return stream;
        }
    }
}