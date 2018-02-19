using System.IO;
using Xunit;

namespace MimeDetective.Tests
{
    public class TrieTests
    {
        public static readonly Trie trie = new Trie(MimeTypes.Types);

        [Theory]
        [InlineData("./Data/Zip/empty.zip")]
        [InlineData("./Data/Zip/images.zip")]
        [InlineData("./Data/Zip/imagesBy7zip.zip")]
        public void BuildTrieSimpleLookUp(string path)
        {
            FileInfo file = new FileInfo(path);
            FileType type = null;

            using (FileStream stream = file.OpenRead())
            {
                ReadResult result = ReadResult.ReadFileHeader(stream);
                type = trie.Search(in result);
            }

            Assert.NotNull(type);
            Assert.Equal(MimeTypes.ZIP, type);
        }

        [Theory]
        [InlineData("./Data/images/test.gif", "gif")]
        public void BuildTrieWildCardLookup(string path, string ext)
        {
            FileInfo file = new FileInfo(path);
            FileType type = null;

            using (FileStream stream = file.OpenRead())
            {
                ReadResult result = ReadResult.ReadFileHeader(stream);
                type = trie.Search(in result);
            }

            Assert.NotNull(type);
            Assert.Contains(type.Extension, ext);
        }
    }
}
