using System;
using System.IO;
using log4net;

namespace Resin.IO.Read
{
    public class MappedTrieReader : TrieReader, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MappedTrieReader));

        private readonly Stream _stream;
        private readonly int _blockSize;

        public MappedTrieReader(string fileName)
        {
            _stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096*1, FileOptions.SequentialScan);
            _blockSize = Serializer.SizeOfNode();

            Log.DebugFormat("opened {0}", fileName);
        }

        protected override void Skip(int count)
        {
            if (count > 0)
            {
                _stream.Seek(_blockSize * count, SeekOrigin.Current);
            }
        }

        protected override LcrsNode Step()
        {
            if (Replay != LcrsNode.MinValue)
            {
                var replayed = Replay;
                Replay = LcrsNode.MinValue;
                return replayed;
            }

            var node = Serializer.DeserializeNode(_stream);

            LastRead = node;

            return LastRead;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}