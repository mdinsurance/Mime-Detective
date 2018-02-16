using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace MimeDetective
{
	internal static class ThrowHelpers
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void CannotReadFromStream(Stream stream)
		{
			throw new IOException("Could not read from Stream");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void StreamCannotBeNull(Stream stream)
		{
			throw new ArgumentNullException("Stream cannot be null");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void ByteArrayCannotBeNull(byte[] array)
		{
			throw new ArgumentNullException("Byte Array cannot be null");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void FileInfoCannotBeNull(FileInfo fileInfo)
		{
			throw new ArgumentNullException("File Info cannot be null");
		}
	}
}
