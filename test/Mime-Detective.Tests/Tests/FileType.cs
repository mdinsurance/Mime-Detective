using System;
using Xunit;

namespace MimeDetective.Tests
{
    public class FileTypeTests
    {
        [Fact]
        public void Constructors()
        {
            var info = new global::MimeDetective.FileType(new byte?[] { 0x12, 0x14, 0x13, 0x15, 0x16 }, "png", "image/png", 4);

            Assert.Throws<ArgumentNullException>(() => { var a = new global::MimeDetective.FileType(null, "png", "image/png", 4); });
        }

        [Fact]
        public void Equal()
        {
            Assert.True(MimeTypes.ELF.Equals(MimeTypes.ELF));

            Assert.False(MimeTypes.ELF.Equals(MimeTypes.DLL_EXE));
        }

        [Fact]
        public void NullEquals()
        {
            var elf = MimeTypes.ELF;

            Assert.False(elf == null);

            Assert.False(null == elf);

            Assert.True(elf != null);

            Assert.True(null != elf);

            Assert.False(elf.Equals(null));
        }

        [Fact]
        public void EqualsOperatorOverloads()
        {
            var elf = MimeTypes.ELF;

            Assert.True(elf == MimeTypes.ELF);

            Assert.False(elf == MimeTypes.DLL_EXE);

            Assert.False(elf != MimeTypes.ELF);

            Assert.True(elf != MimeTypes.DLL_EXE);
        }

        [Fact]
        public void ToStringReturns()
        {
            var elf = MimeTypes.ELF;

            Assert.Equal(elf.ToString(), MimeTypes.ELF.Extension);

            Assert.NotEqual(elf.ToString(), MimeTypes.ELF.GetType().ToString());
        }

        [Fact]
        public void GetHashCodeIsntRandomAndDoesntChange()
        {
            var allValues = ReflectionHelpers.GetAllTypeValues();

            foreach (var value in allValues)
            {
                for (var i = 0; i < 5; i++)
                {
                    var hashCode = value.GetHashCode();

                    var hashCode2 = value.GetHashCode();

                    Assert.Equal(hashCode, hashCode2);
                }
            }
        }
    }
}
