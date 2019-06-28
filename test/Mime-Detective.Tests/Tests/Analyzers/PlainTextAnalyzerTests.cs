using MimeDetective.Analyzers;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Tests.Analyzers
{
    public class PlainTextAnalyzerTests
    {
        [Fact]
        public void DefaultConstructor()
        {
            var analyzer = new PlainTextAnalyzer();

            //assertion here just to have
            Assert.NotNull(analyzer);
        }


        [Theory]
        [InlineData("./Data/Text/test.csv", null, null, "csv", "text/csv")]
        [InlineData("./Data/Text/test.csv", "ignored", "ignored", "csv", "text/csv")]
        [InlineData("./Data/Text/test.txt", null, null, "txt", "text/plain")]
        [InlineData("./Data/Text/test.txt", "mnint", null, "txt", "mnint")]
        [InlineData("./Data/Text/test.txt", null, "exthint", "exthint", "text/plain")]
        [InlineData("./Data/Text/test.txt", "mnint", "exthint", "exthint", "mnint")]
        [InlineData("./Data/Text/my-custom-text-format.cust", null, null, "txt", "text/plain")]
        [InlineData("./Data/Text/my-custom-text-format.cust", "mnint", null, "txt", "mnint")]
        [InlineData("./Data/Text/my-custom-text-format.cust", null, "exthint", "exthint", "text/plain")]
        [InlineData("./Data/Text/my-custom-text-format.cust", "mnint", "exthint", "exthint", "mnint")]
        public async Task Search(string path, string mimeHint, string extensionHint, string expectedExtension, string expectedMime)
        {
            var analyzer = new PlainTextAnalyzer();
            var file = new FileInfo(path);
            FileType type = null;

            using (var result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = analyzer.Search(in result, mimeHint, extensionHint);
            }

            Assert.NotNull(type);
            Assert.Equal(expectedExtension, type.Extension);
            Assert.Equal(expectedMime, type.Mime);
        }
    }
}
