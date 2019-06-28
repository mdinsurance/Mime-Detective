using System;
using System.Collections.Generic;

namespace MimeDetective.Analyzers
{
    public class LinearCounting : IFileAnalyzer
    {
        private FileType[] types = new FileType[20];
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
            {
                ThrowHelpers.FileTypeEnumerableIsNull();
            }

            foreach (var fileType in fileTypes)
            {
                if (!(fileType is null))
                {
                    this.Insert(fileType);
                }
            }

            //types.OrderBy(x => x.HeaderOffset);
            //todo sort
            //Array.Sort<FileType>(types, (x,y) => x.HeaderOffset.CompareTo(y.HeaderOffset));
            //types = types;
        }

        public void Insert(FileType fileType)
        {
            if (fileType is null)
            {
                ThrowHelpers.FileTypeArgumentIsNull();
            }

            if (this.typesLength >= this.types.Length)
            {
                var newTypesCount = this.types.Length * 2;
                var newTypes = new FileType[newTypesCount];
                Array.Copy(this.types, newTypes, this.typesLength);
                this.types = newTypes;
            }

            this.types[this.typesLength] = fileType;
            this.typesLength++;
        }

        public FileType Search(in ReadResult readResult, string mimeHint = null, string extensionHint = null)
        {
            uint highestMatchingCount = 0;
            FileType highestMatchingType = null;

            // compare the file header to the stored file headers
            for (var typeIndex = 0; typeIndex < this.typesLength; typeIndex++)
            {
                var type = this.types[typeIndex];
                uint matchingCount = 0;

                for (int i = 0, iOffset = type.HeaderOffset; iOffset < readResult.ReadLength && i < type.Header.Length; i++, iOffset++)
                {
                    if (type.Header[i] is null || type.Header[i].Value == readResult.Array[iOffset])
                    {
                        matchingCount++;
                    }
                    else
                    {
                        break;
                    }
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
