// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.IO;
using NUnit.Framework;

namespace bugreport
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public abstract class ElfFileParserFixture
    {
        protected StreamWriter writer;
        protected ElfFileParser parser;
        protected MemoryStream stream;

        public virtual void SetUp()
        {
            stream = new MemoryStream();
            writer = new StreamWriter(stream);
        }

        [TearDown]
        public void TearDown()
        {
            if (parser != null)
            {
                parser.Dispose();
                parser = null;
            }
        }
    }

    [TestFixture]
    public class WellFormatted:ElfFileParserFixture
    {
        Byte [] textData;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            Byte[] elfHeader = new Byte[] {0x7f, 0x45, 0x4c, 0x46};
            textData = new Byte[] {0x90};
            stream.Write(elfHeader,0, elfHeader.Length);
            stream.Seek(0x2e0, SeekOrigin.Begin);
            stream.Write(textData, 0, textData.Length);
            parser = new ElfFileParser(stream);
        }

        [Test]
        public void GetBytes()
        {
            Assert.AreEqual(textData[0], parser.GetBytes()[0]);
        }
    }
}