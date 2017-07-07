using System;
using System.Collections.Generic;
using System.Diagnostics;

using Kirkin.Diagnostics;

namespace Kirkin.Media.FFmpeg
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

        /// <summary>
        /// Creates a new ffmpeg wrapper instance without specifying the exact ffmpeg.exe path.
        /// </summary>
        public FFmpegClient()
        {
        }

        /// <summary>
        /// Creates a new ffmpeg wrapper instance with the given ffmpeg.exe path.
        /// </summary>
        public FFmpegClient(string ffmpegPath)
        {
            FFmpegPath = ffmpegPath;
        }

        /// <summary>
        /// Converts the file at the given path using ffmpeg.exe.
        /// </summary>
        /// <param name="inputFilePath">Path to the input file.</param>
        /// <param name="outputFilePath">Path to the output file.</param>
        public void ConvertFile(string inputFilePath, string outputFilePath)
        {
            List<string> args = new List<string> {
                $@"-i ""{inputFilePath}""", // Input.
                "-c:v " + VideoEncoder,
                "-b:v " + VideoBitrate + "k",
                "-c:a " + AudioEncoder,
                "-ac " + AudioChannels,
                "-b:a " + AudioBitrate + "k",
                "-y", // Overwrite files without prompting.
                "-v warning", // Output verbosity level.
                $@"""{outputFilePath}""" // Output.
            };

            ProcessStartInfo info = new ProcessStartInfo(FFmpegPath ?? "ffmpeg", string.Join(" ", args)) {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (Process process = Process.Start(info))
            using (ProcessScope scope = new ProcessScope(process))
            {
                process.EnableRaisingEvents = true;

                bool nonEmptyOutputSeen = false;

                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data) || nonEmptyOutputSeen)
                    {
                        nonEmptyOutputSeen = true;

                        Console.WriteLine(e.Data);
                    }
                };

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
                    throw new FFmpegException(
                        $"FFMpeg exited with code {process.ExitCode}. Error:{Environment.NewLine + string.Join(Environment.NewLine, errors)}",
                        process.ExitCode
                    );
                }

                scope.Complete();
            }
        }
    }
}