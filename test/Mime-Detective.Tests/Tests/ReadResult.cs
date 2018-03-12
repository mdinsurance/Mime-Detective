using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Tests
{
    public class ReadResultTests
    {
        [Theory]
        [InlineData(100, 50)]
        [InlineData(560, 560)]
        [InlineData(1024, 1024)]
        [InlineData(1024, 560)]
        public void CreateFromArray(int arraySize, int readLength)
        {
            ReadResult readResult = new ReadResult(new byte[arraySize], readLength);

            Assert.NotNull(readResult.Array);
            Assert.NotEmpty(readResult.Array);
            Assert.StrictEqual(arraySize, readResult.Array.Length);
            Assert.StrictEqual(readLength, readResult.ReadLength);
            Assert.False(readResult.IsArrayRented);
            Assert.False(readResult.ShouldDisposeStream);
            Assert.False(readResult.ShouldResetStreamPosition);
            Assert.Null(readResult.Source);
        }

        [Fact]
        public void CreateFromNullArrayThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new ReadResult(null, 560));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(561)]
        [InlineData(1024)]
        public void CreateFromArrayReadLengthOutOfBoundsThrows(int readLength)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadResult(new byte[560], readLength));
        }

        [Fact]
        public void CreateFromArrayDispose()
        {
            ReadResult readResult = new ReadResult(new byte[560], 560);

            readResult.Dispose();

            Assert.Throws<ArgumentException>(() => ArrayPool<byte>.Shared.Return(readResult.Array));
        }

        public const String testFile = "./Data/Images/test.jpg";
        public const String nonExistingFile = "./Data/Images/supertest.jpg";

        [Fact]
        public void CreateFromFile()
        {
            ReadResult readResult = ReadResult.ReadFileHeader(new FileInfo(testFile));
            Assert.NotNull(readResult.Array);
            Assert.NotNull(readResult.Source);
            Assert.InRange(readResult.ReadLength, 1, readResult.Array.Length);
            Assert.True(readResult.IsArrayRented);
            Assert.True(readResult.ShouldDisposeStream);
            Assert.False(readResult.ShouldResetStreamPosition);
        }

        [Fact]
        public async Task CreateFromFileAsync()
        {
            ReadResult readResultAsync = await ReadResult.ReadFileHeaderAsync(new FileInfo(testFile));
            Assert.NotNull(readResultAsync.Array);
            Assert.NotNull(readResultAsync.Source);
            Assert.InRange(readResultAsync.ReadLength, 1, readResultAsync.Array.Length);
            Assert.True(readResultAsync.IsArrayRented);
            Assert.True(readResultAsync.ShouldDisposeStream);
            Assert.False(readResultAsync.ShouldResetStreamPosition);
        }

        [Fact]
        public async Task CreateFromNullFileThrows()
        {
            Assert.Throws<ArgumentNullException>(() => ReadResult.ReadFileHeader(null));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await ReadResult.ReadFileHeaderAsync(null));
        }

        [Fact]
        public async Task CreateFromFileThatDoesNotExistThrows()
        {
            Assert.Throws<FileNotFoundException>(() => ReadResult.ReadFileHeader(new FileInfo(nonExistingFile)));
            await Assert.ThrowsAsync<FileNotFoundException>(async () => await ReadResult.ReadFileHeaderAsync(new FileInfo(nonExistingFile)));
        }

        [Fact]
        public void CreateFromFileDispose()
        {
            ReadResult readResult = ReadResult.ReadFileHeader(new FileInfo(testFile));
            readResult.Dispose();

            Assert.Throws<ObjectDisposedException>(() => readResult.Source.ReadByte());
        }

        [Fact]
        public async Task CreateFromFileDisposeAsync()
        {
            ReadResult readResult = await ReadResult.ReadFileHeaderAsync(new FileInfo(testFile));
            readResult.Dispose();

            Assert.Throws<ObjectDisposedException>(() => readResult.Source.ReadByte());
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void CreateFromStream(bool shouldDisposeStream, bool resetStreamPos)
        {
            FileInfo file = new FileInfo(testFile);

            using (FileStream stream = file.OpenRead())
            {
                ReadResult readResult = ReadResult.ReadHeaderFromStream(stream, shouldDisposeStream, resetStreamPos);
                Assert.NotNull(readResult.Array);
                Assert.NotNull(readResult.Source);
                Assert.InRange(readResult.ReadLength, 1, readResult.Array.Length);
                Assert.True(readResult.IsArrayRented);
                Assert.StrictEqual(shouldDisposeStream, readResult.ShouldDisposeStream);
                Assert.StrictEqual(resetStreamPos, readResult.ShouldResetStreamPosition);
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task CreateFromStreamAsync(bool shouldDisposeStream, bool resetStreamPos)
        {
            FileInfo file = new FileInfo(testFile);

            using (FileStream stream = file.OpenRead())
            {
                ReadResult readResult = await ReadResult.ReadHeaderFromStreamAsync(stream, shouldDisposeStream, resetStreamPos);
                Assert.NotNull(readResult.Array);
                Assert.NotNull(readResult.Source);
                Assert.InRange(readResult.ReadLength, 1, readResult.Array.Length);
                Assert.True(readResult.IsArrayRented);
                Assert.StrictEqual(shouldDisposeStream, readResult.ShouldDisposeStream);
                Assert.StrictEqual(resetStreamPos, readResult.ShouldResetStreamPosition);
            }
        }

        [Fact]
        public async Task CreateFromNullStreamThrows()
        {
            Assert.Throws<ArgumentNullException>(() => ReadResult.ReadHeaderFromStream(null));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await ReadResult.ReadHeaderFromStreamAsync(null));
        }

        [Fact]
        public async Task CreateFromUnreadableStreamThrows()
        {
            Stream closedStream = new FileInfo(testFile).OpenRead();
            closedStream.Dispose();
            Assert.Throws<IOException>(() => ReadResult.ReadHeaderFromStream(closedStream));
            await Assert.ThrowsAsync<IOException>(async () => await ReadResult.ReadHeaderFromStreamAsync(closedStream));
        }

        //Stream should not be disposed and should be reset back to zero
        [Fact]
        public void CreateFromStreamDispose()
        {
            Stream stream = new FileInfo(testFile).OpenRead();
            ReadResult readResult = ReadResult.ReadHeaderFromStream(stream);

            readResult.Dispose();

            Assert.StrictEqual(0, readResult.Source.Position);

            int testRead = readResult.Source.ReadByte();
            readResult.Source.Dispose();
        }

        [Fact]
        public void CreateFromStreamDisposeShouldDisposeStream()
        {
            Stream stream = new FileInfo(testFile).OpenRead();
            ReadResult readResult = ReadResult.ReadHeaderFromStream(stream, shouldDisposeStream: true);

            readResult.Dispose();

            Assert.Throws<ObjectDisposedException>(() => readResult.Source.ReadByte());
        }

        [Fact]
        public async Task CreateFromStreamDisposeAsync()
        {
            Stream stream = new FileInfo(testFile).OpenRead();
            ReadResult readResult = await ReadResult.ReadHeaderFromStreamAsync(stream);

            readResult.Dispose();

            Assert.StrictEqual(0, readResult.Source.Position);

            int testRead = readResult.Source.ReadByte();
            readResult.Source.Dispose();
        }

        [Fact]
        public async Task CreateFromStreamAsyncDisposeShouldDisposeStream()
        {
            Stream stream = new FileInfo(testFile).OpenRead();
            ReadResult readResult = await ReadResult.ReadHeaderFromStreamAsync(stream, shouldDisposeStream: true);

            readResult.Dispose();

            Assert.Throws<ObjectDisposedException>(() => readResult.Source.ReadByte());
        }
    }
}
