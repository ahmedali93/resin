﻿using NUnit.Framework;
using Resin;

namespace Tests
{
    [TestFixture]
    public class FieldReaderTests
    {
        [Test]
        public void Can_read_field_file()
        {
            var fileName = Setup.Dir + "\\Can_read_field_file\\0.fld";
            using (var fw = new FieldWriter(fileName))
            {
                fw.Write("0", "hello", 5);
                fw.Write("5", "hello", 99);
            }
            var reader = FieldReader.Load(fileName);
            Assert.AreEqual(5, reader.GetPostings("hello")["0"]);
            Assert.AreEqual(99, reader.GetPostings("hello")["5"]);
        }
    }
}
