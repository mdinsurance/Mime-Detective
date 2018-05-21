using MimeDetective.Analyzers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Tests.Analyzers
{
    public class DictionaryBasedTrieTests
    {
        [Fact]
        public void DefaultConstructor()
        {
            var analyzer = new DictionaryBasedTrie();

            //assertion here just to have
            Assert.NotNull(analyzer);

            analyzer.Insert(MimeTypes.ZIP);
        }

        [Fact]
        public void EnumerableConstructor()
        {
            var analyzer = new DictionaryBasedTrie(MimeTypes.Types);

            //assertion here just to have
            Assert.NotNull(analyzer);
            Assert.Throws<ArgumentNullException>(() => new DictionaryBasedTrie(null));

            analyzer.Insert(MimeTypes.WORD);
        }

        [Fact]
        public void Insert()
        {
            var analyzer = new DictionaryBasedTrie();
            Assert.Throws<ArgumentNullException>(() => analyzer.Insert(null));

            foreach (var fileType in MimeTypes.Types)
            {
                analyzer.Insert(fileType);
            }

            analyzer.Insert(MimeTypes.WORD);
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
        [InlineData("./Data/Audio/flacVLC.flac", "flac")]
        [InlineData("./Data/Audio/mp3ID3Test1.mp3", "mp3")]
        [InlineData("./Data/Audio/mp3ID3Test2.mp3", "mp3")]
        [InlineData("./Data/Assemblies/ManagedExe.exe", "exe")]
        [InlineData("./Data/Assemblies/ManagedDLL.dll", "dll")]
        public async Task Search(string path, string ext)
        {
            var analyzer = new DictionaryBasedTrie(MimeTypes.Types);
            FileInfo file = new FileInfo(path);
            FileType type = null;

            using (ReadResult result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = analyzer.Search(in result);
            }

            Assert.NotNull(type);
            Assert.Contains(ext, type.Extension);
        }

        [Fact]
        public void InsertZeroOffsetFirstWildCard()
        {
            var analyzer = new DictionaryBasedTrie();
            FileType fileType = new FileType(new byte?[1], "ext", "app/ext", 0);
            analyzer.Insert(fileType);
            ReadResult readResult = new ReadResult(new byte[1], 1);
            var type = analyzer.Search(in readResult);
            Assert.NotNull(type);
            Assert.Same(fileType, type);
            Assert.Equal(0, type.HeaderOffset);
        }

        [Fact]
        public void InsertLastOffsetWildCard()
        {
            var analyzer = new DictionaryBasedTrie();
            FileType fileType = new FileType(new byte?[1], "ext", "app/ext", 559);
            analyzer.Insert(fileType);
            ReadResult readResult = new ReadResult(new byte[560], 560);
            var type = analyzer.Search(in readResult);
            Assert.NotNull(type);
            Assert.Same(fileType, type);
            Assert.Equal(559, type.HeaderOffset);
        }

        [Fact]
        public void InsertLastOffsetWildCardFull()
        {
            var analyzer = new DictionaryBasedTrie();
            FileType fileType = new FileType(new byte?[560], "ext", "app/ext", 559);
            analyzer.Insert(fileType);
            ReadResult readResult = new ReadResult(new byte[1120], 1120);
            var type = analyzer.Search(in readResult);
            Assert.NotNull(type);
            Assert.Same(fileType, type);
            Assert.Equal(559, type.HeaderOffset);
        }

        [Fact]
        public void IncrementalInsertSearchBoundries()
        {
            var analyzer = new DictionaryBasedTrie();

            for (int i = 0; i < 560; i++)
            {
                var bytes = new byte?[1];
                FileType fileType = new FileType(bytes, "ext" + i, "app/ext" + 1, (ushort)i);
                analyzer.Insert(fileType);

                var bytes1 = new byte[i + 1];
                ReadResult readResult = new ReadResult(bytes1, bytes1.Length);
                FileType type = analyzer.Search(in readResult);

                Assert.NotNull(type);
                Assert.Same(fileType, type);
                Assert.Equal(i, type.HeaderOffset);
            }
        }

        [Fact]
        public void InsertSearchBoundries()
        {
            var analyzer = new DictionaryBasedTrie();
            List<FileType> fileTypes = new List<FileType>();

            for (int i = 0; i < 560; i++)
            {
                var bytes = new byte?[1];
                FileType fileType = new FileType(bytes, "ext" + i, "app/ext" + 1, (ushort)i);
                analyzer.Insert(fileType);
                fileTypes.Add(fileType);
            }

            for (int i = 0; i < 560; i++)
            {
                var bytes = new byte[i + 1];
                ReadResult readResult = new ReadResult(bytes, bytes.Length);
                FileType type = analyzer.Search(in readResult);
                Assert.NotNull(type);
                Assert.Same(fileTypes[i], type);
                Assert.Equal(i, type.HeaderOffset);
            }
        }
    }
}
