using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using Kirkin.Diagnostics;

namespace Kirkin.Media
{
    public sealed class FFmpegClient
    {
        public string FFmpegPath { get; }

        /// <summary>
        /// Target video bitrate in KB/sec. The default is 2048.
        /// </summary>
        public int VideoBitrate { get; set; } = 2048;

        public FFmpegClient()
        {
        }

        public FFmpegClient(string ffmpegPath)
        {
            FFmpegPath = ffmpegPath;
        }

        public void ConvertFile(string inputFilePath, string outputFilePath)
        {
            string args = $@"-i ""{inputFilePath}"" -c:v libx264 -b:v {VideoBitrate}k -c:a copy -y -v warning ""{outputFilePath}""";

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