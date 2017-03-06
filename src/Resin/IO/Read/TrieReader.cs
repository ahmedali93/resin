using System;
using System.Collections.Generic;
using System.Linq;
using Resin.Analysis;

namespace Resin.IO.Read
{
    public abstract class TrieReader : ITrieReader
    {
        protected abstract LcrsNode Step();
        protected abstract void Skip(int count);

        protected LcrsNode LastRead;
        protected LcrsNode Replay;

        protected TrieReader()
        {
            LastRead = LcrsNode.MinValue;
            Replay = LcrsNode.MinValue;
        }

        private void Rewind()
        {
            Replay = LastRead;
        }

        public bool HasWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentException("path");

            LcrsNode node;
            if (TryFindDepthFirst(word, 0, out node))
            {
                return node.EndOfWord;
            }
            return false;
        }

        public IEnumerable<Word> StartsWith(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentException("path");

            var compressed = new List<Word>();
            LcrsNode node;

            if (TryFindDepthFirst(prefix, 0, out node))
            {
                DepthFirst(prefix, new List<char>(), compressed, prefix.Length - 1);
            }

            return compressed;
        }

        public IEnumerable<Word> Near(string word, int edits)
        {
            var compressed = new List<Word>();

            WithinEditDistanceDepthFirst(word, new string(new char[1]), compressed, 0, edits);

            return compressed.OrderBy(w => w.Distance);
        }

        private void WithinEditDistanceDepthFirst(string word, string state, IList<Word> compressed, int depth, int maxEdits, bool stop = false)
        {
            var reachedMin = maxEdits == 0 ? depth >= 0 : depth >= word.Length - 1 - maxEdits;
            var reachedMean = depth >= word.Length - 1;
            var reachedMax = depth >= word.Length + maxEdits;

            var node = Step();

            if (node == LcrsNode.MinValue) return;

            if (reachedMax || stop)
            {
                Skip(node.Weight - 1);
            }
            else
            {
                string test;

                if (depth == state.Length)
                {
                    test = state + node.Value;
                }
                else
                {
                    test = new string(state.ReplaceOrAppend(depth, node.Value).ToArray());
                }

                if (reachedMin)
                {
                    var edits = Levenshtein.Distance(word, test);

                    if (edits <= maxEdits)
                    {
                        if (node.EndOfWord)
                        {
                            compressed.Add(new Word(test) {Distance = edits});
                        }
                    }
                    else if(reachedMean)
                    {
                        stop = true;
                    }
                }

                // Go left (deep)
                if (node.HaveChild)
                {
                    WithinEditDistanceDepthFirst(word, test, compressed, depth + 1, maxEdits, stop);
                }

                // Go right (wide)
                if (node.HaveSibling)
                {
                    WithinEditDistanceDepthFirst(word, state, compressed, depth, maxEdits);
                }
            }
        }

        private void DepthFirst(string prefix, IList<char> path, IList<Word> compressed, int depth)
        {
            var node = Step();
            var siblings = new Stack<Tuple<int, IList<char>>>();

            // Go left (deep)
            while (node != LcrsNode.MinValue && node.Depth > depth)
            {
                var copyOfPath = new List<char>(path);

                path.Add(node.Value);

                if (node.EndOfWord)
                {
                    compressed.Add(new Word(prefix + new string(path.ToArray())));
                }

                if (node.HaveSibling)
                {
                    siblings.Push(new Tuple<int, IList<char>>(depth, copyOfPath));
                }

                depth = node.Depth;
                node = Step();
            }

            Rewind();

            // Go right (wide)
            foreach (var siblingState in siblings)
            {
                DepthFirst(prefix, siblingState.Item2, compressed, siblingState.Item1);
            }
        }

        private bool TryFindDepthFirst(string path, int currentDepth, out LcrsNode node)
        {
            node = Step();

            if (node != LcrsNode.MinValue && node.Depth != currentDepth)
            {
                Skip(node.Weight-1);
                node = Step();
            }

            if (node != LcrsNode.MinValue)
            {
                if (node.Value == path[currentDepth])
                {
                    if (currentDepth == path.Length - 1)
                    {
                        return true;
                    }
                    // Go left (deep)
                    return TryFindDepthFirst(path, currentDepth + 1, out node);
                }
                // Go right (wide)
                return TryFindDepthFirst(path, currentDepth, out node);
            }

            return false;
        }
    }
}