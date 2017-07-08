using Kirkin.Media.FFmpeg;

using NUnit.Framework;

namespace Kirkin.Tests.Media
{
    public class FFmpegClientTests
    {
        [Test]
        public void TestConvert()
        {
            string inputFilePath = @"C:\temp\video\source.mp4";
            string outputFilePath = @"C:\temp\video\target.m4a";

            FFmpegClient ffmpeg = new FFmpegClient();

            ffmpeg.VideoEncoder = VideoEncoder.NoVideo;
            ffmpeg.AudioEncoder = AudioEncoder.Copy;

            ffmpeg.ConvertFile(inputFilePath, outputFilePath);
        }
    }
}