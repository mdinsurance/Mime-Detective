using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MimeDetective
{
    //TODO document when Stream Dispose and ShouldReset are used in IDispose
    //TODO handle if stream cannot seek
    //TODO if the read fails resources may leak below
    /// <summary>
    /// 
    /// Layout of this structure will be prone to change (aka plan to back properties with a flags enum)
    /// </summary>
    public readonly struct ReadResult : IDisposable
    {
        /// <summary>
        /// Use Array.Length instead of ReadLength when you are refering to the whole file and source is null
        /// </summary>
        public readonly byte[] Array;

        public readonly Stream Source;

        /// <summary>
        /// This is meant to be the int result of Stream.Read, it should cap at 560
        /// Do not use when referring to the whole file
        /// </summary>
        public readonly int ReadLength;

        public bool IsArrayRented { get; }

        public bool ShouldDisposeStream { get; }

        public bool ShouldResetStreamPosition { get; }

        /// <summary>
        /// Non rented array input, Array is Input
        /// </summary>
        /// <param name="array"></param>
        /// <param name="readLength"></param>
        public ReadResult(byte[] array, int readLength)
        {
            if (array is null)
            {
                ThrowHelpers.ByteArrayCannotBeNull();
            }

            if ((uint)readLength > (uint)array.Length)
            {
                ThrowHelpers.ReadLengthCannotBeOutOfBounds();
            }

            this.Array = array;
            this.Source = null;
            this.ReadLength = readLength;
            this.IsArrayRented = false;
            this.ShouldDisposeStream = false;
            this.ShouldResetStreamPosition = false;
        }

        private ReadResult(byte[] array, Stream source, int readLength, bool isArrayRented, bool shouldDisposeStream, bool shouldResetStreamPosition)
        {
            this.Array = array;
            this.Source = source;
            this.ReadLength = readLength;
            this.IsArrayRented = isArrayRented;
            this.ShouldDisposeStream = shouldDisposeStream;
            this.ShouldResetStreamPosition = shouldResetStreamPosition;
        }

        /// <summary>
        /// Reads the file header - first (16) bytes from the file
        /// </summary>
        /// <param name="file">The file to work with</param>
        /// <returns>Array of bytes</returns>
        public static ReadResult ReadFileHeader(FileInfo file)
        {
            if (file is null)
            {
                ThrowHelpers.FileInfoCannotBeNull();
            }

            if (!file.Exists)
            {
                ThrowHelpers.FileDoesNotExist(file);
            }

            var fileStream = file.OpenRead();

            var header = ArrayPool<byte>.Shared.Rent(MimeTypes.MaxHeaderSize);

            var bytesRead = fileStream.Read(header, 0, MimeTypes.MaxHeaderSize);

            return new ReadResult(header, fileStream, bytesRead, isArrayRented: true, shouldDisposeStream: true, shouldResetStreamPosition: false);
        }

        public static async Task<ReadResult> ReadFileHeaderAsync(FileInfo file)
        {
            if (file is null)
            {
                ThrowHelpers.FileInfoCannotBeNull();
            }

            if (!file.Exists)
            {
                ThrowHelpers.FileDoesNotExist(file);
            }

            var fileStream = file.OpenRead();

            var header = ArrayPool<byte>.Shared.Rent(MimeTypes.MaxHeaderSize);

            var bytesRead = await fileStream.ReadAsync(header, 0, MimeTypes.MaxHeaderSize);

            return new ReadResult(header, fileStream, bytesRead, isArrayRented: true, shouldDisposeStream: true, shouldResetStreamPosition: false);
        }

        /// <summary>
        /// Takes a stream does, not dispose of stream, resets read position to beginning though
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="MaxHeaderSize"></param>
        /// <returns></returns>
        public static ReadResult ReadHeaderFromStream(Stream stream, bool shouldDisposeStream = false, bool shouldResetStreamPosition = true)
        {
            if (stream is null)
            {
                ThrowHelpers.StreamCannotBeNull();
            }

            if (!stream.CanRead)
            {
                ThrowHelpers.CannotReadFromStream();
            }

            if (stream.CanSeek && stream.Position > 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var header = ArrayPool<byte>.Shared.Rent(MimeTypes.MaxHeaderSize);

            var bytesRead = stream.Read(header, 0, MimeTypes.MaxHeaderSize);

            return new ReadResult(header, stream, bytesRead, isArrayRented: true, shouldDisposeStream, shouldResetStreamPosition);
        }

        //TODO Figure out how to handle non-seekable Streams
        /// <summary>
        /// Takes a stream does, not dispose of stream, resets read position to beginning though
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="MaxHeaderSize"></param>
        /// <returns></returns>
        public static async Task<ReadResult> ReadHeaderFromStreamAsync(Stream stream, bool shouldDisposeStream = false, bool shouldResetStreamPosition = true)
        {
            if (stream is null)
            {
                ThrowHelpers.StreamCannotBeNull();
            }

            if (!stream.CanRead)
            {
                ThrowHelpers.CannotReadFromStream();
            }

            if (stream.CanSeek && stream.Position > 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var header = ArrayPool<byte>.Shared.Rent(MimeTypes.MaxHeaderSize);

            var bytesRead = await stream.ReadAsync(header, 0, MimeTypes.MaxHeaderSize);

            return new ReadResult(header, stream, bytesRead, isArrayRented: true, shouldDisposeStream, shouldResetStreamPosition);
        }

        /// <summary>
        /// Returns a Stream at position zero that will correctly dispose if necessary.
        /// </summary>
        /// <returns></returns>
        public ReadResultStream GetStream()
        {
            ReadResultStream resultStream;

            if (this.Source is null)
            {
                resultStream = new ReadResultStream(this.Array);
            }
            else
            {
                resultStream = new ReadResultStream(this.Source);
            }

            if (resultStream.CanSeek && resultStream.Position > 0)
            {
                resultStream.Seek(0, SeekOrigin.Begin);
            }

            return resultStream;
        }

        public void Dispose()
        {
            var sourceIsNotNull = !(this.Source is null);

            if (sourceIsNotNull && this.ShouldResetStreamPosition && this.Source.CanSeek)
            {
                this.Source.Seek(0, SeekOrigin.Begin);
            }

            if (sourceIsNotNull && this.ShouldDisposeStream)
            {
                this.Source.Dispose();
            }

            if (this.IsArrayRented)
            {
                ArrayPool<byte>.Shared.Return(this.Array);
            }
        }
    }

    public class ReadResultStream : Stream
    {
        readonly bool disposeStream = false;
        Stream stream;

        public ReadResultStream(Stream s)
        {
            stream = s;
            disposeStream = false;
        }

        public ReadResultStream(byte[] bytes)
        {
            stream = new MemoryStream(bytes);
            disposeStream = true;
        }

        public override bool CanRead => stream.CanRead;
        public override bool CanSeek => stream.CanSeek;
        public override bool CanWrite => stream.CanWrite;
        public override long Length => stream.Length;
        public override long Position { get => stream.Position; set => stream.Position = value; }

        public override void Flush() => stream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
        public override void SetLength(long value) => stream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => stream.BeginRead(buffer, offset, count, callback, state);

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => stream.BeginWrite(buffer, offset, count, callback, state);

        public override bool CanTimeout => stream.CanTimeout;

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => stream.CopyToAsync(destination, bufferSize, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.disposeStream)
            {
                this.stream?.Dispose();
                this.stream = null;
            }
        }

        public override int EndRead(IAsyncResult asyncResult) => stream.EndRead(asyncResult);

        public override void EndWrite(IAsyncResult asyncResult) => stream.EndWrite(asyncResult);

        public override Task FlushAsync(CancellationToken cancellationToken) => stream.FlushAsync(cancellationToken);

        public override object InitializeLifetimeService() => stream.InitializeLifetimeService();

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => stream.ReadAsync(buffer, offset, count, cancellationToken);

        public override int ReadByte() => stream.ReadByte();

        public override int ReadTimeout { get => stream.ReadTimeout; set => stream.ReadTimeout = value; }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => stream.WriteAsync(buffer, offset, count, cancellationToken);

        public override void WriteByte(byte value) => stream.WriteByte(value);

        public override int WriteTimeout { get => stream.WriteTimeout; set => stream.WriteTimeout = value; }
    }
}