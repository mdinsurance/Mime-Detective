﻿using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Tests.Text
{
    public class TextTests
    {
        public const string TextPath = "./Data/Text/";

        public const string TextFile = "test.txt";

        [Fact]
        public async Task IsTxt()
        {
            var info = new FileInfo(TextPath + TextFile);

            var fileType = await info.GetFileTypeAsync();

            Assert.Equal(MimeTypes.TXT.Extension, fileType.Extension);
            Assert.Equal(MimeTypes.TXT.Mime, fileType.Mime);
        }

        [Fact]
        public async Task IsHtml()
        {
            var info = new FileInfo(TextPath + "htmlFile.html");

            var fileType = await info.GetFileTypeAsync();

            Assert.Equal(MimeTypes.HTML.Extension, fileType.Extension);
            Assert.Equal(MimeTypes.HTML.Mime, fileType.Mime);
        }

        [Fact]
        public async Task IsEml()
        {
            var info = new FileInfo(TextPath + "test.eml");

            var fileType = await info.GetFileTypeAsync();

            Assert.Equal(MimeTypes.EML.Extension, fileType.Extension);
            Assert.Equal(MimeTypes.EML.Mime, fileType.Mime);
        }

        [Fact]
        public async Task IsCsv()
        {
            var info = new FileInfo(TextPath + "test.csv");

            var fileType = await info.GetFileTypeAsync();

            Assert.Equal(MimeTypes.CSV.Extension, fileType.Extension);
            Assert.Equal(MimeTypes.CSV.Mime, fileType.Mime);
        }

        [Fact]
        public async Task IsXml_UTF8_WithBOM()
        {
            // this XML file is encoded with: UTF-8
            // this XML does NOT include a Byte Order Mark (EF BB BF) to signal the encoding
            var info = new FileInfo(TextPath + "MindMap.NoBOM.smmx");

            var fileType = await info.GetFileTypeAsync();

            Assert.Equal(MimeTypes.XML.Extension, fileType.Extension);
            Assert.Equal(MimeTypes.XML.Mime, fileType.Mime);
        }

        [Fact]
        public async Task IsXml_UTF8_WithoutBOM()
        {
            // this XML file is encoded with: UTF-8
            // this XML INCLUDES a Byte Order Mark (EF BB BF) to signal the encoding
            var info = new FileInfo(TextPath + "MindMap.WithBOM.smmx");

            var fileType = await info.GetFileTypeAsync();

            Assert.Equal(MimeTypes.XML.Extension, fileType.Extension);
            Assert.Equal(MimeTypes.XML.Mime, fileType.Mime);
        }

        [Fact]
        public async Task IsXml_UCS2LE_WithBOM()
        {
            // this XML file is encoded with: UCS-2 Little Endian (UTF16)
            // this XML INCLUDES a Byte Order Mark (FEFF) to signal the encoding
            var info = new FileInfo(TextPath + "MindMap.UCS2LE.WithBOM.smmx");

            var fileType = await info.GetFileTypeAsync();

            Assert.Equal(MimeTypes.XML.Extension, fileType.Extension);
            Assert.Equal(MimeTypes.XML.Mime, fileType.Mime);
        }

        [Fact]
        public async Task IsXml_UCS2BE_WithBOM()
        {
            // this XML file is encoded with: UCS-2 Little Endian (UTF16)
            // this XML INCLUDES a Byte Order Mark (FEFF) to signal the encoding
            var info = new FileInfo(TextPath + "MindMap.UCS2BE.WithBOM.smmx");

            var fileType = await info.GetFileTypeAsync();

            Assert.Equal(MimeTypes.XML.Extension, fileType.Extension);
            Assert.Equal(MimeTypes.XML.Mime, fileType.Mime);
        }
    }
}
