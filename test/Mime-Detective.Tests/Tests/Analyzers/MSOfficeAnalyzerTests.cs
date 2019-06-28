using MimeDetective.Analyzers;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Tests.Analyzers
{
    public class MSOfficeAnalyzerTests
    {
        [Fact]
        public void DefaultConstructor()
        {
            var analyzer = new MsOfficeAnalyzer();

            //assertion here just to have
            Assert.NotNull(analyzer);
        }


        [Theory]
        [InlineData("./Data/Documents/XlsExcel2016.xls", "xls")]
        [InlineData("./Data/Documents/PptPowerpoint2016.ppt", "ppt")]
        [InlineData("./Data/Documents/DocWord2016.doc", "doc")]
        [InlineData("./Data/Documents/test.msg", "msg")]
        public async Task Search(string path, string ext)
        {
            var analyzer = new MsOfficeAnalyzer();
            var file = new FileInfo(path);
            FileType type = null;

            using (var result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = analyzer.Search(in result);
            }

            Assert.NotNull(type);
            Assert.Contains(ext, type.Extension);
        }
    }
}
