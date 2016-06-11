using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Kirkin.Linq;

using Xunit;

namespace Kirkin.Tests.Linq
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void ChunkifyPerformance()
        {
            var arr = Enumerable.Range(0, 10).ToArray();

            for (var i = 0; i < 1000000; i++)
            {
                foreach (var chunk in arr.Chunkify(3))
                foreach (var item in chunk)
                {
                    // Materialisation.
                }
            }
        }

        [Fact]
        public void Chunkify()
        {
            var arr = Enumerable.Range(0, 10).ToArray();
            var chunks = arr.Chunkify(3).ToArray();

            this.ValidateChunks(chunks);
        }

        private void ValidateChunks(int[][] chunks)
        {
            Assert.Equal(4, chunks.Length);

            // Chunk 1.
            Assert.Equal(3, chunks[0].Length);
            Assert.Equal(0, chunks[0][0]);
            Assert.Equal(1, chunks[0][1]);
            Assert.Equal(2, chunks[0][2]);

            // Chunk 2.
            Assert.Equal(3, chunks[1].Length);
            Assert.Equal(3, chunks[1][0]);
            Assert.Equal(4, chunks[1][1]);
            Assert.Equal(5, chunks[1][2]);

            // Chunk 3.
            Assert.Equal(3, chunks[2].Length);
            Assert.Equal(6, chunks[2][0]);
            Assert.Equal(7, chunks[2][1]);
            Assert.Equal(8, chunks[2][2]);

            // Chunk 4.
            Assert.Equal(1, chunks[3].Length);
            Assert.Equal(9, chunks[3][0]);
        }

        [Fact]
        public void ChunkifyCorrectChunkNumber()
        {
            Assert.Equal(0, Enumerable.Empty<int>().Chunkify(3).Count());
            Assert.Equal(3, Enumerable.Range(0, 9).Chunkify(3).Count());
            Assert.Equal(4, Enumerable.Range(0, 10).Chunkify(3).Count());
        }

        [Fact]
        public void FlattenSimple()
        {
            var dummy = new NestedDummy
            {
                ID = 1,
                Children = new[] {
                    new NestedDummy {
                        ID = 2,
                        Children = new[] {
                            new NestedDummy { ID = 3 }
                        }
                    },
                    new NestedDummy {
                        ID = 4,
                        Children = new[] {
                            new NestedDummy { ID = 5 }
                        }
                    },
                }
            };

            var flattenedDummies = new[] { dummy }
                .Flatten(d => d.Children)
                .ToArray();

            Assert.Equal(5, flattenedDummies.Length);

            for (int i = 0; i < flattenedDummies.Length; i++)
            {
                Assert.Equal(i + 1, flattenedDummies[i].ID);
            }
        }

        [Fact]
        public void FlattenCircular()
        {
            var troubleDummy = new NestedDummy { ID = 1 };

            var dummy = new NestedDummy
            {
                ID = 2,
                Children = new[] {
                    new NestedDummy {
                        ID = 3,
                        Children = new[] {
                            new NestedDummy { ID = 4 }
                        }
                    },
                    new NestedDummy {
                        ID = 5,
                        Children = new[] {
                            new NestedDummy { ID = 6 },
                            troubleDummy
                        }
                    },
                }
            };

            troubleDummy.Children = new[] { dummy };

            var flattenedDummies = new[] { troubleDummy }
                .Flatten(d => d.Children)
                .ToArray();

            Assert.Equal(6, flattenedDummies.Length);

            for (int i = 0; i < flattenedDummies.Length; i++)
            {
                Assert.Equal(i + 1, flattenedDummies[i].ID);
            }
        }

        sealed class NestedDummy
        {
            public int ID { get; set; }
            public NestedDummy[] Children { get; set; }
        }

        [Fact]
        public void FirstOrDefaultWithMin()
        {
            var dummies = ThreeDummies();

            Assert.Equal(1, dummies.FirstOrDefaultWithMin(d => d.ID).ID);
            Assert.Equal(dummies.OrderBy(d => d.ID).FirstOrDefault(), dummies.FirstOrDefaultWithMin(d => d.ID));
            Assert.Equal(dummies.FirstOrDefault(d => d.ID == dummies.Min(o => o.ID)), dummies.FirstOrDefaultWithMin(d => d.ID));
        }

        [Fact]
        public void FirstOrDefaultWithMinCollision()
        {
            var dummies = ThreeDummiesWithCollisions();

            Assert.Equal("First 1", dummies.FirstOrDefaultWithMin(d => d.ID).Value);
            Assert.Equal(dummies.OrderBy(d => d.ID).FirstOrDefault(), dummies.FirstOrDefaultWithMin(d => d.ID));
            Assert.Equal(dummies.FirstOrDefault(d => d.ID == dummies.Min(o => o.ID)), dummies.FirstOrDefaultWithMin(d => d.ID));
        }

        [Fact]
        public void LastOrDefaultWithMax()
        {
            var dummies = ThreeDummies();

            Assert.Equal(3, dummies.LastOrDefaultWithMax(d => d.ID).ID);
            Assert.Equal(dummies.OrderBy(d => d.ID).LastOrDefault(), dummies.LastOrDefaultWithMax(d => d.ID));
            Assert.Equal(dummies.LastOrDefault(d => d.ID == dummies.Max(o => o.ID)), dummies.LastOrDefaultWithMax(d => d.ID));
        }

        [Fact]
        public void LastOrDefaultWithMaxCollision()
        {
            var dummies = ThreeDummiesWithCollisions();

            Assert.Equal("Third 2", dummies.LastOrDefaultWithMax(d => d.ID).Value);
            Assert.Equal(dummies.OrderBy(d => d.ID).LastOrDefault(), dummies.LastOrDefaultWithMax(d => d.ID));
            Assert.Equal(dummies.LastOrDefault(d => d.ID == dummies.Max(o => o.ID)), dummies.LastOrDefaultWithMax(d => d.ID));
        }

        [Fact]
        public void PerfFirstOrDefaultWithMin()
        {
            var dummies = ThreeDummies();
            
            for (int i = 0; i < 1000000; i++)
            {
                var dummy = dummies.FirstOrDefaultWithMin(d => d.ID);
            }
        }

        [Fact]
        public void PerfOrderBy()
        {
            var dummies = ThreeDummies();

            for (int i = 0; i < 1000000; i++)
            {
                var dummy = dummies.OrderBy(d => d.ID).FirstOrDefault();
            }
        }

        static Dummy[] ThreeDummies()
        {
            return new[] {
                new Dummy { ID = 1, Value = "First" },
                new Dummy { ID = 2, Value = "Second" },
                new Dummy { ID = 3, Value = "Third" }
            };
        }

        static Dummy[] ThreeDummiesWithCollisions()
        {
            return new[] {
                new Dummy { ID = 1, Value = "First 1" },
                new Dummy { ID = 1, Value = "First 2" },
                new Dummy { ID = 2, Value = "Second" },
                new Dummy { ID = 3, Value = "Third 1" },
                new Dummy { ID = 3, Value = "Third 2" }
            };
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }

        [Fact]
        public void LookAhead()
        {
            for (int i = 0; i < 10000; i++)
            {
                var collection = Enumerable.Range(0, 10);

                Assert.Equal(collection, collection.LookAhead());
            }
        }

        [Fact]
        public void LookAheadExceptionHandling()
        {
            int errorAfter = 3;
            var collection = GenerateWithException(errorAfter);

            foreach (var i in collection.LookAhead(1).Take(1)) {
                Debug.Print("Seen {0}.", i);
            }

            Assert.Equal(2, collection.LookAhead(1).Take(errorAfter - 1).Count());
            Assert.Equal(1, collection.LookAhead(errorAfter - 1).Take(1).Count());
            Assert.Throws<InvalidOperationException>(() => collection.LookAhead(1).Take(errorAfter).ToArray());
            Assert.Throws<InvalidOperationException>(() => collection.LookAhead(errorAfter).Take(1).ToArray());
        }

        [Fact]
        public void LookAheadPredictableTimeComplexity()
        {
            int lastSeenNumber = -1;

            var collection = Enumerable
                .Range(1, 10)
                .Select(i =>
                {
                    lastSeenNumber = i;

                    return i;
                });

            collection.LookAhead().Take(1).ToArray();

            Assert.Equal(2, lastSeenNumber);

            // In the below tests the consumer finishes too
            // quickly causing the producer to break out early,
            // making the number of items non-deterministic
            // (lastSeenNumber <= bufferSize + takeCount).
            collection.LookAhead(2).Take(3).ToArray();

            Assert.True(lastSeenNumber <= 5, "lastSeenNumber expected to be 5 at most.");

            collection.LookAhead(3).Take(3).ToArray();

            Assert.True(lastSeenNumber <= 6, "lastSeenNumber expected to be 6 at most.");

            // Artificially slowed down consumer: lastSeenNumber = bufferSize + take count.
            foreach (var _ in collection.LookAhead(2).Take(3)) {
                Thread.Sleep(20);
            }

            Assert.Equal(5, lastSeenNumber);

            foreach (var _ in collection.LookAhead(3).Take(3)) {
                Thread.Sleep(20);
            }

            Assert.Equal(6, lastSeenNumber);
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        private IEnumerable<int> GenerateWithException(int exceptionAfter)
        {
            for (int i = 0; i < exceptionAfter; i++)
            {
                yield return i;
            }

            throw new InvalidOperationException("Exception after " + exceptionAfter.ToString());
        }
    }
}