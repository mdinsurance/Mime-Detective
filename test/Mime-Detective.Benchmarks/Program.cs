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

namespace Mime_Detective.Benchmarks
{
	public class MyConfig : ManualConfig
	{
		public MyConfig()
		{
			Add(Job.Default.With(Runtime.Clr)
				.With(CsProjClassicNetToolchain.Net47)
				.With(Jit.RyuJit)
				.With(Platform.X64)
				.WithId("Net47"));

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
		}
	}

	[Config(typeof(MyConfig)), MemoryDiagnoser]
	public class TypeLookup
	{
		const string goodFile = "./data/images/test.jpg";

		const string goodXmlFile = "./data/Documents/test.docx";

		const string goodZipFile = "./data/images.zip";

		const string badFile = "./data/empty.jpg";

		static byte[] bytes;

		static FileInfo GoodFile;//, GoodXmlFile, GoodZipFile, BadFile;
        static readonly Trie trie = new Trie(MimeTypes.Types);

		[GlobalSetup]
		public void Setup()
		{
			GoodFile = new FileInfo(goodFile);
			//GoodXmlFile = new FileInfo(goodXmlFile);
			//GoodZipFile = new FileInfo(goodZipFile);
			//BadFile = new FileInfo(badFile);


			//FileType type = MimeTypes.Types[0];
			bytes = new byte[1000];
			using (FileStream file = GoodFile.OpenRead())
			{
				file.Read(bytes,0,1000);
			}

			GC.Collect();
		}

		//[Benchmark(OperationsPerInvoke = 1000)]
		//public void GoodLookup() => DoNTimes(GoodFile);

		//[Benchmark(OperationsPerInvoke = 1000)]
		//public void BinaryLookup() => DoNTimesBinary(GoodFile);

		//[Benchmark(OperationsPerInvoke = 1000)]
		//public void BadLookup() => DoNTimes(BadFile);


		//[Benchmark(OperationsPerInvoke = 1000)]
		//public void DocxLookup() => DoNTimes(GoodXmlFile);

		//[Benchmark(OperationsPerInvoke = 1000)]
		//public void ZipLookup() => DoNTimes(GoodZipFile);

		//[Benchmark(OperationsPerInvoke = 1000)]

		[Benchmark]
		public FileType ByteArrayLookupBinary()
		{
			return bytes.GetFileType();
		}

        [Benchmark]
        public FileType TrieLookUp()
        {
            ReadResult result = new ReadResult(bytes, 1000);
            return trie.Search(in result);
        }

		/*
		//[Benchmark(OperationsPerInvoke = 1000)]
		public void IsTextForLoopIter()
		{
			for (int i = 0; i < 1000; i++)
			{
				var type = IsTxtForLoop();
			}
		}


		public FileType IsTxtForLoop()
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] != 0)
					return null;
				else if (i == bytes.Length - 1)
					return MimeTypes.TXT;
			}

			return null;
		}

		//[Benchmark(OperationsPerInvoke = 1000)]
		public void IsTextForeachLoopIter()
		{
			for (int i = 0; i < 1000; i++)
			{
				var type = IsTxtForeachLoop();
			}
		}

		//[Benchmark(OperationsPerInvoke = 1000)]
		public void IsTextWhileForeachLoopIter()
		{
			for (int i = 0; i < 1000; i++)
			{
				var type = IsTxtWhileForeachLoop();
			}
		}

		public FileType IsTxtWhileForeachLoop()
		{
			var temp = bytes.AsEnumerable().GetEnumerator();

			while(temp.MoveNext())
			{
				if (temp.Current != 0)
					return null;
			}

			return MimeTypes.TXT;
		}

		public FileType IsTxtForeachLoop()
		{
			foreach(byte bYte in bytes)
			{
				if (bYte != 0)
					return null;
			}

			return MimeTypes.TXT;
		}

		//[Benchmark(OperationsPerInvoke = 1000)]
		public void IsTextAnyLinqIter()
		{
			for (int i = 0; i < 1000; i++)
			{
				var type = IsTextAnyLinq();
			}
		}


		public FileType IsTextAnyLinq()
		{
			if (!bytes.Any(b => b == 0))
				return MimeTypes.TXT;
			else
				return null;
		}
		*/
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<TypeLookup>(); 
		}
	}
}