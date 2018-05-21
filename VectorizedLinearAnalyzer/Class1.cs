using MimeDetective;
using MimeDetective.Analyzers;
using System;
using System.Numerics;

namespace VectorizedLinearAnalyzer
{
    public class VectorizedLinearAnalyzer : IFileAnalyzer
    {
        public void Insert(FileType fileType)
        {
            throw new NotImplementedException();
        }

        public FileType Search(in ReadResult readResult)
        {
            if (readResult.ReadLength == 0)
                return null;

            uint highestMatchingCount = 0;
            FileType highestMatchingType = null;

            // compare the file header to the stored file headers
            for (int typeIndex = 0; typeIndex < MimeTypes.Types.Length; typeIndex++)
            {
                FileType type = MimeTypes.Types[typeIndex];

                uint matchingCount = 0;
                int iOffset = type.HeaderOffset;
                int readLength = iOffset + type.Header.Length;

                if (readLength > readResult.ReadLength)
                    continue;

                for (int i = 0; iOffset < readLength; i++, iOffset++)
                {
                    if (type.Header[i] is null || type.Header[i].Value == readResult.Array[iOffset])
                        matchingCount++;
                }

                if (type.Header.Length == matchingCount && matchingCount > highestMatchingCount)
                {
                    highestMatchingType = type;
                    highestMatchingCount = matchingCount;
                }
            }

            return highestMatchingType;
        }
    }
}
