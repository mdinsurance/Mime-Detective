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
            using (ReadResult readResult = ReadResult.ReadHeaderFromStream(stream, shouldDisposeStream: false, shouldResetStreamPosition: true))
            {
                return MimeAnalyzers.GetFileType(in readResult, null, null);
            }
        }

        /// <summary>
        /// Read header of a stream and depending on the information in the header
        /// return object FileType.
        /// Return null in case when the file type is not identified.
        /// Throws Application exception if the file can not be read or does not exist
        /// </summary>
        /// <param name="file">The FileInfo object.</param>
        /// <returns>FileType or null not identified</returns>
        public static FileType GetFileType(this Stream stream, bool shouldDisposeStream = false, bool shouldResetStreamPosition = true, string mimeHint = null, string extensionHint = null)
        {
            using (ReadResult readResult = ReadResult.ReadHeaderFromStream(stream, shouldDisposeStream, shouldResetStreamPosition))
            {
                return MimeAnalyzers.GetFileType(in readResult, mimeHint, extensionHint);
            }
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
            using (ReadResult readResult = await ReadResult.ReadHeaderFromStreamAsync(stream, shouldDisposeStream: false, shouldResetStreamPosition: true))
            {
                return MimeAnalyzers.GetFileType(in readResult, null, null);
            }
        }

        /// <summary>
        /// Read header of a stream and depending on the information in the header
        /// return object FileType.
        /// Return null in case when the file type is not identified.
        /// Throws Application exception if the file can not be read or does not exist
        /// </summary>
        /// <param name="file">The FileInfo object.</param>
        /// <returns>FileType or null not identified</returns>
        public static async Task<FileType> GetFileTypeAsync(this Stream stream, bool shouldDisposeStream = false, bool shouldResetStreamPosition = true, string mimeHint = null, string extensionHint = null)
        {
            using (ReadResult readResult = await ReadResult.ReadHeaderFromStreamAsync(stream, shouldDisposeStream, shouldResetStreamPosition))
            {
                return MimeAnalyzers.GetFileType(in readResult, mimeHint, extensionHint);
            }
        }
    }
}