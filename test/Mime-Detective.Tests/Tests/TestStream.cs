﻿using System.IO;

namespace MimeDetective.Tests
{
    public class TestStream : Stream
    {
        private readonly Stream stream;

        public override bool CanRead => stream.CanRead;
        public override bool CanSeek => stream.CanSeek;
        public override bool CanWrite => stream.CanWrite;
        public override long Length => stream.Length;
        public override long Position { get => stream.Position; set => stream.Position = value; }
        public bool HasBeenDisposed { get; private set; }

        public TestStream(Stream stream)
        {
            this.stream = stream;
        }

        public override void Flush() => stream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
        public override void SetLength(long value) => stream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.HasBeenDisposed = true;
        }
    }
}
