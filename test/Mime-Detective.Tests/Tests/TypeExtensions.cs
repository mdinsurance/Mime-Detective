using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MimeDetective.Extensions;
using MimeDetective;
using Xunit;

namespace MimeDetective.Tests
{
	public class TypeExtensions
	{
		const string GoodFile = "./data/Images/test.jpg";

		const string GoodXmlFile = "./data/Documents/DocxWord2016.docx";

		const string GoodZipFile = "./data/Zip/images.zip";

		const string BadFile = "./data/Images/empty.jpg";

		const string NonexistentFile = "./data/nonexistent.jpg";

		const string smallTxtFile = "./data/Text/SuperSmall.txt";

		//small ascii text files
		const string oneByteFile = "./data/Text/oneCharFile.txt";
		const string twoByteFile = "./data/Text/twoCharFile.txt";
		const string threeByteFile = "./data/Text/threeCharFile.txt";


		//load from fileinfo
		//attempt to load from real file
		//attempt to load from nonexistent file /badfile
		[Fact]
		public async Task FromFile()
		{
			var fileInfo = new FileInfo(GoodFile);

			var fileType = await fileInfo.GetFileTypeAsync();

			Assert.True(fileType == MimeTypes.JPEG);
		}

		[Fact]
		public void FromFileSync()
		{
			var fileInfo = new FileInfo(GoodFile);

			Assert.True(fileInfo.IsJpeg());
		}

		//test shouldn't fail, an empty file can be valid input
		[Fact]
		public async Task FromEmptyFile()
		{
			var fileInfo = new FileInfo(BadFile);

			var type = await fileInfo.GetFileTypeAsync();

			//no match so return type is null
			Assert.Null(type);
		}

		[Fact]
		public async Task FromNonExistentFile()
		{
			var fileInfo = new FileInfo(NonexistentFile);

			await Assert.ThrowsAnyAsync<Exception>(() => fileInfo.GetFileTypeAsync());
		}

		//load from stream
			//attempt to load from good stream
			//attempt to load from empty stream
			//stream shouldn't be closed

		[Fact]
		public async Task FromStream()
		{
			var fileInfo = new FileInfo(GoodFile);

			using (var fileStream = fileInfo.OpenRead())
			{
				var fileType = await fileStream.GetFileTypeAsync();

				Assert.NotNull(fileType);

				Assert.Equal(MimeTypes.JPEG, fileType);
			}
		}

		[Fact]
		public void FromStreamSync()
		{
			var fileInfo = new FileInfo(GoodFile);

			using (var fileStream = fileInfo.OpenRead())
			{
				var fileType = fileStream.GetFileType();

				Assert.NotNull(fileType);

				Assert.Equal(MimeTypes.JPEG, fileType);
			}
		}

		[Fact]
		public async Task FromEmptyStream()
		{
			var emptyStream = System.IO.Stream.Null;

			var nullReturn = await emptyStream.GetFileTypeAsync();

			Assert.Null(nullReturn);
		}

		[Theory]
		[InlineData(GoodFile, "jpg")]
		[InlineData(GoodXmlFile, "docx")]
		[InlineData(GoodZipFile, "zip")]
		public async Task StreamShouldStillBeOpen(string path, string ext)
		{
			var fileInfo = new FileInfo(path);

			var fileStream = fileInfo.OpenRead();
			
			var fileType = await fileStream.GetFileTypeAsync();

			Assert.NotNull(fileType);

			Assert.Equal(ext, fileType.Extension);

			Assert.True(fileStream.CanRead);

			Assert.True(fileStream.CanSeek);

			Assert.False(fileStream.CanWrite);

			fileStream.Dispose();

			Assert.False(fileStream.CanRead);

			Assert.False(fileStream.CanSeek);

			Assert.False(fileStream.CanWrite);
		}

		[Theory]
		[InlineData(GoodFile, "jpg")]
		[InlineData(GoodXmlFile, "docx")]
		[InlineData(GoodZipFile, "zip")]
		public async Task StreamShouldBeDisposed(string path, string ext)
		{
			var fileInfo = new FileInfo(path);

			var fileStream = fileInfo.OpenRead();

			var fileType = await fileStream.GetFileTypeAsync(shouldDisposeStream: true);

			Assert.NotNull(fileType);

			Assert.Equal(ext, fileType.Extension);

			Assert.NotNull(fileStream);

			Assert.False(fileStream.CanRead);

			Assert.False(fileStream.CanSeek);

			Assert.False(fileStream.CanWrite);
		}

		[Theory]
		[InlineData(GoodFile, "jpg")]
		[InlineData(GoodXmlFile, "docx")]
		[InlineData(GoodZipFile, "zip")]
		public void StreamShouldBeDisposedSync(string path, string ext)
		{
			var fileInfo = new FileInfo(path);

			var fileStream = fileInfo.OpenRead();

			var fileType = fileStream.GetFileType(shouldDisposeStream: true);

			Assert.NotNull(fileType);

			Assert.Equal(ext, fileType.Extension);

			Assert.NotNull(fileStream);

			Assert.False(fileStream.CanRead);

			Assert.False(fileStream.CanSeek);

			Assert.False(fileStream.CanWrite);
		}

		//load from byte array
		//load from good byte array
		//attempt to load from empty byte array
		[Fact]
		public async Task FromByteArray()
		{
			var fileInfo = new FileInfo(GoodFile);

			//560 is the max file header size
			byte[] byteArray = new byte[560];

			using (var fileStream = fileInfo.OpenRead())
			{
				await fileStream.ReadAsync(byteArray, 0, 560);
			}

			var mimeType = byteArray.GetFileType();

			Assert.NotNull(mimeType);

			Assert.Equal(MimeTypes.JPEG, mimeType);
		}

		[Fact]
		public void FromEmptyByteArray()
		{
			var zerodByteArray = new byte[560];

			var zerodResult = zerodByteArray.GetFileType();

			Assert.Null(zerodResult);

			var emptyBtyeArray = new byte[0];

			Assert.Null(emptyBtyeArray.GetFileType());
		}

		[Theory]
		[InlineData(new byte[] { 0 })]
		[InlineData(new byte[] { 0, 0 })]
		[InlineData(new byte[] { 0, 0, 0, 0 })]
		[InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 1 })]
		[InlineData(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 1 })]
		public void NoMatchingFileDef(byte[] array)
		{
			var almostZerodResult = array.GetFileType();

			Assert.Null(almostZerodResult);
		}

		[Theory]
		[InlineData(smallTxtFile)]
		[InlineData(oneByteFile, Skip = "Planned in text detector for 1.0.0")]
		[InlineData(twoByteFile, Skip = "Planned in text detector for 1.0.0")]
		[InlineData(threeByteFile, Skip = "Planned in text detector for 1.0.0")]
		public async Task FromSmallTxtFile(string file)
		{
			FileInfo smallTxt = new FileInfo(file);

			using (var stream = smallTxt.OpenRead())
			{
				byte[] bytes = new byte[smallTxt.Length];

				await stream.ReadAsync(bytes, 0, bytes.Length);

				stream.Seek(0, SeekOrigin.Begin);

				var result = await smallTxt.GetFileTypeAsync();

				Assert.Equal(MimeTypes.TXT_UTF8, result);

				Assert.Equal(MimeTypes.TXT_UTF8, await stream.GetFileTypeAsync());

				Assert.Equal(MimeTypes.TXT_UTF8, bytes.GetFileType());
			}
		}
	}
}
