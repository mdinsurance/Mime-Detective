using MimeDetective.Analyzers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Tests.Analyzers
{
    public class PlainTextAnalyzerTests
    {
        private readonly PlainTextAnalyzer analyzer;

        public PlainTextAnalyzerTests()
        {
            this.analyzer = new PlainTextAnalyzer();
        }

        [Fact]
        public void DefaultConstructor()
        {
            Assert.NotNull(this.analyzer);
        }

        public static IEnumerable<(string path, string extension, string mimeType)> DetectableFiles => new List<(string path, string extension, string mimeType)>
        {
            ("./Data/Text/test.csv", "csv", "text/csv"),
            ("./Data/Text/htmlFile.html", "html", "text/html"),
            ("./Data/Text/htmlFileNoDocType.html", "html", "text/html"),
            ("./Data/Text/html4FileWithDocType.html", "html", "text/html"),
            ("./Data/Text/test.eml", "eml", "message/rfc822"),
            ("./Data/Text/test.xml", "xml", "text/xml"),
        };

        public static IEnumerable<(string path, string extension, string mimeType)> NonDetectableFiles => new List<(string path, string extension, string mimeType)>
        {
            ("./Data/Text/test.txt", "txt", "text/plain"),
            ("./Data/Text/TextFile1.txt", "txt", "text/plain"),
            ("./Data/Text/threeCharFile.txt", "txt", "text/plain"),
            ("./Data/Text/twoCharFile.txt", "txt", "text/plain"),
            ("./Data/Text/htmlFragment.html", "txt", "text/plain"),
        };

        public static IEnumerable<object[]> DetectableFilesWithExtensionsAndMimeType => DetectableFiles.Select(x => new object[] { x.path, x.extension, x.mimeType }).ToList();

        public static IEnumerable<object[]> AllFilesWithExtensionsAndMimeType => DetectableFiles.Union(NonDetectableFiles).Select(x => new object[] { x.path, x.extension, x.mimeType }).ToList();

        public static IEnumerable<object[]> AllFilesPathsOnly => DetectableFiles.Union(NonDetectableFiles).Select(x => new object[] { x.path }).ToList();

        [Theory]
        [MemberData(nameof(AllFilesWithExtensionsAndMimeType))]
        public async Task Search_Returns_Correct_Extension_And_MimeType(string path, string expectedExtension, string expectedMimeType)
        {
            var file = new FileInfo(path);
            FileType type = null;

            using (var result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = this.analyzer.Search(in result);
            }

            Assert.NotNull(type);
            Assert.Equal(expectedExtension, type.Extension);
            Assert.Equal(expectedMimeType, type.Mime);
        }

        [Theory]
        [MemberData(nameof(DetectableFilesWithExtensionsAndMimeType))]
        public async Task Search_Returns_FileType_And_Ignores_Hints_For_Detectable_FilesTypes(string path, string expectedExtension, string expectedMimeType)
        {
            var file = new FileInfo(path);
            FileType type = null;

            using (var result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = this.analyzer.Search(in result, "ignored", "ignored");
            }

            Assert.NotNull(type);
            Assert.Equal(expectedExtension, type.Extension);
            Assert.Equal(expectedMimeType, type.Mime);
        }

        [Fact]
        public async Task Search_Returns_Hinted_MimeType_When_Unknown_Type()
        {
            var file = new FileInfo("./Data/Text/my-custom-text-format.cust");
            FileType type = null;

            using (var result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = this.analyzer.Search(in result, "hint", null);
            }

            Assert.NotNull(type);
            Assert.Equal("txt", type.Extension);
            Assert.Equal("hint", type.Mime);
        }

        [Fact]
        public async Task Search_Returns_Hinted_Extension_When_Unknown_Type()
        {
            var analyzer = new ZipFileAnalyzer();
            var file = new FileInfo("./Data/Text/my-custom-text-format.cust");
            FileType type = null;

            using (var result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = this.analyzer.Search(in result, null, "hint");
            }

            Assert.NotNull(type);
            Assert.Equal("hint", type.Extension);
            Assert.Equal("text/plain", type.Mime);
        }

        [Fact]
        public async Task Search_Returns_Basic_Text_Details_When_Unknown_Type()
        {
            var analyzer = new ZipFileAnalyzer();
            var file = new FileInfo("./Data/Text/my-custom-text-format.cust");
            FileType type = null;

            using (var result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = this.analyzer.Search(in result);
            }

            Assert.NotNull(type);
            Assert.Equal("txt", type.Extension);
            Assert.Equal("text/plain", type.Mime);
        }

        [Theory]
        [MemberData(nameof(AllFilesPathsOnly))]
        public async Task Analyzer_Does_Not_Dipose_Stream_When_ShouldDisposeStream_True(string path)
        {
            using (var s = File.OpenRead(path))
            {
                using (var testStream = new TestStream(s))
                {
                    using (var readResult = await ReadResult.ReadHeaderFromStreamAsync(testStream, shouldDisposeStream: true, shouldResetStreamPosition: true))
                    {
                        this.analyzer.Search(in readResult);

                        Assert.False(testStream.HasBeenDisposed);
                    }

                    Assert.True(testStream.HasBeenDisposed);
                }
            }
        }

        [Theory]
        [MemberData(nameof(AllFilesPathsOnly))]
        public async Task Analyzer_Does_Not_Dipose_Stream_When_ShouldDisposeStream_False(string path)
        {
            using (var s = File.OpenRead(path))
            {
                using (var testStream = new TestStream(s))
                {
                    using (var readResult = await ReadResult.ReadHeaderFromStreamAsync(testStream, shouldDisposeStream: false, shouldResetStreamPosition: true))
                    {
                        this.analyzer.Search(in readResult);

                        Assert.False(testStream.HasBeenDisposed);
                    }

                    Assert.False(testStream.HasBeenDisposed);
                }
            }
        }
    }
}
