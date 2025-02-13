﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using MimeDetective.Analyzers;
using System.IO;

namespace MimeDetective.Benchmarks
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
            /*
            Add(Job.Default.With(Runtime.Core)
                .With(CsProjCoreToolchain.NetCoreApp11)
                .With(Platform.X64)
                .With(Jit.RyuJit)
                .WithId("NetCore1.1"));*/

            /*
            Add(Job.Default.With(Runtime.Core)
                .With(CsProjCoreToolchain.NetCoreApp20)
                .With(Platform.X64)
                .With(Jit.RyuJit)
                .WithId("NetCore2.0"));
            */

            this.Add(Job.Default.With(Runtime.Core)
                .With(CsProjCoreToolchain.NetCoreApp21)
                .With(Platform.X64)
                .With(Jit.RyuJit)
                .WithId("NetCore2.1"));
        }
    }

    [Config(typeof(MyConfig)), MemoryDiagnoser]
    public class Benchmarks
    {
        private static readonly byte[][] files = new byte[][]
        {
            ReadFile(new FileInfo("./data/Assemblies/ManagedDLL.dll")),
            ReadFile(new FileInfo("./data/Assemblies/ManagedExe.exe")),
            ReadFile(new FileInfo("./data/Images/test.png")),
            ReadFile(new FileInfo("./data/Images/test.jpg")),
            ReadFile(new FileInfo("./data/Images/test.gif")),
            ReadFile(new FileInfo("./data/Documents/DocWord2016.doc")),
            ReadFile(new FileInfo("./data/Zip/Images.zip")),
            ReadFile(new FileInfo("./data/Assemblies/NativeExe.exe")),
            ReadFile(new FileInfo("./data/Audio/wavVLC.wav")),
            ReadFile(new FileInfo("./data/Documents/PdfWord2016.pdf"))
        };
        private const int OpsPerInvoke = 10;
        private static readonly LinearCounting linear = new LinearCounting(MimeTypes.Types);
        private static readonly DictionaryTrie dict = new DictionaryTrie(MimeTypes.Types);
        private static readonly HybridTrie hybrid = new HybridTrie(MimeTypes.Types);
        private static readonly ArrayTrie array = new ArrayTrie(MimeTypes.Types);
        private static readonly LinearTrie linearTrie = new LinearTrie(MimeTypes.Types);

        private static byte[] ReadFile(FileInfo info)
        {
            var bytes = new byte[MimeTypes.MaxHeaderSize];
            using (var file = info.OpenRead())
            {
                file.Read(bytes, 0, MimeTypes.MaxHeaderSize);
            }
            return bytes;
        }

        [Benchmark]
        public LinearCounting LinearCountingInsertAll() => new LinearCounting(MimeTypes.Types);

        [Benchmark]
        public LinearTrie LinearTrieInsertAll() => new LinearTrie(MimeTypes.Types);

        [Benchmark]
        public DictionaryTrie DictTrieInsertAll() => new DictionaryTrie(MimeTypes.Types);

        [Benchmark]
        public ArrayTrie ArrayTrieInsertAll() => new ArrayTrie(MimeTypes.Types);

        [Benchmark]
        public HybridTrie HybridTrieInsertAll() => new HybridTrie(MimeTypes.Types);

        [Benchmark(OperationsPerInvoke = OpsPerInvoke)]
        public FileType LinearCountingSearch()
        {
            FileType result = null;
            foreach (var array in files)
            {
                using (var readResult = new ReadResult(array, MimeTypes.MaxHeaderSize))
                {
                    result = linear.Search(in readResult);
                }
            }
            return result;
        }

        [Benchmark(OperationsPerInvoke = OpsPerInvoke)]
        public FileType LinearTrieSearch()
        {
            FileType result = null;
            foreach (var array in files)
            {
                using (var readResult = new ReadResult(array, MimeTypes.MaxHeaderSize))
                {
                    result = linearTrie.Search(in readResult);
                }
            }
            return result;
        }

        [Benchmark(OperationsPerInvoke = OpsPerInvoke)]
        public FileType DictionaryTrieSearch()
        {
            FileType result = null;
            foreach (var array in files)
            {
                using (var readResult = new ReadResult(array, MimeTypes.MaxHeaderSize))
                {
                    result = dict.Search(in readResult);
                }
            }
            return result;
        }


        [Benchmark(OperationsPerInvoke = OpsPerInvoke)]
        public FileType HybridTrieSearch()
        {
            FileType result = null;
            foreach (var array in files)
            {
                using (var readResult = new ReadResult(array, MimeTypes.MaxHeaderSize))
                {
                    result = hybrid.Search(in readResult);
                }
            }
            return result;
        }


        [Benchmark(OperationsPerInvoke = OpsPerInvoke)]
        public FileType ArrayTrieSearch()
        {
            FileType result = null;
            foreach (var array in files)
            {
                using (var readResult = new ReadResult(array, MimeTypes.MaxHeaderSize))
                {
                    result = Benchmarks.array.Search(in readResult);
                }
            }
            return result;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}