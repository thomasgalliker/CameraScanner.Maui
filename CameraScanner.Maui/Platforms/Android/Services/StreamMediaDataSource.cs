namespace CameraScanner.Maui.Platforms.Services
{
    internal class StreamMediaDataSource : global::Android.Media.MediaDataSource
    {
        private Stream data;

        public StreamMediaDataSource(Stream data)
        {
            this.data = data;
        }

        public override long Size => this.data.Length;

        public override int ReadAt(long position, byte[]? buffer, int offset, int size)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            if (this.data.CanSeek)
            {
                this.data.Seek(position, SeekOrigin.Begin);
            }

            return this.data.Read(buffer, offset, size);
        }

        public override void Close()
        {
            this.data.Dispose();
            this.data = Stream.Null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.data.Dispose();
            this.data = Stream.Null;
        }
    }
}