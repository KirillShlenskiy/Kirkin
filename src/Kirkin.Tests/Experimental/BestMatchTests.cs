using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Kirkin.Tests.Experimental
{
    public class BestMatchTests
    {
        [Fact]
        public void Matching()
        {
            Info[] left = {
                new Info('a', 'b', 'c'),
                new Info('d', 'e', 'f'),
                new Info('a', 'b', 'f')
            };

            Info[] right = {
                new Info('a', 'b', 'c'),
                new Info('d', 'e', 'f')
            };

            // Cross product.
            Match[] matches = left
                .SelectMany(l => right.Select(r => new Match(l, r, MatchQuality(l, r))))
                .ToArray();

            // Find best fit.
            // ... ?

            var z = 0;
        }

        static double MatchQuality(Info left, Info right)
        {
            return (double)left.Data.Intersect(right.Data).Count() / 3;
        }

        sealed class Info
        {
            public char[] Data { get; }

            public Info(params char[] data)
            {
                Data = data;
            }

            public override bool Equals(object obj)
            {
                Info other = obj as Info;
                return other != null && other.Data.SequenceEqual(Data);
            }

            public override int GetHashCode()
            {
                return new string(Data).GetHashCode();
            }

            public override string ToString()
            {
                return "[" + string.Join(", ", Data) + "]";
            }
        }

        sealed class Match
        {
            public Info Left { get; }
            public Info Right { get; }
            public double Quality { get; }

            public Match(Info left, Info right, double quality)
            {
                Left = left;
                Right = right;
                Quality = quality;
            }

            public override string ToString()
            {
                return $"{Left} -> {Right} (quality: {Quality:0.###})";
            }
        }
    }
}