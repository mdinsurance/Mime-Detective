using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.IO;
using MimeDetective;
using static MimeDetective.Utilities.TypeComparisions;

namespace MimeDetective.Tests.Documents
{
    public class MsOfficeFormats
    {
        public const string DocsPath = "./Data/Documents/";

        [Theory]
        [InlineData("DocWord2016")]
        [InlineData("DocWord97")]
        public async Task IsDoc(string filePath)
        {
            var info = GetFileInfo(DocsPath, filePath, ".doc");

            Assert.True(info.IsWord());

            await AssertIsType(info, MimeTypes.WORD);
        }

        [Theory]
        [InlineData("DocxWord2016")]
        [InlineData("StrictOpenXMLWord2016")]
        public async Task IsDocx(string filePath)
        {
            var info = GetFileInfo(DocsPath, filePath, ".docx");

            Assert.True(info.IsWord());

            await AssertIsType(info, MimeTypes.WORDX);
        }

        [Theory]
        [InlineData("RichTextWord2016")]
        public async Task IsRTF(string filePath)
        {
            var info = GetFileInfo(DocsPath, filePath, ".rtf");

            Assert.True(info.IsRtf());

            await AssertIsType(info, MimeTypes.RTF);
        }

        [Theory]
        [InlineData("OpenDocWord2016")]
        public async Task IsOpenDoc(string filePath)
        {
            var info = GetFileInfo(DocsPath, filePath, ".odt");

            await AssertIsType(info, MimeTypes.ODT);
        }

        [Theory]
        [InlineData("PptPowerpoint2016")]
        public async Task IsPowerPoint(string filePath)
        {
            var info = GetFileInfo(DocsPath, filePath, ".ppt");

            Assert.True(info.IsPowerPoint());

            await AssertIsType(info, MimeTypes.PPT);
        }

        [Theory]
        [InlineData("PptxPowerpoint2016")]
        public async Task IsPowerPointX(string filePath)
        {
            var info = GetFileInfo(DocsPath, filePath, ".pptx");

            Assert.True(info.IsPowerPoint());

            await AssertIsType(info, MimeTypes.PPTX);
        }

        [Theory]
        [InlineData("XlsExcel2016")]
        [InlineData("XlsExcel2007")]
        public void IsExcel(string filePath)
        {
            var info = GetFileInfo(DocsPath, filePath, ".xls");

            Assert.True(info.IsExcel());
        }

        [Theory]
        [InlineData("XlsExcel2016")]
        public async Task IsExcel2(string filePath)
        {
            var info = GetFileInfo(DocsPath, filePath, ".xls");

            Assert.True(info.IsExcel());

            await AssertIsType(info, MimeTypes.EXCEL);
        }

        [Theory]
        [InlineData("XlsxExcel2016")]
        public async Task IsExcelX(string filePath)
        {
            var info = GetFileInfo(DocsPath, filePath, ".xlsx");

            Assert.True(info.IsExcel());

            await AssertIsType(info, MimeTypes.EXCELX);
        }
    }
}
