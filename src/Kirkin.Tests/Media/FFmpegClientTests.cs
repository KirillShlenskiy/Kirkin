using Kirkin.Media;

using NUnit.Framework;

namespace Kirkin.Tests.Media
{
    public class FFmpegClientTests
    {
        [Test]
        public void TestConvert()
        {
            string inputFilePath = @"";
            string outputFilePath = @"";

            FFmpegClient ffmpeg = new FFmpegClient();

            ffmpeg.ConvertFile(inputFilePath, outputFilePath);
        }
    }
}