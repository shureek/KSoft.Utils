using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace UnitTest.IO
{
    [TestClass]
    public class StreamReaderTests
    {
        [TestMethod]
        public void TestReadLine()
        {
            Stream stream = new MockStream(length: 100);

            stream.ReadByte();
        }
    }
}
