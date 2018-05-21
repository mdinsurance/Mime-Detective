using System;
using System.Runtime.InteropServices;

namespace MimeDetective
{
    /// <summary>
    /// Data Structure to hold information about file types.
    /// Holds information about binary header at the start of the file
    /// </summary>
    public class FileType : IEquatable<FileType>
    {
        public byte?[] Header { get; }

        public ushort HeaderOffset { get; }

        public string Extension { get; }

        public string Mime { get; }

        private readonly int hashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileType"/> class
        /// Takes the details of offset for the header
        /// </summary>
        /// <param name="header">Byte array with header.</param>
        /// <param name="offset">The header offset - how far into the file we need to read the header</param>
        /// <param name="extension">String with extension.</param>
        /// <param name="mime">The description of MIME.</param>
        public FileType(byte?[] header, string extension, string mime, ushort offset = 0)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header), $"cannot be null, {nameof(FileType)} needs file header data");

            if (offset > (MimeTypes.MaxHeaderSize - 1))
                throw new ArgumentException($"Header Offset cannot exceed Max Header Size {MimeTypes.MaxHeaderSize} - 1");

            HeaderOffset = offset;
            Extension = extension;
            Mime = mime;

            hashCode = (base.GetHashCode() ^ Header.GetHashCode() ^ HeaderOffset ^ Extension.GetHashCode() ^ Mime.GetHashCode());
        }

        public static bool operator == (FileType a, FileType b)
        {
            if (a is null && b is null)
                return true;

            if (b is null)
                return a.Equals(b);
            
            return b.Equals(a);
        }

        public static bool operator !=(FileType a, FileType b) => !(a == b);

        public override bool Equals(object other)
        {
            if (other is null)
                return false;

            if (other is FileType type
                && HeaderOffset == type.HeaderOffset
                && Extension.Equals(type.Extension)
                && Mime.Equals(type.Mime)
                && CompareHeaders(Header, type.Header))
                    return true;

            return false;
        }

        public bool Equals(FileType other)
        {
            if (other is null)
                return false;

            if (HeaderOffset == other.HeaderOffset
                && Extension.Equals(other.Extension)
                && Mime.Equals(other.Mime)
                && CompareHeaders(Header, other.Header))
                return true;

            return false;
        }

        private static bool CompareHeaders(byte?[] array1, byte?[] array2)
        {
            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
                if (array1[i] != array2[i])
                    return false;

            return true;
        }

        public override int GetHashCode() => hashCode;

        public override string ToString() => Extension;
    }
}