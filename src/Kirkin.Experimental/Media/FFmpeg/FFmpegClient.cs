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
        /// Audio encoder. The default is "aac".
        /// </summary>
        public AudioEncoder AudioEncoder { get; set; } = AudioEncoder.AAC;

        /// <summary>
        /// Target audio bitrate in Kb/sec. The default is 192.
        /// </summary>
        public int AudioBitrate { get; set; } = 192;

        /// <summary>
        /// Number of audio channels. The default is zero (original).
        /// </summary>
        public int AudioChannels { get; set; } = 0;

        /// <summary>
        /// Video encoder. The default is libx264, slow preset.
        /// </summary>
        public VideoEncoder VideoEncoder { get; set; } = VideoEncoder.Libx264Slow;

        /// <summary>
        /// Target video bitrate in Kb/sec. The default is 0 (auto).
        /// A good guide is as follows:
        /// * 1080p (VideoHeight = 1080): ~2000 kpbs
        /// * 720p (VideoHeight = 720): ~1500 kbps
        /// * 480p (VideoHeight = 480): ~1000 kbps
        /// </summary>
        public int VideoBitrate { get; set; }

        /// <summary>
        /// Effective video bitrate.
        /// </summary>
        internal int VideoBitrateResolved
        {
            get
            {
                if (VideoBitrate != 0) return VideoBitrate;
                
                if (VideoHeight.HasValue)
                {
                    if (VideoHeight.Value >= 1440) return 2500;
                    if (VideoHeight.Value >= 1080) return 2000;
                    if (VideoHeight.Value >= 720) return 1500;
                    if (VideoHeight.Value >= 480) return 1000;

                    return 500;
                }

                return 0;
            }
        }

        /// <summary>
        /// Video width. The default is null (scale, preserve aspect ratio if VideoHeight is greater than zero).
        /// </summary>
        public int? VideoWidth { get; set; }

        /// <summary>
        /// Video height (usually 480, 720, 1080). The default is null (scale, preserve aspect ratio if VideoWidth is greater than zero).
        /// </summary>
        public int? VideoHeight { get; set; }

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
            ProcessStartInfo info = new ProcessStartInfo(FFmpegPath ?? "ffmpeg", GetFfmpegArgs(inputFilePath, outputFilePath)) {
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

                    throw new FFmpegException($"FFMpeg exited with code {process.ExitCode}. {errorText?.TrimStart(' ' , ':')}", process.ExitCode);
                }

                scope.Complete();
            }
        }

        private string GetFfmpegArgs(string inputFilePath, string outputFilePath)
        {
            List<string> args = new List<string>();

            args.Add($@"-i ""{inputFilePath}"""); // Input.
            args.Add(VideoEncoder.GetCliArgs(this));

            if (VideoWidth.HasValue || VideoHeight.HasValue) {
                // -2 means "auto, preserve aspect ratio.
                args.Add($"-vf scale={VideoWidth ?? -2}:{VideoHeight ?? -2}");
            }

            args.Add(AudioEncoder.GetCliArgs(this));

            if (AudioChannels != 0)
                args.Add("-ac " + AudioChannels);

            args.Add("-y"); // Overwrite files without prompting.
            args.Add("-v warning"); // Output verbosity level.
            args.Add($@"""{outputFilePath}"""); // Output.

            return string.Join(" ", args);
        }

        public override string ToString()
        {
            return FFmpegPath ?? "ffmpeg" + " " + GetFfmpegArgs("<input>", "<output>");
        }
    }
}