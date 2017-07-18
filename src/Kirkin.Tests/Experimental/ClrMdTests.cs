using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.Diagnostics.Runtime;

using NUnit.Framework;

namespace Kirkin.Tests.Experimental
{
    public class ClrMdTests
    {
        private readonly int PID;

        public ClrMdTests()
        {
            PID = Process.GetProcessesByName("ArdexServicesMonitor").Single().Id;
        }

        [Test]
        public void BasicDump()
        {
            using (var dataTarget = DataTarget.AttachToProcess(PID, 10000, AttachFlag.Invasive))
            {
                // Dump CLR info
                var clrVersion = dataTarget.ClrVersions.First();
                var dacInfo = clrVersion.DacInfo;

                Console.WriteLine("# CLR Info");
                Console.WriteLine("Version:   {0}", clrVersion.Version);
                Console.WriteLine("Filesize:  {0:X}", dacInfo.FileSize);
                Console.WriteLine("Timestamp: {0:X}", dacInfo.TimeStamp);
                Console.WriteLine("Dac file:  {0}", dacInfo.FileName);
            }
        }

        [Test]
        public void StringDump()
        {
            using (var dataTarget = DataTarget.AttachToProcess(PID, 10000, AttachFlag.Invasive))
            {
                var clrVersion = dataTarget.ClrVersions.First();
                var runtime = clrVersion.CreateRuntime();
                var heap = runtime.Heap;
                var numberOfStrings = 0;
                var uniqueStrings = new Dictionary<string, int>();

                foreach (var ptr in heap.EnumerateObjectAddresses())
                {
                    var type = heap.GetObjectType(ptr);

                    // Skip if not a string
                    if (type == null || type.IsString == false)
                    {
                        continue;
                    }

                    // Count total
                    numberOfStrings++;

                    // Get value
                    var text = (string)type.GetValue(ptr);

                    if (uniqueStrings.ContainsKey(text))
                    {
                        uniqueStrings[text]++;
                    }
                    else
                    {
                        uniqueStrings[text] = 1;
                    }
                }

                Console.WriteLine("## String info");
                Console.WriteLine("String count:     {0}", numberOfStrings);
                Console.WriteLine("");

                Console.WriteLine("Most duplicated strings: (top 5)");
                foreach (var keyValuePair in uniqueStrings.OrderByDescending(kvp => kvp.Value).Take(5))
                {
                    Console.WriteLine("* {0} usages: {1}", keyValuePair.Value, keyValuePair.Key);
                }
            }
        }
    }
}
