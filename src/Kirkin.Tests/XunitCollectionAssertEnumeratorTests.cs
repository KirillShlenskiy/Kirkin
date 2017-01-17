﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using Xunit.Sdk;

namespace Kirkin.Tests
{
    public class XunitCollectionAssertEnumeratorTests
    {
        [Fact]
        public void EnumeratorDisposedOnAssertEmpty()
        {
            Enumerator<int> enumerator = new Enumerator<int>(Enumerable.Empty<int>());

            Assert.Empty(enumerator);
            Assert.True(enumerator.IsDisposed); // Fails.
        }

        [Fact]
        public void EnumeratorDisposedOnAssertNotEmpty()
        {
            Enumerator<int> enumerator = new Enumerator<int>(Enumerable.Range(0, 1));

            Assert.NotEmpty(enumerator);
            Assert.True(enumerator.IsDisposed); // Fails.
        }

        sealed class Enumerator<T> : IEnumerable<T>, IEnumerator<T>
        {
            private IEnumerator<T> _enumerator; // Null when disposed.

            public T Current
            {
                get
                {
                    return _enumerator.Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return _enumerator.Current;
                }
            }

            public bool IsDisposed
            {
                get
                {
                    return _enumerator == null;
                }
            }

            public Enumerator(IEnumerable<T> enumerable)
            {
                _enumerator = enumerable.GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this;
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                _enumerator.Dispose();
                _enumerator = null;
            }
        }

        [Fact]
        public void EnumeratorDisposedOnAssertEmptyFix()
        {
            Enumerator<int> enumerator = new Enumerator<int>(Enumerable.Empty<int>());

            Empty(enumerator);
            Assert.True(enumerator.IsDisposed);
        }

        [Fact]
        public void EnumeratorDisposedOnAssertNotEmptyFix()
        {
            Enumerator<int> enumerator = new Enumerator<int>(Enumerable.Range(0, 1));

            NotEmpty(enumerator);
            Assert.True(enumerator.IsDisposed);
        }

        public static void Empty(IEnumerable collection)
        {
            IEnumerator enumerator = collection.GetEnumerator();

            try
            {
                if (enumerator.MoveNext())
                    throw new EmptyException();
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }

        public static void NotEmpty(IEnumerable collection)
        {
            IEnumerator enumerator = collection.GetEnumerator();

            try
            {
                if (!enumerator.MoveNext())
                    throw new NotEmptyException();
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }
    }
}