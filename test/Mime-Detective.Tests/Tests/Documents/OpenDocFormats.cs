using System.Threading.Tasks;
using Xunit;
using static MimeDetective.Utilities.TypeComparisions;

namespace MimeDetective.Tests.Documents
{
    public class OpenDocFormats
    {
        public const string DocsPath = "./Data/Documents/";

        [Theory]
        [InlineData("OpenDocWord2016")]
        [InlineData("OpenOfficeDoc")]
        public async Task IsOpenDoc(string filePath)
        {
            var info = GetFileInfo(DocsPath, filePath, ".odt");

            await AssertIsType(info, MimeTypes.ODT);
        }

        [Theory]
        [InlineData("OpenOfficePresentation")]
        public async Task IsOpenPresentation(string filePath)
        {
            var info = GetFileInfo(DocsPath, filePath, ".odp");

            await AssertIsType(info, MimeTypes.ODP);
        }

        [Theory]
        [InlineData("OpenOfficeSpreadsheet")]
        public async Task IsOpenSpreadSheet(string filePath)
        {
            var info = GetFileInfo(DocsPath, filePath, ".ods");

            await AssertIsType(info, MimeTypes.ODS);
        }
    }
}
