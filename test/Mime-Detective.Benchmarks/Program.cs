using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;
using System;
using System.IO;
using System.Security.Cryptography;
using MimeDetective.Extensions;
using MimeDetective;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Toolchains.CsProj;
using System.Linq;
using System.Runtime.CompilerServices;
using MimeDetective.Analyzers;

namespace Mime_Detective.Benchmarks
{
    public class MyConfig : ManualConfig
    {
        public MyConfig()
        {
            /*
            Add(Job.Default.With(Runtime.Clr)
                .With(CsProjClassicNetToolchain.Net47)
                .With(Jit.RyuJit)
                .With(Platform.X64)
                .WithId("Net47"));*/

            Add(Job.Default.With(Runtime.Core)
                .With(CsProjCoreToolchain.NetCoreApp11)
                .With(Platform.X64)
                .With(Jit.RyuJit)
                .WithId("NetCore1.1"));

            Add(Job.Default.With(Runtime.Core)
                .With(CsProjCoreToolchain.NetCoreApp20)
                .With(Platform.X64)
                .With(Jit.RyuJit)
                .WithId("NetCore2.0"));

            Add(Job.Default.With(Runtime.Core)
                .With(CsProjCoreToolchain.NetCoreApp21)
                .With(Platform.X64)
                .With(Jit.RyuJit)
                .WithId("NetCore2.1"));
        }
    }

    [Config(typeof(MyConfig)), MemoryDiagnoser]
    public class TypeLookup
    {
        static readonly byte[][] files = new byte[][]
        {
            ReadFile(new FileInfo("./data/Images/test.jpg")),
            ReadFile(new FileInfo("./data/Images/test.gif")),
            ReadFile(new FileInfo("./data/Documents/DocWord2016.doc")),
            ReadFile(new FileInfo("./data/Zip/Images.zip")),
            ReadFile(new FileInfo("./data/Assemblies/NativeExe.exe")),
            ReadFile(new FileInfo("./data/Audio/wavVLC.wav"))
        };

        const int OpsPerInvoke = 6;
        static readonly LinearCountingAnalyzer linear = new LinearCountingAnalyzer(MimeTypes.Types);
        static readonly DictionaryBasedTrie trie2 = new DictionaryBasedTrie(MimeTypes.Types);
        static readonly DictionaryBasedTrie2 trie3 = new DictionaryBasedTrie2(MimeTypes.Types);
        static readonly ArrayBasedTrie trie5 = new ArrayBasedTrie(MimeTypes.Types);
        static readonly ArrayBasedTrie2 trie6 = new ArrayBasedTrie2(MimeTypes.Types);

        static byte[] ReadFile(FileInfo info)
        {
            byte[] bytes = new byte[MimeTypes.MaxHeaderSize];
            using (FileStream file = info.OpenRead())
            {
                file.Read(bytes, 0, MimeTypes.MaxHeaderSize);
            }
            return bytes;
        }

        //[Benchmark]
        public LinearCountingAnalyzer LinearCountingAnalyzerInsertAll()
        {
            return new LinearCountingAnalyzer(MimeTypes.Types);
        }

        //[Benchmark]
        public DictionaryBasedTrie DictTrieInsertAll()
        {
            return new DictionaryBasedTrie(MimeTypes.Types);
        }

        //[Benchmark]
        public DictionaryBasedTrie2 DictTrie2InsertAll()
        {
            return new DictionaryBasedTrie2(MimeTypes.Types);
        }

        //[Benchmark]
        public ArrayBasedTrie ArrayTrieInsertAll()
        {
            return new ArrayBasedTrie(MimeTypes.Types);
        }

        //[Benchmark]
        public ArrayBasedTrie2 ArrayTrieInsertAll2()
        {
            return new ArrayBasedTrie2(MimeTypes.Types);
        }

        [Benchmark(OperationsPerInvoke = OpsPerInvoke)]
        public FileType LinearCountingAnalyzerSearch()
        {
            FileType result = null;
            foreach (var array in files)
            {
                using (ReadResult readResult = new ReadResult(array, MimeTypes.MaxHeaderSize))
                {
                    result = linear.Search(in readResult);
                }
            }
            return result;
        }

        [Benchmark(OperationsPerInvoke = OpsPerInvoke)]
        public FileType DictionaryBasedTrie()
        {
            FileType result = null;
            foreach (var array in files)
            {
                using (ReadResult readResult = new ReadResult(array, MimeTypes.MaxHeaderSize))
                {
                    result = trie2.Search(in readResult);
                }
            }
            return result;
        }


        [Benchmark(OperationsPerInvoke = OpsPerInvoke)]
        public FileType DictionaryBasedTrie2()
        {
            FileType result = null;
            foreach (var array in files)
            {
                using (ReadResult readResult = new ReadResult(array, MimeTypes.MaxHeaderSize))
                {
                    result = trie3.Search(in readResult);
                }
            }
            return result;
        }


        [Benchmark(OperationsPerInvoke = OpsPerInvoke)]
        public FileType ArrayBasedTrie()
        {
            FileType result = null;
            foreach (var array in files)
            {
                using (ReadResult readResult = new ReadResult(array, MimeTypes.MaxHeaderSize))
                {
                    result = trie5.Search(in readResult);
                }
            }
            return result;
        }

        [Benchmark(OperationsPerInvoke = OpsPerInvoke)]
        public FileType ArrayBasedTrie2()
        {
            FileType result = null;
            foreach (var array in files)
            {
                using (ReadResult readResult = new ReadResult(array, MimeTypes.MaxHeaderSize))
                {
                    result = trie6.Search(in readResult);
                }
            }
            return result;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<TypeLookup>(); 
        }
    }
}