using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kirkin.ChangeTracking;

using Newtonsoft.Json;

using NUnit.Framework;

namespace Kirkin.Tests
{
    public class TimestampTests
    {
        [Test]
        public void TimestampBasics()
        {
            var timestamp = new Timestamp(10);

            Assert.AreEqual("A", timestamp.ToString());
            Assert.AreEqual(10, timestamp.ToUInt64());
            Assert.AreEqual(10, timestamp.ToInt64());

            Debug.Print("Done.");
        }

        [Test]
        public void Performance()
        {
            var memStart = GC.GetTotalMemory(true);

            for (int i = 0; i < 1000000; i++)
            {
                //var t = new Timestamp("AAA");
                var t = new Timestamp(-1);

                t++;
            }

            var memEnd = GC.GetTotalMemory(false);

            Debug.Print("Mem diff new: {0}", memEnd - memStart);
            Debug.Print("Done.");
        }

        [Test]
        public void NullCompare()
        {
            Assert.AreNotSame(null, default(Timestamp));
        }

        [Test]
        public void ComparisonTests()
        {
            var t1 = new Timestamp(1);
            var t2 = new Timestamp(2);

            Assert.True(t1 < t2);
            Assert.True(t2 > t1);
            Assert.AreEqual(t1.CompareTo(t2), 1.CompareTo(2));
        }

        [Test]
        public void IncrementDecrement()
        {
            var t1 = new Timestamp(1);

            Assert.AreNotEqual(new Timestamp(2), t1);
            Assert.AreEqual(new Timestamp(2), ++t1);
            Assert.AreEqual(new Timestamp(1), --t1);
        }

        [Test]
        public void Load()
        {
            Assert.AreEqual(new Timestamp(1), new Timestamp(new byte[] { 1 }));
            Assert.AreEqual(Timestamp.Parse("101"), new Timestamp(new byte[] { 1, 1 }));
            Assert.AreEqual(Timestamp.Parse("0x101-0101-0101-0101"), new Timestamp(new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 }));
        }

        [Test]
        public void ToStringTests()
        {
            foreach (var i in new long[] { long.MinValue, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 100, 1000, 10000, 100000, long.MaxValue })
            {
                string expectedValue = (i == 0) ? "" : Convert.ToString(i, 16).ToUpper();

                Assert.AreEqual(expectedValue, new Timestamp(i).ToString());
            }
        }

        [Test]
        public void ToStringPerf()
        {
            for (var i = 0; i < 1000000; i++)
            {
                var t = new Timestamp(i).ToString();
            }
        }

        [Test]
        public void Ordering()
        {
            var comparer = Comparer<Timestamp>.Create((x, y) => x.CompareTo(y));

            var timestamps = new[] {
                new Timestamp(100),
                new Timestamp(10),
                new Timestamp(1)
            };

            Assert.True(timestamps.OrderBy(t => t, comparer).SequenceEqual(timestamps.Reverse()));
            Assert.True(timestamps.OrderByDescending(t => t, comparer).SequenceEqual(timestamps));
        }

        [Test]
        public void Serialization()
        {
            var ser = new JsonSerializer();

            ser.Converters.Add(new TimestampJsonConverter());

            var sb = new StringBuilder();

            var sourceDummy = new Dummy {
                ID = 1,
                Timestamp = new Timestamp(ulong.MaxValue)
            };

            using (var w = new StringWriter(sb)) {
                ser.Serialize(w, sourceDummy);
            }

            var str = sb.ToString();
            Dummy targetDummy;
            
            using (var r = new StringReader(str)) {
                targetDummy = (Dummy)ser.Deserialize(r, typeof(Dummy));
            }

            Assert.True(PropertyValueEqualityComparer<Dummy>.Default.Equals(sourceDummy, targetDummy));
            Debug.Print("Done.");
        }

        public class TimestampJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Timestamp);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.ValueType != typeof(string)) {
                    throw new InvalidOperationException("String type expected.");
                }

                return Timestamp.Parse((string)reader.Value);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var timestamp = (Timestamp)value;

                writer.WriteValue(timestamp.ToString());
            }
        }

        public sealed class Dummy
        {
            public int ID { get; set; }
            public Timestamp Timestamp { get; set; }
        }
    
        [Test]
        public void TornReadViaReassignment()
        {
            // When a method on a struct is executing, it is possible
            // for it to see a different value of "this" if it is dereferenced
            // multiple times and the field or variable containing the struct
            // is being reassigned by another thread. Consider the below.
            var timestamp = new Timestamp();

            var reassignment = Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();

                while (sw.ElapsedMilliseconds < 1000) {
                    // Reassignment which frequently results in zero
                    // value for the timstamp (which is unique in how
                    // it is handled by the ToString call).
                    timestamp = new Timestamp((timestamp.ToInt64() + 1) % 4);
                }
            });

            while (!reassignment.IsCompleted)
            {
                Assert.AreNotEqual("0", timestamp.ToString());
            }
        }

        [Test]
        public void TimestampParsePerf()
        {
            var text = new Timestamp(123).ToString();
            var textWPrefix = "0x" + text;
            var textWSuffix = textWPrefix + "-";

            for (int i = 0; i < 1000000; i++)
            {
                Timestamp.Parse(text);
                Timestamp.Parse(textWPrefix);
                Timestamp.Parse(textWSuffix);
            }
        }

        [Test]
        public void ParseRoundtripMaxValue()
        {
            Timestamp t = new Timestamp(-1);
            string s = t.ToString();

            Assert.AreEqual(t, Timestamp.Parse(s));
        }

        [Test]
        public void ArrayRoundtripMaxValue()
        {
            Timestamp t = new Timestamp(-1);
            byte[] arr = t.ToArray();

            Assert.AreEqual(t, new Timestamp(arr));
        }

        [Test]
        public void TimestampNullOrEmptyInputThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Timestamp.Parse(null));
            Assert.Throws<ArgumentException>(() => Timestamp.Parse(""));
        }

        [Test]
        public void ToByteArrayTests()
        {
            var bytes = new byte[] { 255, 255, 255, 255, 255, 255, 255, 255 };
            var recreatedBytes = new Timestamp(ulong.MaxValue).ToArray();

            Assert.True(bytes.SequenceEqual(recreatedBytes));
            Assert.True(bytes.SequenceEqual(new Timestamp(bytes).ToArray()));
        }

        [Test]
        public void ToArrayBenchmark()
        {
            for (int i = 0; i < 10000000; i++)
            {
                new Timestamp(1).ToArray();
            }
        }

        [Test]
        public void InterlockedBasics()
        {
            Timestamp timestamp = new Timestamp(1);

            Assert.AreEqual(Timestamp.InterlockedIncrement(ref timestamp), timestamp);
            Assert.AreEqual(new Timestamp(2), timestamp);

            Assert.AreEqual(Timestamp.InterlockedDecrement(ref timestamp), timestamp);
            Assert.AreEqual(new Timestamp(1), timestamp);

            Assert.AreEqual(timestamp, Timestamp.InterlockedExchange(ref timestamp, new Timestamp(-1)));
            Assert.AreEqual(new Timestamp(-1), timestamp);

            Assert.AreEqual(timestamp, Timestamp.InterlockedCompareExchange(ref timestamp, new Timestamp(1), new Timestamp(1)));
            Assert.AreEqual(new Timestamp(-1), timestamp); // Unchanged.

            Assert.AreEqual(timestamp, Timestamp.InterlockedCompareExchange(ref timestamp, new Timestamp(1), new Timestamp(-1)));
            Assert.AreEqual(new Timestamp(1), timestamp); // Changed.
        }
    }
}