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
            using (DataTarget dataTarget = DataTarget.AttachToProcess(PID, 10000, AttachFlag.Invasive))
            {
                // Dump CLR info
                ClrInfo clrVersion = dataTarget.ClrVersions[0];
                DacInfo dacInfo = clrVersion.DacInfo;

                Console.WriteLine("# CLR Info");
                Console.WriteLine("Version:   {0}", clrVersion.Version);
                Console.WriteLine("Filesize:  {0:X}", dacInfo.FileSize);
                Console.WriteLine("Timestamp: {0:X}", dacInfo.TimeStamp);
                Console.WriteLine("Dac file:  {0}", dacInfo.FileName);
            }
        }

        [Test]
        public void TypeStats()
        {
            using (DataTarget dataTarget = DataTarget.AttachToProcess(PID, 10000, AttachFlag.Invasive))
            {
                ClrInfo clrVersion = dataTarget.ClrVersions[0];
                ClrRuntime runtime = clrVersion.CreateRuntime();
                Dictionary<string, int> countsByClrType = new Dictionary<string, int>();

                foreach (ulong ptr in runtime.Heap.EnumerateObjectAddresses())
                {
                    ClrType type = runtime.Heap.GetObjectType(ptr);
                    int count;

                    if (countsByClrType.TryGetValue(type.Name, out count))
                    {
                        count++;
                    }
                    else
                    {
                        count = 0;
                    }

                    countsByClrType[type.Name] = count;
                }

                foreach (KeyValuePair<string, int> kvp in countsByClrType.OrderByDescending(v => v.Value))
                {
                    if (kvp.Value < 10) {
                        continue;
                    }

                    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                }
            }
        }

        [Test]
        public void StringDump()
        {
            using (DataTarget dataTarget = DataTarget.AttachToProcess(PID, 10000, AttachFlag.Invasive))
            {
                ClrInfo clrVersion = dataTarget.ClrVersions[0];
                ClrRuntime runtime = clrVersion.CreateRuntime();
                ClrHeap heap = runtime.Heap;
                int numberOfStrings = 0;
                Dictionary<string, int> uniqueStrings = new Dictionary<string, int>();

                foreach (ulong ptr in heap.EnumerateObjectAddresses())
                {
                    ClrType type = heap.GetObjectType(ptr);

                    // Skip if not a string
                    if (type == null || type.IsString == false) {
                        continue;
                    }

                    // Count total
                    numberOfStrings++;

                    // Get value
                    string text = (string)type.GetValue(ptr);

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

                foreach (KeyValuePair<string, int> keyValuePair in uniqueStrings.OrderByDescending(kvp => kvp.Value).Take(5))
                {
                    Console.WriteLine("* {0} usages: {1}", keyValuePair.Value, keyValuePair.Key);
                }
            }
        }
    }
}
