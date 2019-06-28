using MimeDetective.Analyzers;
using System.Linq;
using Xunit;
using static MimeDetective.Tests.ReflectionHelpers;

namespace MimeDetective.Tests
{
    public class FileHeader
    {
        private static readonly FileType[][] typeArrays = new FileType[][] { MimeTypes.Types, MimeTypes.XmlTypes };

        [Fact]
        public void VerifyHeaderOffsetsDontExceedMaxHeaderSize()
        {
            var allValues = GetAllTypeValues();

            foreach (var value in allValues)
            {
                Assert.NotNull(value);

                var headerPlusOffset = (uint)value.Header.Length + value.HeaderOffset;

                Assert.True(headerPlusOffset <= MimeTypes.MaxHeaderSize);
            }
        }

        [Fact]
        public void VerifyHeaderDefsAreNotNull()
        {
            var allValues = GetAllTypeValues();

            foreach (var value in allValues)
            {
                Assert.NotNull(value);
                Assert.NotNull(value.Header);

                uint count = 0;
                for (var i = 0; i < value.Header.Length; i++)
                {
                    if (value.Header[i] is null)
                    {
                        count++;
                    }
                }

                Assert.False(count > 0 && count == value.Header.Length);
            }
        }

        [Fact]
        public void VerifyExtensionsAreNotNullOrEmpty()
        {
            var allValues = GetAllTypeValues();

            foreach (var value in allValues)
            {
                Assert.NotNull(value);
                Assert.NotNull(value.Extension);
                Assert.NotEmpty(value.Extension);
                Assert.False(string.IsNullOrWhiteSpace(value.Extension));
            }
        }

        [Fact]
        public void VerifyMimesAreNotNullOrEmpty()
        {
            var allValues = GetAllTypeValues();

            foreach (var value in allValues)
            {
                Assert.NotNull(value);
                Assert.NotNull(value.Mime);
                Assert.NotEmpty(value.Mime);
                Assert.False(string.IsNullOrWhiteSpace(value.Mime));
            }
        }

        [Fact]
        public void VerifyAllNonExcludedTypesArePresentInTypesArray()
        {
            var allTypeValues = GetAllTypeValues();
            var zipFileTypesFilteredOut = allTypeValues
                                        .Except(MsOfficeAnalyzer.MsDocTypes)
                                        .Except(MimeTypes.XmlTypes)
                                        .Except(PlainTextAnalyzer.PlainTextTypes)
                                        .Except(new[] { MimeTypes.UNKNOWN })
                                        .Distinct();

            Assert.NotNull(zipFileTypesFilteredOut);
            Assert.NotEmpty(zipFileTypesFilteredOut);

            foreach (var value in zipFileTypesFilteredOut)
            {
                Assert.Contains(value, MimeTypes.Types);
            }
        }

        [Fact]
        public void VerifyAllFileTypeFieldsArePublicStaticAndReadonly()
        {
            var allFields = GetAllTypeFields();

            foreach (var field in allFields)
            {
                Assert.True(field.IsPublic);
                Assert.True(field.IsStatic);
                Assert.True(field.IsInitOnly);
            }
        }

        [Fact]
        public void NoTwoHashCodesShouldEqualEachOther()
        {
            var allFields = GetAllTypeValues();

            foreach (var value in allFields)
            {
                foreach (var subValue in allFields)
                {
                    if (!object.ReferenceEquals(value, subValue))
                    {
                        Assert.NotEqual(value.GetHashCode(), subValue.GetHashCode());
                    }
                }
            }
        }

        [Fact]
        public void NoTwoFileTypesShouldEqualEachOther()
        {
            var allFields = GetAllTypeValues();

            foreach (var value in allFields)
            {
                foreach (var subValue in allFields)
                {
                    if (value.GetHashCode() != subValue.GetHashCode())
                    {
                        Assert.NotEqual(value, subValue);
                    }
                }
            }
        }

        //TBD Verify that no two file headers have the same matching data
        [Fact]
        public void VerifyNoDuplicateFileHeaderData()
        {
            var allFields = GetAllTypeValues();

            foreach (var value in allFields)
            {
                foreach (var subValue in allFields)
                {
                    if (value.GetHashCode() == subValue.GetHashCode())
                    {
                        return;
                    }

                    if (value.HeaderOffset != subValue.HeaderOffset
                        && value.Header.Length != subValue.Header.Length)
                    {
                        return;
                    }

                    var matchingCount = 0;

                    for (var i = 0; i < value.Header.Length; i++)
                    {
                        if (value.Header[i] == subValue.Header[i])
                        {
                            matchingCount++;
                        }
                    }

                    Assert.NotEqual(matchingCount, value.Header.Length);
                }
            }
        }
    }
}
