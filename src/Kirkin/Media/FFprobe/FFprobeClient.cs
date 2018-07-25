#if !NETSTANDARD2_0

using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

using Kirkin.Diagnostics;

namespace Kirkin.Media.FFprobe
{
    /// <summary>
    /// Managed FFprobe wrapper.
    /// </summary>
    public sealed class FFprobeClient
    {
        /// <summary>
        /// ffprobe.exe path specified when this instance was created.
        /// The default is null (use current directory/PATH).
        /// </summary>
        public string FFprobePath { get; }

        /// <summary>
        /// Creates a new ffprobe wrapper instance without specifying the exact ffprobe.exe path.
        /// </summary>
        public FFprobeClient()
        {
        }

        /// <summary>
        /// Creates a new ffprobe wrapper instance with the given ffprobe.exe path.
        /// </summary>
        public FFprobeClient(string ffprobePath)
        {
            FFprobePath = ffprobePath;
        }

        /// <summary>
        /// Returns information describing audio and video streams in the file at the given path.
        /// </summary>
        public FFprobeStreamInfo[] GetVideoInfo(string filePath)
        {
            if (!Path.IsPathRooted(filePath)) {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            }

            string args = $"\"{filePath}\" -v error -show_streams -of xml";

            ProcessStartInfo processStartInfo = new ProcessStartInfo(FFprobePath ?? "ffprobe.exe", args) {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            string output;

            using (Process process = ChildProcess.Start(processStartInfo, out bool associated)) {
                output = process.StandardOutput.ReadToEnd();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(__FFprobeResult));

            using (StringReader reader = new StringReader(output))
            {
                __FFprobeResult result = (__FFprobeResult)serializer.Deserialize(reader);

                if (result.streams.Length == 0) {
                    throw new InvalidOperationException("Video stream not found.");
                }

                return result.streams;
            }
        }

        [XmlType("ffprobe")]
        public sealed class __FFprobeResult
        {
            public FFprobeStreamInfo[] streams { get; set; }
        }
    }
}

#endif