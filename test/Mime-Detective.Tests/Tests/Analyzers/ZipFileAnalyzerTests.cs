using MimeDetective.Analyzers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Tests.Analyzers
{
    public class ZipFileAnalyzerTests
    {
        private readonly ZipFileAnalyzer analyzer;

        public ZipFileAnalyzerTests()
        {
            this.analyzer = new ZipFileAnalyzer();
        }

        [Fact]
        public void DefaultConstructor()
        {
            //assertion here just to have
            Assert.NotNull(this.analyzer);
        }

        public static IEnumerable<(string path, string extension, string mimeType)> DetectableFiles => new List<(string path, string extension, string mimeType)>
        {
            ("./Data/Documents/PptxPowerpoint2016.pptx", "pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"),
            ("./Data/Documents/StrictOpenXMLWord2016.docx", "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
            ("./Data/Documents/XlsxExcel2016.xlsx", "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
            ("./Data/Documents/XlsxExcel2016.xlsb", "xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"),
            ("./Data/Documents/XlsxExcel2016.xlsm", "xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"),
            ("./Data/Documents/DocxWord2016.docx", "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
            ("./Data/Documents/DocxWord2016.docm", "docm", "application/vnd.ms-word.document.macroEnabled.12"),
        };

        public static IEnumerable<(string path, string extension, string mimeType)> PlainZipFiles => new List<(string path, string extension, string mimeType)>
        {
            ("./Data/Zip/Images.zip", "zip", "application/x-compressed"),
            ("./Data/Zip/ImagesBy7zip.zip", "zip", "application/x-compressed"),
            ("./Data/Zip/EmptiedBy7zip.zip", "zip", "application/x-compressed"),
            ("./Data/Zip/emptyZip.zip", "zip", "application/x-compressed"),
        };

        public static IEnumerable<object[]> MatchingAllZippedFilesWithExtensionsAndMimeType => DetectableFiles.Union(PlainZipFiles).Select(x => new object[] { x.path, x.extension, x.mimeType }).ToList();

        public static IEnumerable<object[]> MatchingDetectableFilesWithExtensionsAndMimeType => DetectableFiles.Select(x => new object[] { x.path, x.extension, x.mimeType }).ToList();

        public static IEnumerable<object[]> MatchingAllFilesPathsOnly => DetectableFiles.Union(PlainZipFiles).Select(x => new object[] { x.path }).ToList();

        [Theory]
        [MemberData(nameof(MatchingAllZippedFilesWithExtensionsAndMimeType))]
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
        [MemberData(nameof(MatchingDetectableFilesWithExtensionsAndMimeType))]
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
            var file = new FileInfo("./Data/Zip/my-custom-format.custom");
            FileType type = null;

            using (var result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = this.analyzer.Search(in result, "hint", null);
            }

            Assert.NotNull(type);
            Assert.Equal("zip", type.Extension);
            Assert.Equal("hint", type.Mime);
        }

        [Fact]
        public async Task Search_Returns_Hinted_Extension_When_Unknown_Type()
        {
            var file = new FileInfo("./Data/Zip/my-custom-format.custom");
            FileType type = null;

            using (var result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = this.analyzer.Search(in result, null, "hint");
            }

            Assert.NotNull(type);
            Assert.Equal("hint", type.Extension);
            Assert.Equal("application/x-compressed", type.Mime);
        }

        [Fact]
        public async Task Search_Returns_Basic_Zip_Details_When_Unknown_Type()
        {
            var file = new FileInfo("./Data/Zip/my-custom-format.custom");
            FileType type = null;

            using (var result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = this.analyzer.Search(in result);
            }

            Assert.NotNull(type);
            Assert.Equal("zip", type.Extension);
            Assert.Equal("application/x-compressed", type.Mime);
        }

        [Theory]
        [MemberData(nameof(MatchingAllFilesPathsOnly))]
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
        [MemberData(nameof(MatchingAllFilesPathsOnly))]
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
