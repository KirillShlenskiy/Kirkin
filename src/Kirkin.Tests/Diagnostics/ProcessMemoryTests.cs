using System.Diagnostics;
using System.IO;

using Kirkin.Diagnostics;

using NUnit.Framework;

namespace Kirkin.Tests.Diagnostics
{
    public class ProcessMemoryTests
    {
        [Test]
        [Ignore("Skip")]
        public void ReadNotepadMemory()
        {
            Directory.CreateDirectory(@"C:\Temp");
            string filePath = @"C:\Temp\Text.txt";

            File.WriteAllText(filePath, "Hello world.");

            byte[] bytes;

            using (Process notepad = Process.Start("notepad", filePath))
            {
                try
                {
                    ProcessMemory memory = new ProcessMemory(notepad);
                    bytes = memory.Read();
                }
                finally
                {
                    notepad.Kill();
                }
            }

            Assert.AreNotEqual(0, bytes.Length);
        }
    }
}