using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using Kirkin.Diagnostics;

namespace Kirkin.Media
{
    /// <summary>
    /// Managed FFmpeg wrapper.
    /// </summary>
    public sealed class FFmpegClient
    {
        public string FFmpegPath { get; }

        /// <summary>
        /// Target audio bitrate in KB/sec. The default is 192.
        /// </summary>
        public int AudioBitrate { get; set; } = 192;

        /// <summary>
        /// Audio encoder. The default is "aac".
        /// </summary>
        public string AudioEncoder { get; set; } = "aac";

        /// <summary>
        /// Target video bitrate in KB/sec. The default is 2048.
        /// </summary>
        public int VideoBitrate { get; set; } = 2048;

        /// <summary>
        /// Video encoder. The default is "libx264".
        /// </summary>
        public string VideoEncoder { get; set; } = "libx264";

        public FFmpegClient()
        {
        }

        public FFmpegClient(string ffmpegPath)
        {
            FFmpegPath = ffmpegPath;
        }

        public void ConvertFile(string inputFilePath, string outputFilePath)
        {
            string args = $@"-i ""{inputFilePath}"" -c:v {VideoEncoder} -b:v {VideoBitrate}k -c:a {AudioEncoder} -b:a {AudioBitrate}k -y -v warning ""{outputFilePath}""";

            ProcessStartInfo info = new ProcessStartInfo(FFmpegPath ?? "ffmpeg", args) {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (Process process = Process.Start(info))
            using (ProcessScope scope = new ProcessScope(process))
            {
                process.EnableRaisingEvents = true;

                List<string> errors = new List<string>();

                process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);

                process.ErrorDataReceived += (s, e) =>
                {
                    errors.Add(e.Data);

                    Console.WriteLine(e.Data);
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Win32Exception(
                        process.ExitCode,
                        $"FFMpeg exited with code {process.ExitCode}. Error:{Environment.NewLine + string.Join(Environment.NewLine, errors)}"
                    );
                }

                scope.Complete();
            }
        }
    }
}