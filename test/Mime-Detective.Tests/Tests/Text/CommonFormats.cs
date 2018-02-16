using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MimeDetective;
using Xunit;
using System.IO;
using MimeDetective.Utilities;

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

            Assert.Equal(fileType.Extension, MimeTypes.TXT.Extension);
        }
    }
}
