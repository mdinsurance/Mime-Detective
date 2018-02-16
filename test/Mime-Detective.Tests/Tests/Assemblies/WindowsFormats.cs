using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static MimeDetective.Utilities.TypeComparisions;

namespace MimeDetective.Tests.Assemblies
{
    public class WindowsFormats
    {
        public const String AssembliesPath = "./Data/Assemblies";

        [Theory]
        [InlineData("ManagedExe.exe")]
        [InlineData("ManagedDLL.dll")]
        [InlineData("MixedDLL.dll")]
        [InlineData("MixedExe.exe")]
        [InlineData("NativeExe.exe")]
        [InlineData("NativeDLL.dll")]
        public async Task IsExeOrDLL(string fileName)
        {
            var info = GetFileInfo(AssembliesPath, fileName);

            Assert.True(info.IsExe());

            await AssertIsType(info, MimeTypes.DLL_EXE);
        }
    }
}
