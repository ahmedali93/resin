﻿using System.Linq;
using NUnit.Framework;
using Resin.IO;

namespace Tests
{
    [TestFixture]
    public class LcrsTrieTests
    {
        [Test]
        public void Can_append_tries()
        {
            var one = new LcrsTrie('\0', false);
            one.Add("ape");
            one.Add("app");
            one.Add("banana");

            var two = new LcrsTrie('\0', false);
            two.Add("apple");
            two.Add("banana");

            one.Merge(two);

            Word found;
            Assert.IsTrue(one.HasWord("ape", out found));
            Assert.IsTrue(one.HasWord("app", out found));
            Assert.IsTrue(one.HasWord("apple", out found));
            Assert.IsTrue(one.HasWord("banana", out found));
        }

        [Test]
        public void Can_get_weight()
        {
            var tree = new LcrsTrie('\0', false);
            tree.Add("pap");
            tree.Add("papp");
            tree.Add("papaya");

            Assert.AreEqual(8, tree.Weight);

            tree.Add("ape");
            tree.Add("apelsin");

            Assert.AreEqual(15, tree.Weight);
        }

        [Test]
        public void Can_find_near()
        {
            var tree = new LcrsTrie('\0', false);
            var near = tree.Near("ba", 1).Select(w=>w.Value).ToList();

            Assert.That(near, Is.Empty);

            tree.Add("bad");
            near = tree.Near("ba", 1).Select(w => w.Value).ToList();

            Assert.That(near.Count, Is.EqualTo(1));
            Assert.IsTrue(near.Contains("bad"));

            tree.Add("baby");
            near = tree.Near("ba", 1).Select(w => w.Value).ToList();

            Assert.That(near.Count, Is.EqualTo(1));
            Assert.IsTrue(near.Contains("bad"));

            tree.Add("b");
            near = tree.Near("ba", 1).Select(w => w.Value).ToList();

            Assert.That(near.Count, Is.EqualTo(2));
            Assert.IsTrue(near.Contains("bad"));
            Assert.IsTrue(near.Contains("b"));

            near = tree.Near("ba", 2).Select(w => w.Value).ToList();

            Assert.That(near.Count, Is.EqualTo(3));
            Assert.IsTrue(near.Contains("b"));
            Assert.IsTrue(near.Contains("bad"));
            Assert.IsTrue(near.Contains("baby"));

            near = tree.Near("ba", 0).Select(w => w.Value).ToList();

            Assert.That(near.Count, Is.EqualTo(0));

            tree.Add("bananas");
            near = tree.Near("ba", 6).Select(w => w.Value).ToList();

            Assert.That(near.Count, Is.EqualTo(4));
            Assert.IsTrue(near.Contains("b"));
            Assert.IsTrue(near.Contains("bad"));
            Assert.IsTrue(near.Contains("baby"));
            Assert.IsTrue(near.Contains("bananas"));

            near = tree.Near("bazy", 1).Select(w => w.Value).ToList();

            Assert.That(near.Count, Is.EqualTo(1));
            Assert.IsTrue(near.Contains("baby"));

            tree.Add("bank");
            near = tree.Near("bazy", 3).Select(w => w.Value).ToList();

            Assert.AreEqual(4, near.Count);
            Assert.IsTrue(near.Contains("baby"));
            Assert.IsTrue(near.Contains("bank"));
            Assert.IsTrue(near.Contains("bad"));
            Assert.IsTrue(near.Contains("b"));
        }

        [Test]
        public void Can_find_prefixed()
        {
            var tree = new LcrsTrie('\0', false);

            tree.Add("rambo");
            tree.Add("rambo");

            tree.Add("2");

            tree.Add("rocky");

            tree.Add("2");

            tree.Add("raiders");

            tree.Add("of");
            tree.Add("the");
            tree.Add("lost");
            tree.Add("ark");

            tree.Add("rain");

            tree.Add("man");

            var prefixed = tree.StartsWith("ra").Select(w=>w.Value).ToList();

            Assert.That(prefixed.Count, Is.EqualTo(3));
            Assert.IsTrue(prefixed.Contains("rambo"));
            Assert.IsTrue(prefixed.Contains("raiders"));
            Assert.IsTrue(prefixed.Contains("rain"));
        }

        [Test]
        public void Can_find_exact()
        {
            var tree = new LcrsTrie('\0', false);
            Word word;
            Assert.False(tree.HasWord("xxx", out word));

            tree.Add("xxx");

            Assert.True(tree.HasWord("xxx", out word));
            Assert.False(tree.HasWord("baby", out word));
            Assert.False(tree.HasWord("dad", out word));

            tree.Add("baby");

            Assert.True(tree.HasWord("xxx", out word));
            Assert.True(tree.HasWord("baby", out word));
            Assert.False(tree.HasWord("dad", out word));

            tree.Add("dad");

            Assert.True(tree.HasWord("xxx", out word));
            Assert.True(tree.HasWord("baby", out word));
            Assert.True(tree.HasWord("dad", out word));
        }

        [Test]
        public void Can_build_one_leg()
        {
            var tree = new LcrsTrie('\0', false);
            Word word;
            tree.Add("baby");

            Assert.That(tree.LeftChild.Value, Is.EqualTo('b'));
            Assert.That(tree.LeftChild.LeftChild.Value, Is.EqualTo('a'));
            Assert.That(tree.LeftChild.LeftChild.LeftChild.Value, Is.EqualTo('b'));
            Assert.That(tree.LeftChild.LeftChild.LeftChild.LeftChild.Value, Is.EqualTo('y'));

            Assert.True(tree.HasWord("baby", out word));
        }

        [Test]
        public void Can_build_two_legs()
        {
            var root = new LcrsTrie('\0', false);
            root.Add("baby");
            root.Add("dad");
            Word word;
            Assert.That(root.LeftChild.RightSibling.Value, Is.EqualTo('d'));
            Assert.That(root.LeftChild.LeftChild.Value, Is.EqualTo('a'));
            Assert.That(root.LeftChild.RightSibling.LeftChild.LeftChild.Value, Is.EqualTo('d'));

            Assert.That(root.LeftChild.Value, Is.EqualTo('b'));
            Assert.That(root.LeftChild.RightSibling.LeftChild.Value, Is.EqualTo('a'));
            Assert.That(root.LeftChild.LeftChild.LeftChild.Value, Is.EqualTo('b'));
            Assert.That(root.LeftChild.LeftChild.LeftChild.LeftChild.Value, Is.EqualTo('y'));

            Assert.True(root.HasWord("baby", out word));
            Assert.True(root.HasWord("dad", out word));
        }

        [Test]
        public void Can_append()
        {
            var root = new LcrsTrie('\0', false);

            root.Add("baby");
            root.Add("bad");
            Word word;

            Assert.That(root.LeftChild.Value, Is.EqualTo('b'));
            Assert.That(root.LeftChild.LeftChild.Value, Is.EqualTo('a'));
            Assert.That(root.LeftChild.LeftChild.LeftChild.RightSibling.Value, Is.EqualTo('d'));

            Assert.That(root.LeftChild.LeftChild.LeftChild.Value, Is.EqualTo('b'));
            Assert.That(root.LeftChild.LeftChild.LeftChild.LeftChild.Value, Is.EqualTo('y'));

            Assert.True(root.HasWord("baby", out word));
            Assert.True(root.HasWord("bad", out word));
        }
    }
}