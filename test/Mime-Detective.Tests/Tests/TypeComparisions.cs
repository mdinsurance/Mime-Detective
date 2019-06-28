using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Utilities
{
    public static class TypeComparisions
    {
        public static FileInfo GetFileInfo(string dataPath, string file) => new FileInfo(Path.Combine(dataPath, file));

        public static FileInfo GetFileInfo(string dataPath, string file, string ext) => new FileInfo(Path.Combine(dataPath, file + ext));

        public static async Task AssertIsType(FileInfo info, FileType type)
        {
            Assert.Equal(type, await info.GetFileTypeAsync());

            Assert.Equal(type, info.GetFileType());

            Assert.True(info.IsType(type));

            Assert.True(info.GetFileType().Equals(type));

            Assert.True(info.GetFileType() == type);

            Assert.False(info.GetFileType() != type);
        }
    }
}