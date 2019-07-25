using MimeDetective.Analyzers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Tests.Analyzers
{
    public class MicrosoftCompoundDocumentFileAnalyzerTests
    {
        private readonly MicrosoftCompoundDocumentFileAnalyzer analyzer;

        public MicrosoftCompoundDocumentFileAnalyzerTests()
        {
            this.analyzer = new MicrosoftCompoundDocumentFileAnalyzer();
        }

        [Fact]
        public void DefaultConstructor()
        {
            //assertion here just to have
            Assert.NotNull(this.analyzer);
        }

        public static IEnumerable<(string path, string extension, string mimeType)> Files => new List<(string path, string extension, string mimeType)>
        {
            ("./Data/Documents/XlsExcel2007.xls", "xls", "application/excel"),
            ("./Data/Documents/XlsExcel2016.xls", "xls", "application/excel"),
            ("./Data/Documents/PptPowerpoint2016.ppt", "ppt", "application/mspowerpoint"),
            ("./Data/Documents/DocWord2016.doc", "doc", "application/msword"),
            ("./Data/Documents/test.msg", "msg", "application/vnd.ms-outlook"),
            ("./Data/Documents/OpenOfficeExcel.xls", "xls", "application/excel"),
            ("./Data/Documents/OpenOfficeExcel50.xls", "xls", "application/excel"),
            ("./Data/Documents/OpenOfficeExcel95.xls", "xls", "application/excel"),
            ("./Data/Documents/OpenOfficePpt.ppt", "ppt", "application/mspowerpoint"),
            ("./Data/Documents/OpenOfficeWordDoc.doc", "doc", "application/msword"),
        };

        public static IEnumerable<object[]> MatchingFilesWithExtensionsAndMimeType => Files.Select(x => new object[] { x.path, x.extension, x.mimeType }).ToList();

        public static IEnumerable<object[]> MatchingFilesPathsOnly => Files.Select(x => new object[] { x.path }).ToList();

        [Theory]
        [MemberData(nameof(MatchingFilesWithExtensionsAndMimeType))]
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
        [MemberData(nameof(MatchingFilesPathsOnly))]
        public async Task Analyzer_Does_Not_Dipose_Stream_When_ShouldDisposeStream_True(string path)
        {
            var analyzer = new MicrosoftCompoundDocumentFileAnalyzer();

            using (var s = File.OpenRead(path))
            {
                using (var testStream = new TestStream(s))
                {
                    using (var readResult = await ReadResult.ReadHeaderFromStreamAsync(testStream, shouldDisposeStream: true, shouldResetStreamPosition: true))
                    {
                        analyzer.Search(in readResult);

                        Assert.False(testStream.HasBeenDisposed);
                    }

                    Assert.True(testStream.HasBeenDisposed);
                }
            }
        }

        [Theory]
        [MemberData(nameof(MatchingFilesPathsOnly))]
        public async Task Analyzer_Does_Not_Dipose_Stream_When_ShouldDisposeStream_False(string path)
        {
            var analyzer = new MicrosoftCompoundDocumentFileAnalyzer();

            using (var s = File.OpenRead(path))
            {
                using (var testStream = new TestStream(s))
                {
                    using (var readResult = await ReadResult.ReadHeaderFromStreamAsync(testStream, shouldDisposeStream: false, shouldResetStreamPosition: true))
                    {
                        analyzer.Search(in readResult);

                        Assert.False(testStream.HasBeenDisposed);
                    }

                    Assert.False(testStream.HasBeenDisposed);
                }
            }
        }
    }
}
