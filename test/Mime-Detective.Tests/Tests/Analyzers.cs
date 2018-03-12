using MimeDetective.Analyzers;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Tests
{
    public class Analyzers
    {
        public static readonly LinearCountingAnalyzer countingAnalyzer = new LinearCountingAnalyzer(MimeTypes.Types);
        public static readonly DictionaryBasedTrie dictTrie = new DictionaryBasedTrie(MimeTypes.Types);
        public static readonly ArrayBasedTrie arrayTrie = new ArrayBasedTrie(MimeTypes.Types);

        
        public void LinearCountingAnalyzerConstructors()
        {

        }


        public void LinearCountingAnalyzerInsert()
        {

        }

        [Theory]
        [InlineData("./Data/Documents/XlsExcel2016.xls", "xls")]
        [InlineData("./Data/Documents/PptPowerpoint2016.ppt", "ppt")]
        [InlineData("./Data/Documents/DocWord2016.doc", "doc")]
        [InlineData("./Data/Documents/PdfWord2016.pdf", "pdf")]
        [InlineData("./Data/Zip/empty.zip", "zip")]
        [InlineData("./Data/Zip/images.zip", "zip")]
        [InlineData("./Data/Zip/imagesBy7zip.zip", "zip")]
        [InlineData("./Data/images/test.gif", "gif")]
        [InlineData("./Data/Audio/wavVLC.wav", "wav")]
        public async Task LinearCountingAnalyzerSearch(string path, string ext)
        {
            FileInfo file = new FileInfo(path);
            FileType type = null;

            using (ReadResult result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = countingAnalyzer.Search(in result);
            }

            Assert.NotNull(type);
            Assert.Contains(ext, type.Extension);
        }

        [Theory]
        [InlineData("./Data/Documents/XlsExcel2016.xls", "xls")]
        [InlineData("./Data/Documents/PptPowerpoint2016.ppt", "ppt")]
        [InlineData("./Data/Documents/DocWord2016.doc", "doc")]
        [InlineData("./Data/Documents/PdfWord2016.pdf", "pdf")]
        [InlineData("./Data/Zip/empty.zip", "zip")]
        [InlineData("./Data/Zip/images.zip", "zip")]
        [InlineData("./Data/Zip/imagesBy7zip.zip", "zip")]
        [InlineData("./Data/images/test.gif", "gif")]
        [InlineData("./Data/Audio/wavVLC.wav", "wav")]
        public async Task DictionaryBasedTrieSearch(string path, string ext)
        {
            FileInfo file = new FileInfo(path);
            FileType type = null;

            using (ReadResult result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = dictTrie.Search(in result);
            }

            Assert.NotNull(type);
            Assert.Contains(ext, type.Extension);
        }


        [Theory]
        [InlineData("./Data/Documents/XlsExcel2016.xls", "xls")]
        [InlineData("./Data/Documents/PptPowerpoint2016.ppt", "ppt")]
        [InlineData("./Data/Documents/DocWord2016.doc", "doc")]
        [InlineData("./Data/Documents/PdfWord2016.pdf", "pdf")]
        [InlineData("./Data/Zip/empty.zip", "zip")]
        [InlineData("./Data/Zip/images.zip", "zip")]
        [InlineData("./Data/Zip/imagesBy7zip.zip", "zip")]
        [InlineData("./Data/images/test.gif", "gif")]
        [InlineData("./Data/Audio/wavVLC.wav", "wav")]
        public async Task ArrayBasedTrieSearch(string path, string ext)
        {
            FileInfo file = new FileInfo(path);
            FileType type = null;

            using (ReadResult result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = arrayTrie.Search(in result);
            }

            Assert.NotNull(type);
            Assert.Contains(ext, type.Extension);
        }
    }
}
