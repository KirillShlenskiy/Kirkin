using Xunit;

using Kirkin.Collections.Specialised;

namespace Kirkin.Tests.Collections.Generic
{
    public class UndoRedoCollectionTests
    {
        [Fact]
        public void BasicTest()
        {
            var collection = new UndoRedoCollection<string>();

            Assert.False(collection.CanUndo);
            Assert.False(collection.CanRedo);

            collection.Add("hello"); // [hello]

            Assert.Equal("hello", collection.Current);
            Assert.True(collection.CanUndo);
            Assert.False(collection.CanRedo);

            collection.Add("world"); // hello [world]

            Assert.Equal("world", collection.Current);
            Assert.Equal("hello world", string.Join(" ", collection));
            Assert.True(collection.CanUndo);
            Assert.False(collection.CanRedo);
            
            collection.Add("dummy"); // hello world [dummy]

            Assert.Equal("dummy", collection.Current);
            Assert.Equal("hello world dummy", string.Join(" ", collection));
            Assert.True(collection.CanUndo);
            Assert.False(collection.CanRedo);

            collection.Undo(); // hello [world] dummy

            Assert.Equal("world", collection.Current);
            Assert.Equal("hello world dummy", string.Join(" ", collection));
            Assert.True(collection.CanUndo);
            Assert.True(collection.CanRedo);

            collection.Undo(); // [hello] world dummy

            Assert.Equal("hello", collection.Current);
            Assert.Equal("hello world dummy", string.Join(" ", collection));
            Assert.True(collection.CanUndo);
            Assert.True(collection.CanRedo);

            collection.Add("you"); // hello [you]

            Assert.Equal("you", collection.Current);
            Assert.Equal("hello you", string.Join(" ", collection));
            Assert.True(collection.CanUndo);
            Assert.False(collection.CanRedo);
        }

        [Fact]
        public void Capacity()
        {
            var collection = new UndoRedoCollection<string> { "hello", "goddamn", "world" };

            Assert.Equal(3, collection.Count);

            collection.Capacity = 2;

            Assert.Equal(2, collection.Count);
            Assert.Equal("goddamn world", string.Join(" ", collection));
            Assert.Equal("world", collection.Current);

            collection.Add("hello");

            Assert.Equal(2, collection.Count);
            Assert.Equal("world hello", string.Join(" ", collection));
            Assert.Equal("hello", collection.Current);

            collection.Add("world");

            Assert.Equal(2, collection.Count);
            Assert.Equal("hello world", string.Join(" ", collection));
            Assert.Equal("world", collection.Current);
        }
    }
}
