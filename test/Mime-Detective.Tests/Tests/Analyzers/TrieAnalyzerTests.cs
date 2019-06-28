using MimeDetective.Analyzers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace MimeDetective.Tests.Analyzers
{
    public static class TestHelpers
    {
        public static IFileAnalyzer DefaultCtor(this Type type) => Activator.CreateInstance(type) as IFileAnalyzer;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<FileType>, IFileAnalyzer>> ctorCache = new ConcurrentDictionary<Type, Func<IEnumerable<FileType>, IFileAnalyzer>>();

        public static IFileAnalyzer EnumerableCtor(this Type type, IEnumerable<FileType> fileTypes)
        {
            if (!ctorCache.TryGetValue(type, out var func))
            {
                var ctor = type.GetConstructors().Where(x => x.GetParameters().Any(y => y.ParameterType == typeof(IEnumerable<FileType>))).Single();
                var info = Expression.Parameter(typeof(IEnumerable<FileType>), "fileTypes");
                Expression callTheCtor = Expression.New(ctor, info);
                func = Expression.Lambda<Func<IEnumerable<FileType>, IFileAnalyzer>>(callTheCtor, new ParameterExpression[] { info }).Compile();
                ctorCache.TryAdd(type, func);
            }

            return func(fileTypes);
        }
    }

    public class TrieAnalyzerTests
    {
        [Theory]
        [InlineData(typeof(LinearTrie))]
        [InlineData(typeof(ArrayTrie))]
        [InlineData(typeof(HybridTrie))]
        [InlineData(typeof(DictionaryTrie))]
        [InlineData(typeof(LinearCounting))]
        public void DefaultConstructor(Type type)
        {
            var analyzer = type.DefaultCtor();

            //assertion here just to have
            Assert.NotNull(analyzer);

            analyzer.Insert(MimeTypes.ZIP);
        }

        [Theory]
        [InlineData(typeof(LinearTrie))]
        [InlineData(typeof(ArrayTrie))]
        [InlineData(typeof(HybridTrie))]
        [InlineData(typeof(DictionaryTrie))]
        [InlineData(typeof(LinearCounting))]
        public void EnumerableConstructor(Type type)
        {
            var analyzer = type.EnumerableCtor(MimeTypes.Types);

            //assertion here just to have
            Assert.NotNull(analyzer);
            Assert.Throws<ArgumentNullException>(() => type.EnumerableCtor(null));

            var emptyAnalyzer = type.EnumerableCtor(Enumerable.Empty<FileType>());
            Assert.NotNull(emptyAnalyzer);

            analyzer.Insert(MimeTypes.WORD);
        }

        [Theory]
        [InlineData(typeof(LinearTrie))]
        [InlineData(typeof(ArrayTrie))]
        [InlineData(typeof(HybridTrie))]
        [InlineData(typeof(DictionaryTrie))]
        [InlineData(typeof(LinearCounting))]
        public void EnumerableCtorDoesNotThrowIfSequenceContainsNull(Type type)
        {
            var types = new FileType[] { MimeTypes.ELF, null, MimeTypes.DLL_EXE };

            var analyzer = type.EnumerableCtor(types);

            Assert.NotNull(analyzer);
        }

        [Theory]
        [InlineData(typeof(LinearTrie))]
        [InlineData(typeof(ArrayTrie))]
        [InlineData(typeof(HybridTrie))]
        [InlineData(typeof(DictionaryTrie))]
        [InlineData(typeof(LinearCounting))]
        public void Insert(Type type)
        {
            var analyzer = type.DefaultCtor();
            Assert.Throws<ArgumentNullException>(() => analyzer.Insert(null));

            foreach (var fileType in MimeTypes.Types)
            {
                analyzer.Insert(fileType);
            }

            analyzer.Insert(MimeTypes.WORD);
        }

        [Theory]
        [InlineData("./Data/images/test.ico", "ico")]
        public async Task SearchLinear(string path, string ext)
        {
            var analyzer = new LinearTrie();
            analyzer.Insert(MimeTypes.ICO);

            var file = new FileInfo(path);
            FileType type = null;

            using (var result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = analyzer.Search(in result);
            }

            Assert.NotNull(type);
            Assert.Contains(ext, type.Extension);
        }

        [Theory]
        [InlineData(typeof(ArrayTrie), "./Data/Documents/XlsExcel2016.xls", "xls")]
        [InlineData(typeof(ArrayTrie), "./Data/Documents/PptPowerpoint2016.ppt", "ppt")]
        [InlineData(typeof(ArrayTrie), "./Data/Documents/DocWord2016.doc", "doc")]
        [InlineData(typeof(ArrayTrie), "./Data/Documents/PdfWord2016.pdf", "pdf")]
        [InlineData(typeof(ArrayTrie), "./Data/Zip/empty.zip", "zip")]
        [InlineData(typeof(ArrayTrie), "./Data/Zip/images.zip", "zip")]
        [InlineData(typeof(ArrayTrie), "./Data/Zip/imagesBy7zip.zip", "zip")]
        [InlineData(typeof(ArrayTrie), "./Data/images/test.gif", "gif")]
        [InlineData(typeof(ArrayTrie), "./Data/images/test.jpg", "jpg")]
        [InlineData(typeof(ArrayTrie), "./Data/images/test.ico", "ico")]
        [InlineData(typeof(ArrayTrie), "./Data/images/test.png", "png")]
        [InlineData(typeof(ArrayTrie), "./Data/images/test.bmp", "bmp")]
        [InlineData(typeof(ArrayTrie), "./Data/Audio/wavVLC.wav", "wav")]
        [InlineData(typeof(ArrayTrie), "./Data/Audio/flacVLC.flac", "flac")]
        [InlineData(typeof(ArrayTrie), "./Data/Audio/mp3ID3Test1.mp3", "mp3")]
        [InlineData(typeof(ArrayTrie), "./Data/Audio/mp3ID3Test2.mp3", "mp3")]
        [InlineData(typeof(ArrayTrie), "./Data/Assemblies/ManagedExe.exe", "exe")]
        [InlineData(typeof(ArrayTrie), "./Data/Assemblies/ManagedDLL.dll", "dll")]
        [InlineData(typeof(HybridTrie), "./Data/Documents/XlsExcel2016.xls", "xls")]
        [InlineData(typeof(HybridTrie), "./Data/Documents/PptPowerpoint2016.ppt", "ppt")]
        [InlineData(typeof(HybridTrie), "./Data/Documents/DocWord2016.doc", "doc")]
        [InlineData(typeof(HybridTrie), "./Data/Documents/PdfWord2016.pdf", "pdf")]
        [InlineData(typeof(HybridTrie), "./Data/Zip/empty.zip", "zip")]
        [InlineData(typeof(HybridTrie), "./Data/Zip/images.zip", "zip")]
        [InlineData(typeof(HybridTrie), "./Data/Zip/imagesBy7zip.zip", "zip")]
        [InlineData(typeof(HybridTrie), "./Data/images/test.gif", "gif")]
        [InlineData(typeof(HybridTrie), "./Data/images/test.jpg", "jpg")]
        [InlineData(typeof(HybridTrie), "./Data/images/test.ico", "ico")]
        [InlineData(typeof(HybridTrie), "./Data/images/test.png", "png")]
        [InlineData(typeof(HybridTrie), "./Data/images/test.bmp", "bmp")]
        [InlineData(typeof(HybridTrie), "./Data/Audio/wavVLC.wav", "wav")]
        [InlineData(typeof(HybridTrie), "./Data/Audio/flacVLC.flac", "flac")]
        [InlineData(typeof(HybridTrie), "./Data/Audio/mp3ID3Test1.mp3", "mp3")]
        [InlineData(typeof(HybridTrie), "./Data/Audio/mp3ID3Test2.mp3", "mp3")]
        [InlineData(typeof(HybridTrie), "./Data/Assemblies/ManagedExe.exe", "exe")]
        [InlineData(typeof(HybridTrie), "./Data/Assemblies/ManagedDLL.dll", "dll")]
        [InlineData(typeof(DictionaryTrie), "./Data/Documents/XlsExcel2016.xls", "xls")]
        [InlineData(typeof(DictionaryTrie), "./Data/Documents/PptPowerpoint2016.ppt", "ppt")]
        [InlineData(typeof(DictionaryTrie), "./Data/Documents/DocWord2016.doc", "doc")]
        [InlineData(typeof(DictionaryTrie), "./Data/Documents/PdfWord2016.pdf", "pdf")]
        [InlineData(typeof(DictionaryTrie), "./Data/Zip/empty.zip", "zip")]
        [InlineData(typeof(DictionaryTrie), "./Data/Zip/images.zip", "zip")]
        [InlineData(typeof(DictionaryTrie), "./Data/Zip/imagesBy7zip.zip", "zip")]
        [InlineData(typeof(DictionaryTrie), "./Data/images/test.gif", "gif")]
        [InlineData(typeof(DictionaryTrie), "./Data/images/test.jpg", "jpg")]
        [InlineData(typeof(DictionaryTrie), "./Data/images/test.ico", "ico")]
        [InlineData(typeof(DictionaryTrie), "./Data/images/test.png", "png")]
        [InlineData(typeof(DictionaryTrie), "./Data/images/test.bmp", "bmp")]
        [InlineData(typeof(DictionaryTrie), "./Data/Audio/wavVLC.wav", "wav")]
        [InlineData(typeof(DictionaryTrie), "./Data/Audio/flacVLC.flac", "flac")]
        [InlineData(typeof(DictionaryTrie), "./Data/Audio/mp3ID3Test1.mp3", "mp3")]
        [InlineData(typeof(DictionaryTrie), "./Data/Audio/mp3ID3Test2.mp3", "mp3")]
        [InlineData(typeof(DictionaryTrie), "./Data/Assemblies/ManagedExe.exe", "exe")]
        [InlineData(typeof(DictionaryTrie), "./Data/Assemblies/ManagedDLL.dll", "dll")]
        [InlineData(typeof(LinearCounting), "./Data/Documents/XlsExcel2016.xls", "xls")]
        [InlineData(typeof(LinearCounting), "./Data/Documents/PptPowerpoint2016.ppt", "ppt")]
        [InlineData(typeof(LinearCounting), "./Data/Documents/DocWord2016.doc", "doc")]
        [InlineData(typeof(LinearCounting), "./Data/Documents/PdfWord2016.pdf", "pdf")]
        [InlineData(typeof(LinearCounting), "./Data/Zip/empty.zip", "zip")]
        [InlineData(typeof(LinearCounting), "./Data/Zip/images.zip", "zip")]
        [InlineData(typeof(LinearCounting), "./Data/Zip/imagesBy7zip.zip", "zip")]
        [InlineData(typeof(LinearCounting), "./Data/images/test.gif", "gif")]
        [InlineData(typeof(LinearCounting), "./Data/images/test.jpg", "jpg")]
        [InlineData(typeof(LinearCounting), "./Data/images/test.ico", "ico")]
        [InlineData(typeof(LinearCounting), "./Data/images/test.png", "png")]
        [InlineData(typeof(LinearCounting), "./Data/images/test.bmp", "bmp")]
        [InlineData(typeof(LinearCounting), "./Data/Audio/wavVLC.wav", "wav")]
        [InlineData(typeof(LinearCounting), "./Data/Audio/flacVLC.flac", "flac")]
        [InlineData(typeof(LinearCounting), "./Data/Audio/mp3ID3Test1.mp3", "mp3")]
        [InlineData(typeof(LinearCounting), "./Data/Audio/mp3ID3Test2.mp3", "mp3")]
        [InlineData(typeof(LinearCounting), "./Data/Assemblies/ManagedExe.exe", "exe")]
        [InlineData(typeof(LinearCounting), "./Data/Assemblies/ManagedDLL.dll", "dll")]
        [InlineData(typeof(LinearTrie), "./Data/Documents/XlsExcel2016.xls", "xls")]
        [InlineData(typeof(LinearTrie), "./Data/Documents/PptPowerpoint2016.ppt", "ppt")]
        [InlineData(typeof(LinearTrie), "./Data/Documents/DocWord2016.doc", "doc")]
        [InlineData(typeof(LinearTrie), "./Data/Documents/PdfWord2016.pdf", "pdf")]
        [InlineData(typeof(LinearTrie), "./Data/Zip/empty.zip", "zip")]
        [InlineData(typeof(LinearTrie), "./Data/Zip/images.zip", "zip")]
        [InlineData(typeof(LinearTrie), "./Data/Zip/imagesBy7zip.zip", "zip")]
        [InlineData(typeof(LinearTrie), "./Data/images/test.gif", "gif")]
        [InlineData(typeof(LinearTrie), "./Data/images/test.jpg", "jpg")]
        [InlineData(typeof(LinearTrie), "./Data/images/test.ico", "ico")]
        [InlineData(typeof(LinearTrie), "./Data/images/test.png", "png")]
        [InlineData(typeof(LinearTrie), "./Data/images/test.bmp", "bmp")]
        [InlineData(typeof(LinearTrie), "./Data/Audio/wavVLC.wav", "wav")]
        [InlineData(typeof(LinearTrie), "./Data/Audio/flacVLC.flac", "flac")]
        [InlineData(typeof(LinearTrie), "./Data/Audio/mp3ID3Test1.mp3", "mp3")]
        [InlineData(typeof(LinearTrie), "./Data/Audio/mp3ID3Test2.mp3", "mp3")]
        [InlineData(typeof(LinearTrie), "./Data/Assemblies/ManagedExe.exe", "exe")]
        [InlineData(typeof(LinearTrie), "./Data/Assemblies/ManagedDLL.dll", "dll")]
        public async Task Search(Type analyzerType, string path, string ext)
        {
            var analyzer = analyzerType.EnumerableCtor(MimeTypes.Types);
            var expectedTypes = MimeTypes.Types.Where(x => x.Extension.Contains(ext));
            var file = new FileInfo(path);
            FileType type = null;

            using (var result = await ReadResult.ReadFileHeaderAsync(file))
            {
                type = analyzer.Search(in result);
            }

            Assert.NotNull(type);
            Assert.Contains(ext, type.Extension);
            Assert.Contains(type, expectedTypes);
        }

        [Theory]
        [InlineData(typeof(LinearTrie))]
        [InlineData(typeof(ArrayTrie))]
        [InlineData(typeof(HybridTrie))]
        [InlineData(typeof(DictionaryTrie))]
        [InlineData(typeof(LinearCounting))]
        public void InsertZeroOffsetFirstWildCard(Type type)
        {
            var analyzer = type.DefaultCtor();
            var fileType = new FileType(new byte?[1], "ext", "app/ext", 0);
            analyzer.Insert(fileType);
            var readResult = new ReadResult(new byte[1], 1);
            var result = analyzer.Search(in readResult);
            Assert.NotNull(result);
            Assert.Same(fileType, result);
            Assert.Equal(0, result.HeaderOffset);
        }

        [Theory]
        [InlineData(typeof(LinearTrie))]
        [InlineData(typeof(ArrayTrie))]
        [InlineData(typeof(HybridTrie))]
        [InlineData(typeof(DictionaryTrie))]
        [InlineData(typeof(LinearCounting))]
        public void InsertLastOffsetWildCard(Type type)
        {
            var analyzer = type.DefaultCtor();
            var fileType = new FileType(new byte?[1], "ext", "app/ext", 559);
            analyzer.Insert(fileType);
            var readResult = new ReadResult(new byte[560], 560);
            var result = analyzer.Search(in readResult);
            Assert.NotNull(result);
            Assert.Same(fileType, result);
            Assert.Equal(559, result.HeaderOffset);
        }

        [Theory]
        [InlineData(typeof(LinearTrie))]
        [InlineData(typeof(ArrayTrie))]
        [InlineData(typeof(HybridTrie))]
        [InlineData(typeof(DictionaryTrie))]
        [InlineData(typeof(LinearCounting))]
        public void InsertLastOffsetWildCardFull(Type type)
        {
            var analyzer = type.DefaultCtor();
            var fileType = new FileType(new byte?[560], "ext", "app/ext", 559);
            analyzer.Insert(fileType);
            var readResult = new ReadResult(new byte[1120], 1120);
            var result = analyzer.Search(in readResult);
            Assert.NotNull(result);
            Assert.Same(fileType, result);
            Assert.Equal(559, result.HeaderOffset);
        }

        [Theory]
        [InlineData(typeof(LinearTrie))]
        [InlineData(typeof(ArrayTrie))]
        [InlineData(typeof(HybridTrie))]
        [InlineData(typeof(DictionaryTrie))]
        [InlineData(typeof(LinearCounting))]
        public void IncrementalInsertSearchBoundries(Type type)
        {
            var analyzer = type.DefaultCtor();

            for (var i = 0; i < 560; i++)
            {
                var bytes = new byte?[1];
                var fileType = new FileType(bytes, "ext" + i, "app/ext" + 1, (ushort)i);
                analyzer.Insert(fileType);

                var bytes1 = new byte[i + 1];
                var readResult = new ReadResult(bytes1, bytes1.Length);
                var result = analyzer.Search(in readResult);

                Assert.NotNull(result);
                Assert.Same(fileType, result);
                Assert.Equal(i, result.HeaderOffset);
            }
        }

        [Theory]
        [InlineData(typeof(LinearTrie))]
        [InlineData(typeof(ArrayTrie))]
        [InlineData(typeof(HybridTrie))]
        [InlineData(typeof(DictionaryTrie))]
        [InlineData(typeof(LinearCounting))]
        public void InsertSearchBoundries(Type type)
        {
            var analyzer = type.DefaultCtor();
            var fileTypes = new List<FileType>();

            for (var i = 0; i < 560; i++)
            {
                var bytes = new byte?[1];
                var fileType = new FileType(bytes, "ext" + i, "app/ext" + 1, (ushort)i);
                analyzer.Insert(fileType);
                fileTypes.Add(fileType);
            }

            for (var i = 0; i < 560; i++)
            {
                var bytes = new byte[i + 1];
                var readResult = new ReadResult(bytes, bytes.Length);
                var result = analyzer.Search(in readResult);
                Assert.NotNull(result);
                Assert.Same(fileTypes[i], result);
                Assert.Equal(i, result.HeaderOffset);
            }
        }

        [Theory]
        [InlineData(typeof(LinearTrie))]
        [InlineData(typeof(ArrayTrie))]
        [InlineData(typeof(HybridTrie))]
        [InlineData(typeof(DictionaryTrie))]
        [InlineData(typeof(LinearCounting))]
        public void InsertReverseSequentialScenario(Type type)
        {
            var trie = type.DefaultCtor();

            var type1 = new FileType(new byte?[] { 1, 0, 1 }, "ext1", "app/ext1");
            var type1Bytes = new byte[] { 1, 0, 1 };
            var type2 = new FileType(new byte?[] { 1, 0, 1, 0, 1 }, "ext2", "app/ext2");
            var type2Bytes = new byte[] { 1, 0, 1, 0, 1 };
            var type3 = new FileType(new byte?[] { 1, 0, 1, 0, 1, 0, 1 }, "ext3", "app/ext3");
            var type3Bytes = new byte[] { 1, 0, 1, 0, 1, 0, 1 };

            trie.Insert(type3);
            trie.Insert(type2);
            trie.Insert(type1);

            //lookup type 1
            var type1Result = trie.Search(new ReadResult(type1Bytes, type1Bytes.Length));
            Assert.NotNull(type1Result);
            Assert.Same(type1, type1Result);

            //lookup type 2
            var type2Result = trie.Search(new ReadResult(type2Bytes, type2Bytes.Length));
            Assert.NotNull(type2Result);
            Assert.Same(type2, type2Result);

            //lookup type 3
            var type3Result = trie.Search(new ReadResult(type3Bytes, type3Bytes.Length));
            Assert.NotNull(type3Result);
            Assert.Same(type3, type3Result);
        }

        [Theory]
        [InlineData(typeof(LinearTrie))]
        [InlineData(typeof(ArrayTrie))]
        [InlineData(typeof(HybridTrie))]
        [InlineData(typeof(DictionaryTrie))]
        [InlineData(typeof(LinearCounting))]
        public void InsertSequentialScenario(Type type)
        {
            var trie = type.DefaultCtor();

            var type1 = new FileType(new byte?[] { 1, 0, 1 }, "ext1", "app/ext1");
            var type1Bytes = new byte[] { 1, 0, 1 };
            var type2 = new FileType(new byte?[] { 1, 0, 1, 0, 1 }, "ext2", "app/ext2");
            var type2Bytes = new byte[] { 1, 0, 1, 0, 1 };
            var type3 = new FileType(new byte?[] { 1, 0, 1, 0, 1, 0, 1 }, "ext3", "app/ext3");
            var type3Bytes = new byte[] { 1, 0, 1, 0, 1, 0, 1 };

            trie.Insert(type1);
            trie.Insert(type2);
            trie.Insert(type3);

            //lookup type 1
            var type1Result = trie.Search(new ReadResult(type1Bytes, type1Bytes.Length));
            Assert.NotNull(type1Result);
            Assert.Same(type1, type1Result);

            //lookup type 2
            var type2Result = trie.Search(new ReadResult(type2Bytes, type2Bytes.Length));
            Assert.NotNull(type2Result);
            Assert.Same(type2, type2Result);

            //lookup type 3
            var type3Result = trie.Search(new ReadResult(type3Bytes, type3Bytes.Length));
            Assert.NotNull(type3Result);
            Assert.Same(type3, type3Result);
        }
    }
}
