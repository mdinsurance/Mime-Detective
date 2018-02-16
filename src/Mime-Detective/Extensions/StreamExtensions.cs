using System;
using System.IO;
using System.Threading.Tasks;

namespace MimeDetective.Extensions
{
	public static class StreamExtensions
	{
		/// <summary>
		/// Read header of a stream and depending on the information in the header
		/// return object FileType.
		/// Return null in case when the file type is not identified.
		/// Throws Application exception if the file can not be read or does not exist
		/// </summary>
		/// <param name="file">The FileInfo object.</param>
		/// <returns>FileType or null not identified</returns>
		public static FileType GetFileType(this Stream stream)
		{
			if (stream is null)
				ThrowHelpers.StreamCannotBeNull(stream);

			ReadResult readResult = ReadResult.ReadHeaderFromStream(stream, shouldDisposeStream: false);

			return MimeTypes.GetFileType(in readResult);
		}

		/// <summary>
		/// Read header of a stream and depending on the information in the header
		/// return object FileType.
		/// Return null in case when the file type is not identified.
		/// Throws Application exception if the file can not be read or does not exist
		/// </summary>
		/// <param name="file">The FileInfo object.</param>
		/// <returns>FileType or null not identified</returns>
		public static FileType GetFileType(this Stream stream, bool shouldDisposeStream = false)
		{
			if (stream is null)
				ThrowHelpers.StreamCannotBeNull(stream);

			ReadResult readResult = ReadResult.ReadHeaderFromStream(stream, shouldDisposeStream);

			return MimeTypes.GetFileType(in readResult);
		}

		/// <summary>
		/// Read header of a stream and depending on the information in the header
		/// return object FileType.
		/// Return null in case when the file type is not identified.
		/// Throws Application exception if the file can not be read or does not exist
		/// </summary>
		/// <param name="file">The FileInfo object.</param>
		/// <returns>FileType or null not identified</returns>
		public static async Task<FileType> GetFileTypeAsync(this Stream stream)
		{
			if (stream is null)
				ThrowHelpers.StreamCannotBeNull(stream);

			ReadResult readResult = await ReadResult.ReadHeaderFromStreamAsync(stream, shouldDisposeStream: false);

			return MimeTypes.GetFileType(in readResult);
		}

		/// <summary>
		/// Read header of a stream and depending on the information in the header
		/// return object FileType.
		/// Return null in case when the file type is not identified.
		/// Throws Application exception if the file can not be read or does not exist
		/// </summary>
		/// <param name="file">The FileInfo object.</param>
		/// <returns>FileType or null not identified</returns>
		public static async Task<FileType> GetFileTypeAsync(this Stream stream, bool shouldDisposeStream = false)
		{
			if (stream is null)
				ThrowHelpers.StreamCannotBeNull(stream);

			ReadResult readResult = await ReadResult.ReadHeaderFromStreamAsync(stream, shouldDisposeStream);

			return MimeTypes.GetFileType(in readResult);
		}
	}
}