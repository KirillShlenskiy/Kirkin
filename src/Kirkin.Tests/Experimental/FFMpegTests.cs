using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using Kirkin.Diagnostics;

using NUnit.Framework;

namespace Kirkin.Tests.Experimental
{
    public class FFmpegTests
    {
        [Test]
        public void TestConvert()
        {
            string inputFilePath = @"";
            string outputFilePath = @"";

            ConvertVideo(inputFilePath, outputFilePath);
        }

        private static void ConvertVideo(string inputFilePath, string outputFilePath, int bitrateK = 2048)
        {
            string args = $@"-i ""{inputFilePath}"" -c:v libx264 -b:v {bitrateK}k -c:a copy -y -v warning ""{outputFilePath}""";

            ProcessStartInfo info = new ProcessStartInfo("ffmpeg", args) {
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