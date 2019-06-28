﻿using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Tests.Extensions
{
    public class FileInfoExtensionTests
    {
        private const string GoodFile = "./data/Images/test.jpg";
        private const string GoodXmlFile = "./data/Documents/DocxWord2016.docx";
        private const string GoodZipFile = "./data/Zip/images.zip";
        private const string BadFile = "./data/Images/empty.jpg";
        private const string NonexistentFile = "./data/nonexistent.jpg";
        private const string smallTxtFile = "./data/Text/SuperSmall.txt";

        //small ascii text files
        private const string oneByteFile = "./data/Text/oneCharFile.txt";
        private const string twoByteFile = "./data/Text/twoCharFile.txt";
        private const string threeByteFile = "./data/Text/threeCharFile.txt";


        //load from fileinfo
        //attempt to load from real file
        //attempt to load from nonexistent file /badfile
        [Fact]
        public async Task FromFileAsync()
        {
            var fileInfo = new FileInfo(GoodFile);

            var fileType = await fileInfo.GetFileTypeAsync();

            Assert.True(fileType == MimeTypes.JPEG);
        }

        [Fact]
        public void FromFile()
        {
            var fileInfo = new FileInfo(GoodFile);

            Assert.True(fileInfo.IsJpeg());
        }

        //test shouldn't fail, an empty file can be valid input
        [Fact]
        public async Task FromEmptyFile()
        {
            var fileInfo = new FileInfo(BadFile);

            var type = await fileInfo.GetFileTypeAsync();

            //no match so return type is null
            Assert.Null(type);
        }

        [Fact]
        public async Task FromNonExistentFile()
        {
            var fileInfo = new FileInfo(NonexistentFile);

            await Assert.ThrowsAnyAsync<Exception>(() => fileInfo.GetFileTypeAsync());
        }

        [Theory]
        [InlineData("./data/Zip/images.zip")]
        [InlineData("./data/Zip/Empty.zip")]
        public void IsZipFile(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            Assert.True(fileInfo.IsZip());
        }

        [Theory]
        [InlineData("./data/Zip/WinRar.rar")]
        public void IsRarFile(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            Assert.True(fileInfo.IsRar());
        }
    }
}
