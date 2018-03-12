using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static MimeDetective.Utilities.TypeComparisions;

namespace MimeDetective.Tests.Audio
{
    public class CommonFormats
    {
        public const String AudioPath = "./Data/Audio";

        [Theory]
        [InlineData("mp3ID3Test1.mp3")]
        [InlineData("mp3ID3Test2.mp3")]
        public async Task IsMP3ID3(string fileName)
        {
            var info = GetFileInfo(AudioPath, fileName);

            await AssertIsType(info, MimeTypes.Mp3ID3);
        }

        [Theory]
        [InlineData("flacVLC.flac")]
        public async Task IsFLAC(string fileName)
        {
            var info = GetFileInfo(AudioPath, fileName);

            await AssertIsType(info, MimeTypes.Flac);
        }


        [Theory]
        [InlineData("oggVLC.ogg")]
        [InlineData("oggArchive.ogg")]
        public async Task IsOGG(string fileName)
        {
            var info = GetFileInfo(AudioPath, fileName);

            await AssertIsType(info, MimeTypes.OGG);
        }

        [Theory]
        [InlineData("mp4WinVoiceApp.m4a")]
        public async Task IsMP4A(string fileName)
        {
            var info = GetFileInfo(AudioPath, fileName);

            await AssertIsType(info, MimeTypes.Mp4QuickTime);
        }


        [Theory]
        [InlineData("wavVLC.wav")]
        public async Task IsWAV(string fileName)
        {
            var info = GetFileInfo(AudioPath, fileName);

            await AssertIsType(info, MimeTypes.Wav);
        }
    }
}
