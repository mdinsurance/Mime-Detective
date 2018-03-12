using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static MimeDetective.Utilities.TypeComparisions;

namespace MimeDetective.Tests.Documents
{
    public class CommonFormats
    {
        public const string DocsPath = "./Data/Documents/";

        [Theory]
        [InlineData("RichTextWord2016")]
        [InlineData("OpenOfficeRtf")]
        public async Task IsRTF(string filePath)
        {
            var info = GetFileInfo(DocsPath, filePath, ".rtf");

            Assert.True(info.IsRtf());

            await AssertIsType(info, MimeTypes.RTF);
        }
    }
}
