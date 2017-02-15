using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kirkin.Diagnostics;

using NUnit.Framework;

namespace Kirkin.Tests.Diagnostics
{
    public class ProcessMemoryTests
    {
        [Test]
        public void ReadWriteNotepadMemory()
        {
            Directory.CreateDirectory(@"C:\Temp");
            string filePath = @"C:\Temp\Text.txt";

            File.WriteAllText(filePath, "Hello world.", Encoding.Unicode);

            using (Process notepad = Process.Start("notepad", $"/W {filePath}"))
            {
                try
                {
                    ProcessMemory memory = new ProcessMemory(notepad);
                    byte[] allMemory = memory.Read();
                    string allText = Encoding.UTF8.GetString(allMemory);

                    Assert.True(allText.IndexOf("Hello world", StringComparison.OrdinalIgnoreCase) != -1, "Text not found.");
                }
                finally
                {
                    notepad.Kill();
                }
            }
        }
    }
}