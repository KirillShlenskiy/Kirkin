using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        /// Target audio bitrate in Kb/sec. The default is 192.
        /// </summary>
        public int AudioBitrate { get; set; } = 192;

        /// <summary>
        /// Number of audio channels. The default is zero (original).
        /// </summary>
        public int AudioChannels { get; set; } = 0;

        /// <summary>
        /// Audio encoder. The default is "aac".
        /// </summary>
        public string AudioEncoder { get; set; } = "aac";

        /// <summary>
        /// Target video bitrate in Kb/sec. The default is 2048.
        /// </summary>
        public int VideoBitrate { get; set; } = 2048;

        /// <summary>
        /// Video encoder. The default is "libx264".
        /// </summary>
        public string VideoEncoder { get; set; } = "libx264";

        /// <summary>
        /// Video height (480p, 720p, 1080p). The default is zero (original).
        /// </summary>
        public int VideoHeight { get; set; } = 0;

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
            List<string> args = new List<string>();

            args.Add($@"-i ""{inputFilePath}"""); // Input.

            if (VideoEncoder == null)
            {
                args.Add("-c:v copy");
            }
            else
            {
                args.Add("-c:v " + VideoEncoder);

                if (string.Equals(VideoEncoder, "libx264", StringComparison.OrdinalIgnoreCase))
                    args.Add("-profile:v high -preset slow");

                if (VideoBitrate != 0)
                    args.Add($"-b:v {VideoBitrate}k -maxrate {VideoBitrate}k -bufsize {VideoBitrate * 2}k");

                if (VideoHeight != 0)
                    args.Add("-vf scale=-2:" + VideoHeight);
            }

            if (AudioEncoder == null)
            {
                args.Add("-c:a copy");
            }
            else
            {
                args.Add("-c:a " + AudioEncoder);

                if (AudioChannels != 0)
                    args.Add("-ac " + AudioChannels);

                if (AudioBitrate != 0)
                    args.Add("-b:a " + AudioBitrate + "k");
            }

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
                    string errorText = (errors.Count > 1)
                        ? Environment.NewLine + string.Join(Environment.NewLine, errors)
                        : errors.FirstOrDefault();

                    throw new FFmpegException($"FFMpeg exited with code {process.ExitCode}. {errorText}", process.ExitCode);
                }

                scope.Complete();
            }
        }
    }
}