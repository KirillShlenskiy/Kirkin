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
        /// <summary>
        /// ffmpeg.exe path specified when this instance was created.
        /// The default is null (use current directory/PATH).
        /// </summary>
        public string FFmpegPath { get; }

        /// <summary>
        /// Target audio bitrate in KB/sec. The default is 192.
        /// </summary>
        public int AudioBitrate { get; set; } = 192;

        /// <summary>
        /// Number of audio channels. The default is 2 (stereo).
        /// </summary>
        public int AudioChannels { get; set; } = 2;

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
            List<string> args = new List<string>();

            args.Add($@"-i ""{inputFilePath}"""); // Input.
            args.Add("-c:v " + VideoEncoder);
            args.Add("-b:v " + VideoBitrate + "k");
            args.Add("-c:a " + AudioEncoder);
            args.Add("-ac " + AudioChannels);
            args.Add("-b:a " + AudioBitrate + "k");
            args.Add("-y"); // Overwrite files without prompting.
            args.Add("-v warning"); // Output verbosity level.
            args.Add($@"""{outputFilePath}"""); // Output.

            ProcessStartInfo info = new ProcessStartInfo(FFmpegPath ?? "ffmpeg", string.Join(" ", args)) {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (Process process = Process.Start(info))
            using (ProcessScope scope = new ProcessScope(process))
            {
                process.EnableRaisingEvents = true;

                process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);

                List<string> errors = new List<string>();

                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errors.Add(e.Data);
                        Console.WriteLine(e.Data);
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
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