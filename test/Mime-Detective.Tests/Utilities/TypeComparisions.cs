using MimeDetective;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Utilities
{
    public static class TypeComparisions
    {
        public static FileInfo GetFileInfo(string dataPath, string file)
        {
            return new FileInfo(Path.Combine(dataPath, file));
        }

        public static FileInfo GetFileInfo(string dataPath, string file, string ext)
        {
            return new FileInfo(Path.Combine(dataPath, file + ext));
        }

        public static async Task AssertIsType(FileInfo info, FileType type)
        {
            Assert.Equal(type, await info.GetFileTypeAsync());

            Assert.Equal(info.GetFileType(), type);

            Assert.True(info.IsType(type));

            Assert.True(info.GetFileType().Equals(type));

            Assert.True(info.GetFileType() == type);

            Assert.False(info.GetFileType() != type);
        }
    }
}