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
            string outputFilePath = @"C:\temp\video\target.mp4";

            FFmpegClient ffmpeg = new FFmpegClient();

            ffmpeg.VideoEncoder = VideoEncoder.Libx264Fast;
            ffmpeg.VideoHeight = 480;
            ffmpeg.VideoBitrate = 750;
            ffmpeg.AudioEncoder = AudioEncoder.Copy;

            ffmpeg.ConvertFile(inputFilePath, outputFilePath);
        }
    }
}