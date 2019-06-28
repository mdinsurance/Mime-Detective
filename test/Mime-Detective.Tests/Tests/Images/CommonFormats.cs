using MimeDetective.Utilities;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Tests.Images
{
    public class CommonFormats
    {
        public const string ImagePath = "./Data/Images/";

        [Fact]
        public async Task IsJpeg()
        {
            var info = new FileInfo(ImagePath + "test.jpg");

            Assert.True(info.IsJpeg());

            //false assertions
            Assert.False(info.IsGif());

            Assert.False(info.IsPng());

            await TypeComparisions.AssertIsType(info, MimeTypes.JPEG);
        }

        [Fact]
        public async Task IsBitmap()
        {
            var info = new FileInfo(ImagePath + "test.bmp");

            //false assertions
            Assert.False(info.IsJpeg());

            Assert.False(info.IsGif());

            Assert.False(info.IsPng());

            await TypeComparisions.AssertIsType(info, MimeTypes.BMP);
        }

        [Fact]
        public async Task IsPng()
        {
            var info = new FileInfo(ImagePath + "test.png");

            Assert.True(info.IsPng());

            //false assertions
            Assert.False(info.IsGif());

            Assert.False(info.IsJpeg());

            await TypeComparisions.AssertIsType(info, MimeTypes.PNG);
        }

        [Fact]
        public async Task IsGif()
        {
            var info = new FileInfo(ImagePath + "test.gif");

            Assert.True(info.IsGif());

            //false assertions
            Assert.False(info.IsPng());

            Assert.False(info.IsJpeg());

            await TypeComparisions.AssertIsType(info, MimeTypes.GIF);
        }

        [Fact]
        public async Task IsIco()
        {
            var info = new FileInfo(ImagePath + "test.ico");

            //false assertions
            Assert.False(info.IsPng());

            Assert.False(info.IsGif());

            Assert.False(info.IsJpeg());

            await TypeComparisions.AssertIsType(info, MimeTypes.ICO);
        }

        [Fact]
        public async Task IsWebp()
        {
            var info = new FileInfo(ImagePath + "test.webp");

            //false assertions
            Assert.False(info.IsPng());

            Assert.False(info.IsGif());

            Assert.False(info.IsJpeg());

            await TypeComparisions.AssertIsType(info, MimeTypes.WEBP);
        }
    }
}
