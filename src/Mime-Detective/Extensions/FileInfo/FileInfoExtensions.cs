﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MimeDetective
{
    public static partial class FileInfoExtensions
    {
        /// <summary>
        /// Read header of a file and depending on the information in the header
        /// return object FileType.
        /// Return null in case when the file type is not identified.
        /// Throws Application exception if the file can not be read or does not exist
        /// </summary>
        /// <param name="file">The FileInfo object.</param>
        /// <returns>FileType or null (for no match)</returns>
        public static FileType GetFileType(this FileInfo file, string mimeHint = null, string extensionHint = null)
        {
            using (var readResult = ReadResult.ReadFileHeader(file))
            {
                return MimeAnalyzers.GetFileType(in readResult, mimeHint, extensionHint);
            }
        }

        /// <summary>
        /// Read header of a file and depending on the information in the header
        /// return object FileType.
        /// Return null in case when the file type is not identified.
        /// Throws Application exception if the file can not be read or does not exist
        /// </summary>
        /// <param name="file">The FileInfo object.</param>
        /// <returns>FileType or null (for no match)</returns>
        public static async Task<FileType> GetFileTypeAsync(this FileInfo file, string mimeHint = null, string extensionHint = null)
        {
            using (var readResult = await ReadResult.ReadFileHeaderAsync(file))
            {
                return MimeAnalyzers.GetFileType(in readResult, mimeHint, extensionHint);
            }
        }

        /// <summary>
        /// Determines whether provided file belongs to one of the provided list of files
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="requiredTypes">The required types.</param>
        /// <returns>
        ///   <c>true</c> if file of the one of the provided types; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFileOfTypes(this FileInfo file, List<FileType> requiredTypes)
        {
            var currentType = file.GetFileType();

            //TODO Write a test to check if this null check is correct
            if (currentType.Mime == null)
            {
                return false;
            }

            return requiredTypes.Contains(currentType);
        }

        /// <summary>
        /// Determines whether provided file belongs to one of the provided list of files,
        /// where list of files provided by string with Comma-Separated-Values of extensions
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="requiredTypes">The required types.</param>
        /// <returns>
        ///   <c>true</c> if file of the one of the provided types; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFileOfTypes(this FileInfo file, string CSV)
        {
            var providedTypes = MimeTypes.GetFileTypesByExtensions(CSV);

            return file.IsFileOfTypes(providedTypes);
        }

        /// <summary>
        /// Determines whether the specified file is of provided type
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="type">The FileType</param>
        /// <returns>
        ///   <c>true</c> if the specified file is type; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsType(this FileInfo file, FileType type)
        {
            var actualType = GetFileType(file);

            //TODO Write a test to check if this null check is correct
            if (actualType.Mime is null)
            {
                return false;
            }

            return (actualType.Equals(type));
        }

        /// <summary>
        /// Checks if the file is executable file .exe
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns>
        ///     <c>true</c> if the specified file is type; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsExe(this FileInfo fileInfo) => fileInfo.IsType(MimeTypes.DLL_EXE);

        /// <summary>
        /// Check if the file is Microsoft Installer.
        /// Beware, many Microsoft file types are starting with the same header.
        /// So use this one with caution. If you think the file is MSI, just need to confirm, use this method.
        /// But it could be MSWord or MSExcel, or Powerpoint...
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns>
        ///     <c>true</c> if the specified file is type; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMsi(this FileInfo fileInfo)
        {
            // MSI has a generic DOCFILE header. Also it matches PPT files
            return fileInfo.IsType(MimeTypes.PPT) || fileInfo.IsType(MimeTypes.MS_OFFICE);
        }
    }
}
