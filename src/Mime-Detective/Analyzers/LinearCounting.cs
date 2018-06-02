using System;
using System.Collections.Generic;
using System.Text;

namespace MimeDetective.Analyzers
{
    public class LinearCounting : IFileAnalyzer
    {
        private FileType[] types = new FileType[10];
        private int typesLength = 0;

        /// <summary>
        /// Constructs an empty LinearCountingAnalyzer, use <see cref="Insert(FileType)"/> to add file types
        /// </summary>
        public LinearCounting()
        {
        }

        /// <summary>
        /// Constructs a LinearCountingAnalyzer using the supplied IEnumerable<FileType>
        /// </summary>
        /// <param name="fileTypes"></param>
        public LinearCounting(IEnumerable<FileType> fileTypes)
        {
            if (fileTypes is null)
                ThrowHelpers.FileTypeEnumerableIsNull();

            foreach (var fileType in fileTypes)
            {
                if ((object)fileType != null)
                    Insert(fileType);
            }
        }

        public void Insert(FileType fileType)
        {
            if (fileType is null)
                ThrowHelpers.FileTypeArgumentIsNull();

            if (typesLength >= types.Length)
            {
                int newTypesCount = types.Length * 2 + 1;
                var newTypes = new FileType[newTypesCount];
                Array.Copy(types, newTypes, typesLength);
                types = newTypes;
            }

            types[typesLength] = fileType;
            typesLength++;
        }

        public FileType Search(in ReadResult readResult)
        {
            if (readResult.ReadLength == 0)
                return null;

            uint highestMatchingCount = 0;
            FileType highestMatchingType = null;

            // compare the file header to the stored file headers
            for (int typeIndex = 0; typeIndex < typesLength; typeIndex++)
            {
                FileType type = types[typeIndex];

                uint matchingCount = 0;
                int iOffset = type.HeaderOffset;
                int readEnd = iOffset + type.Header.Length;

                if (readEnd > readResult.ReadLength)
                    continue;

                for (int i = 0; iOffset < readEnd; i++, iOffset++)
                {
                    if (type.Header[i] is null || type.Header[i].Value == readResult.Array[iOffset])
                        matchingCount++;
                }

                //TODO should this be default behavior?
                if (type.Header.Length == matchingCount && matchingCount >= highestMatchingCount)
                {
                    highestMatchingType = type;
                    highestMatchingCount = matchingCount;
                }
            }

            return highestMatchingType;
        }
    }
}
