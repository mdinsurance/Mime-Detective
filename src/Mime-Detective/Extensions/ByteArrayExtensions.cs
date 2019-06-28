namespace MimeDetective
{
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Read header of bytes and depending on the information in the header
        /// return object FileType.
        /// Return null in case when the file type is not identified.
        /// Throws Application exception if the file can not be read or does not exist
        /// </summary>
        /// <param name="file">The FileInfo object.</param>
        /// <returns>FileType or null not identified</returns>
        public static FileType GetFileType(this byte[] bytes, string mimeHint = null, string extensionHint = null)
        {
            int min = bytes.Length > MimeTypes.MaxHeaderSize ? MimeTypes.MaxHeaderSize : bytes.Length;
            using (ReadResult readResult = new ReadResult(bytes, min))
            {
                return MimeAnalyzers.GetFileType(in readResult, mimeHint, extensionHint);
            }
        }
    }
}
