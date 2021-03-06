﻿using System;
using System.Linq;

using Kirkin.Functional;

using NUnit.Framework;

namespace Kirkin.Tests.Functional
{
    public class DisposableTests
    {
        [Test]
        public void Create()
        {
            var i = 0;
            var d = Disposable.Create(() => i++);

            d.Dispose();
            d.Dispose();
            d.Dispose();

            Assert.AreEqual(1, i);
        }

        [Test]
        public void Combine()
        {
            var i = 0;
            var count = 10;
            var disposable = Disposable.Combine(Enumerable.Range(0, count).Select(_ => Disposable.Create(() => i++)));

            disposable.Dispose();
            disposable.Dispose();
            disposable.Dispose();

            Assert.AreEqual(count, i);
        }

        [Test]
        public void CombinePartialDispose()
        {
            var i = 0;
            var errored = false;

            try
            {
                var disposable = Disposable.Combine(Enumerable.Range(0, 10).Select(a =>
                {
                    if (a == 5)
                        throw new InvalidOperationException("Expected");

                    return Disposable.Create(() => i++);
                }));
            }
            catch (InvalidOperationException)
            {
                errored = true;
            }

            Assert.True(errored);
            Assert.AreEqual(5, i);
        }

        [Test]
        public void ParametrizedCreate1()
        {
            var i = 0;
            var d = Disposable.Create(123, _ => i++);

            d.Dispose();
            d.Dispose();
            d.Dispose();

            Assert.AreEqual(1, i);
        }

        [Test]
        public void ParametrizedCreate2()
        {
            var i = 1;
            var d = Disposable.Create(i, j => i = j + 1);

            d.Dispose();
            d.Dispose();
            d.Dispose();

            Assert.AreEqual(2, i);
        }

        [Test]
        public void Empty()
        {
            var d = Disposable.Empty;

            d.Dispose();
            d.Dispose();

            using (Disposable.Empty)
            {
                // Do nothing.
            }
        }
    }
}
