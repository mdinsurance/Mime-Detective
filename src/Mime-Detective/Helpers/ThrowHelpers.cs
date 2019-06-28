using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace MimeDetective
{
    internal static class ThrowHelpers
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CannotReadFromStream() => throw new IOException("Could not read from Stream");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void StreamCannotBeNull() => throw new ArgumentNullException("Stream cannot be null");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ByteArrayCannotBeNull() => throw new ArgumentNullException("Byte Array cannot be null");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ReadLengthCannotBeOutOfBounds() => throw new ArgumentOutOfRangeException("Read Length cannot be out of bound of Array");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileInfoCannotBeNull() => throw new ArgumentNullException("File Info cannot be null");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileDoesNotExist(FileInfo fileInfo) => throw new FileNotFoundException($"File: {fileInfo.Name} does not exist in Directory: {fileInfo.Directory}");
    }
}
