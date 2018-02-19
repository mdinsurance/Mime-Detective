using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace MimeDetective
{
    //todo if the read fails the array rented below may leak
    public readonly struct ReadResult
    {
        public readonly byte[] Array;
        public readonly Stream Source;
        public readonly int ReadLength;
        public readonly bool IsArrayRented;
        public readonly bool ShouldDisposeStream;

        /// <summary>
        /// Non rented array input, Array is Input
        /// </summary>
        /// <param name="array"></param>
        /// <param name="readLength"></param>
        public ReadResult(byte[] array, int readLength)
        {
            this.Array = array;
            this.Source = null;
            this.ReadLength = readLength;
            this.IsArrayRented = false;
            this.ShouldDisposeStream = true;
        }

        private ReadResult(byte[] array, Stream source, int readLength, bool isArrayRented, bool shouldDisposeStream = true)
        {
            this.Array = array;
            this.Source = source;
            this.ReadLength = readLength;
            this.IsArrayRented = isArrayRented;
            this.ShouldDisposeStream = shouldDisposeStream;
        }

        /// <summary>
        /// Reads the file header - first (16) bytes from the file
        /// </summary>
        /// <param name="file">The file to work with</param>
        /// <returns>Array of bytes</returns>
        public static ReadResult ReadFileHeader(FileStream fileStream)
        {
            byte[] header = ArrayPool<byte>.Shared.Rent(MimeTypes.MaxHeaderSize);

            int bytesRead = fileStream.Read(header, 0, MimeTypes.MaxHeaderSize);

            return new ReadResult(header, fileStream, bytesRead, isArrayRented: true, shouldDisposeStream: true);
        }

        internal static async Task<ReadResult> ReadFileHeaderAsync(FileStream fileStream)
        {
            byte[] header = ArrayPool<byte>.Shared.Rent(MimeTypes.MaxHeaderSize);

            int bytesRead = await fileStream.ReadAsync(header, 0, MimeTypes.MaxHeaderSize);

            return new ReadResult(header, fileStream, bytesRead, isArrayRented: true, shouldDisposeStream: true);
        }

        /// <summary>
        /// Takes a stream does, not dispose of stream, resets read position to beginning though
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="MaxHeaderSize"></param>
        /// <returns></returns>
        internal static ReadResult ReadHeaderFromStream(Stream stream, bool shouldDisposeStream)
        {
            if (!stream.CanRead)
                ThrowHelpers.CannotReadFromStream(stream);

            if (stream.Position > 0)
                stream.Seek(0, SeekOrigin.Begin);

            byte[] header = ArrayPool<byte>.Shared.Rent(MimeTypes.MaxHeaderSize);

            int bytesRead = stream.Read(header, 0, MimeTypes.MaxHeaderSize);

            return new ReadResult(header, stream, bytesRead, isArrayRented: true, shouldDisposeStream);
        }

        /// <summary>
        /// Takes a stream does, not dispose of stream, resets read position to beginning though
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="MaxHeaderSize"></param>
        /// <returns></returns>
        internal static async Task<ReadResult> ReadHeaderFromStreamAsync(Stream stream, bool shouldDisposeStream)
        {
            if (!stream.CanRead)
                ThrowHelpers.CannotReadFromStream(stream);

            if (stream.Position > 0)
                stream.Seek(0, SeekOrigin.Begin);

            byte[] header = ArrayPool<byte>.Shared.Rent(MimeTypes.MaxHeaderSize);

            int bytesRead = await stream.ReadAsync(header, 0, MimeTypes.MaxHeaderSize);

            return new ReadResult(header, stream, bytesRead, isArrayRented: true, shouldDisposeStream);
        }
    }
}