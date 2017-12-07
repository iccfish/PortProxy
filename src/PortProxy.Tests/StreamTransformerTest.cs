using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PortProxy.Tests
{
    using System;
    using System.Diagnostics;
    using System.Security.Cryptography;

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestEncodeDecode()
        {
            var tick = new Random().Next(int.MaxValue);

            var s1 = new StreamTransformer();
            s1.Init(tick, 0);
            var buffer = new byte[128];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);

            var buffer1 = buffer.Clone() as byte[];
            s1.Encode(buffer1, 0, buffer.Length);

            var s2 = new StreamTransformer();
            s2.Init(tick, 0);
            s2.Decode(buffer1, 0, buffer.Length);

            for (int i = 0; i < buffer.Length; i++)
            {
                Debug.Assert(buffer[i] == buffer1[i], "Test failed.");
            }
        }
    }
}
