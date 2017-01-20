using NUnit.Framework;

using Kirkin.Collections.Specialized;

namespace Kirkin.Tests.Collections.Generic
{
    public class UndoRedoCollectionTests
    {
        [Test]
        public void BasicTest()
        {
            var collection = new UndoRedoCollection<string>();

            Assert.False(collection.CanUndo);
            Assert.False(collection.CanRedo);

            collection.Add("hello"); // [hello]

            Assert.AreEqual("hello", collection.Current);
            Assert.True(collection.CanUndo);
            Assert.False(collection.CanRedo);

            collection.Add("world"); // hello [world]

            Assert.AreEqual("world", collection.Current);
            Assert.AreEqual("hello world", string.Join(" ", collection));
            Assert.True(collection.CanUndo);
            Assert.False(collection.CanRedo);
            
            collection.Add("dummy"); // hello world [dummy]

            Assert.AreEqual("dummy", collection.Current);
            Assert.AreEqual("hello world dummy", string.Join(" ", collection));
            Assert.True(collection.CanUndo);
            Assert.False(collection.CanRedo);

            collection.Undo(); // hello [world] dummy

            Assert.AreEqual("world", collection.Current);
            Assert.AreEqual("hello world dummy", string.Join(" ", collection));
            Assert.True(collection.CanUndo);
            Assert.True(collection.CanRedo);

            collection.Undo(); // [hello] world dummy

            Assert.AreEqual("hello", collection.Current);
            Assert.AreEqual("hello world dummy", string.Join(" ", collection));
            Assert.True(collection.CanUndo);
            Assert.True(collection.CanRedo);

            collection.Add("you"); // hello [you]

            Assert.AreEqual("you", collection.Current);
            Assert.AreEqual("hello you", string.Join(" ", collection));
            Assert.True(collection.CanUndo);
            Assert.False(collection.CanRedo);
        }

        [Test]
        public void Capacity()
        {
            var collection = new UndoRedoCollection<string> { "hello", "goddamn", "world" };

            Assert.AreEqual(3, collection.Count);

            collection.Capacity = 2;

            Assert.AreEqual(2, collection.Count);
            Assert.AreEqual("goddamn world", string.Join(" ", collection));
            Assert.AreEqual("world", collection.Current);

            collection.Add("hello");

            Assert.AreEqual(2, collection.Count);
            Assert.AreEqual("world hello", string.Join(" ", collection));
            Assert.AreEqual("hello", collection.Current);

            collection.Add("world");

            Assert.AreEqual(2, collection.Count);
            Assert.AreEqual("hello world", string.Join(" ", collection));
            Assert.AreEqual("world", collection.Current);
        }
    }
}
