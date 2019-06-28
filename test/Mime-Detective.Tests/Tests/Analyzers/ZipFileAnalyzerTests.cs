using MimeDetective.Analyzers;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Tests.Analyzers
{
    public class ZipFileAnalyzerTests
    {
        [Fact]
        public void DefaultConstructor()
        {
            var analyzer = new ZipFileAnalyzer();

            //assertion here just to have
            Assert.NotNull(analyzer);
        }


        [Theory]
        [InlineData("./Data/Documents/PptxPowerpoint2016.pptx", null, null, "pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
        [InlineData("./Data/Documents/PptxPowerpoint2016.pptx", "ignored", "ignored", "pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
        [InlineData("./Data/Documents/StrictOpenXMLWord2016.docx", null, null, "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
        [InlineData("./Data/Documents/StrictOpenXMLWord2016.docx", "ignored", "ignored", "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
        [InlineData("./Data/Documents/XlsxExcel2016.xlsx", null, null, "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("./Data/Documents/XlsxExcel2016.xlsb", null, null, "xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12")]
        [InlineData("./Data/Documents/XlsxExcel2016.xlsm", null, null, "xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12")]
        [InlineData("./Data/Documents/DocxWord2016.docx", null, null, "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
        [InlineData("./Data/Documents/DocxWord2016.docm", null, null, "docm", "application/vnd.ms-word.document.macroEnabled.12")]
        [InlineData("./Data/Zip/Images.zip", null, null, "zip", "application/x-compressed")]
        [InlineData("./Data/Zip/ImagesBy7zip.zip", null, null, "zip", "application/x-compressed")]
        [InlineData("./Data/Zip/EmptiedBy7zip.zip", null, null, "zip", "application/x-compressed")]
        [InlineData("./Data/Zip/emptyZip.zip", null, null, "zip", "application/x-compressed")]
        [InlineData("./Data/Zip/my-custom-format.custom", null, null, "zip", "application/x-compressed")]
        [InlineData("./Data/Zip/my-custom-format.custom", "mnint", null, "zip", "mnint")]
        [InlineData("./Data/Zip/my-custom-format.custom", null, "exthint", "exthint", "application/x-compressed")]
        [InlineData("./Data/Zip/my-custom-format.custom", "mnint", "exthint", "exthint", "mnint")]
        public async Task Search(string path, string mimeHint, string extensionHint, string expectedExtension, string expectedMime)
        {
            var analyzer = new ZipFileAnalyzer();
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
