using System.IO;
using System.Threading.Tasks;
using Xunit;
using static MimeDetective.Utilities.TypeComparisions;

namespace MimeDetective.Tests.Tests.Documents
{
    public class PdfFormats
    {
        public const string DocsPath = "./Data/Documents/";

        [Theory]
        [InlineData("MicrosoftPrintToPdf")]
        [InlineData("GithubTestPdf2")]
        [InlineData("PdfWord2016")]
        public async Task FileInfoPDF(string testPdf)
        {
            var info = GetFileInfo(DocsPath, testPdf, ".pdf");

            Assert.True(info.IsPdf());

            await AssertIsType(info, MimeTypes.PDF);
        }
    }
}