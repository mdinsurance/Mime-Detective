using System;

namespace MimeDetective.Analyzers
{
    internal static class ThrowHelpers
    {
        public static void GreaterThanMaxHeaderSize() => throw new ArgumentException("Offset cannot be greater than MaxHeaderSize - 1");

        public static void FileTypeArgumentIsNull() => throw new ArgumentNullException("FileType argument cannot be null");

        public static void FileTypeEnumerableIsNull() => throw new ArgumentNullException("FileType Enumerable cannot be null");
    }
}
