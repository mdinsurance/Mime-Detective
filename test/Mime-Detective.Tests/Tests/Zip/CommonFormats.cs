using System.Threading.Tasks;
using Xunit;
using static MimeDetective.Utilities.TypeComparisions;

namespace MimeDetective.Tests.Zip
{
    public class CommonFormats
    {
        public const string dataPath = "./Data/Zip";

        [Theory]
        [InlineData("Empty")]
        [InlineData("Images")]
        [InlineData("ImagesBy7zip")]
        public async Task IsZip(string file)
        {
            var fileInfo = GetFileInfo(dataPath, file, ".zip");

            Assert.True(fileInfo.IsZip());

            await AssertIsType(fileInfo, MimeTypes.ZIP);
        }

        [Theory]
        [InlineData("emptyZip")]
        [InlineData("EmptiedBy7zip")]
        public async Task IsEmptyZip(string file)
        {
            var fileInfo = GetFileInfo(dataPath, file, ".zip");

            Assert.True(fileInfo.IsZip());

            await AssertIsType(fileInfo, MimeTypes.ZIP_EMPTY);
        }

        [Fact]
        public async Task Is7zip()
        {
            var fileInfo = GetFileInfo(dataPath, "Images", ".7z");

            await AssertIsType(fileInfo, MimeTypes.ZIP_7z);
        }

        [Fact]
        public async Task IsRar()
        {
            var fileInfo = GetFileInfo(dataPath, "TestBlender", ".rar");

            await AssertIsType(fileInfo, MimeTypes.RAR);
        }

        [Fact]
        public async Task HintsAreUsedWhenNoSecondaryMatch()
        {
            var fileInfo = GetFileInfo(dataPath, "my-custom-format", ".custom");

            var mimeHint = "mime-hint";
            var extensionHint = "extensionHint";
            var fileType = await fileInfo.GetFileTypeAsync(mimeHint, extensionHint);

            Assert.Equal(MimeTypes.ZIP.Header, fileType.Header);
            Assert.Equal(MimeTypes.ZIP.HeaderOffset, fileType.HeaderOffset);
            Assert.Equal(mimeHint, fileType.Mime);
            Assert.Equal(extensionHint, fileType.Extension);
        }

        /*
        [Fact]
        public async Task IsTar()
        {
            var fileInfo = GetFileInfo(dataPath, "Images7zip", ".tar");

            await AssertIsType(fileInfo, MimeTypes.TAR_ZV);
        }
        */
    }
}
