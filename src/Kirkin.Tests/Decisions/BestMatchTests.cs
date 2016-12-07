using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Kirkin.Tests.Decisions
{
    public class BestMatchTests
    {
        [Fact]
        public void Matching()
        {
            string[] left = { "zzz", "abc", "def", "abf" };
            string[] right = { "abc", "def", "caf" };

            Match[] bestMatches = left
                .SelectMany(l => right.Select(r => new Match(l, r))) // Cross product: all possible matches.
                .Where(m => m.Quality > 0)
                .OrderByDescending(m => m.Quality)
                // Collect.
                .Aggregate(new List<Match>(), (list, match) =>
                {
                    // We only want items on the left and right sides to appear in the result once.
                    if (!list.Any(m => m.Left == match.Left || m.Right == match.Right)) {
                        list.Add(match);
                    }

                    return list;
                })
                .ToArray();

            Assert.Equal(3, bestMatches.Length);
        }

        sealed class Match
        {
            public string Left { get; }
            public string Right { get; }

            public double Quality
            {
                get
                {
                    return (double)Left.Intersect(Right).Count() / 3;
                }
            }

            public Match(string left, string right)
            {
                Left = left;
                Right = right;
            }

            public override string ToString()
            {
                return $"{Left} -> {Right} (quality: {Quality:0.###})";
            }
        }
    }
}