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
    public abstract class DumpFileParserFixture
    {
        protected StreamWriter writer;
        protected DumpFileParser parser;
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
    public class WithNothingElseTest : DumpFileParserFixture
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }


        [Test]
        public void EmptyLine()
        {
            writer.WriteLine(String.Empty);
            writer.Flush();
            parser = new DumpFileParser(stream, "main");

            Assert.IsNull(parser.GetBytes());
        }

        [Test]
        public void NoLines()
        {
            parser = new DumpFileParser(stream, "main");
            Assert.IsNull(parser.GetBytes());
        }
    }

    [TestFixture]
    public class MainAfterNonMainTest : DumpFileParserFixture
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            writer.WriteLine("0804837c <_start>:");
            writer.WriteLine(" 804837c:	c9                   	leave  ");
            writer.WriteLine();
            writer.WriteLine("0804837d <main>:");
        }

        [Test]
        public void MainIsLastFunction()
        {
            writer.WriteLine(" 804837d:	c3                   	ret    ");
            writer.Flush();
            parser = new DumpFileParser(stream, "main");

            Assert.AreEqual(0x0804837c, parser.BaseAddress);
            Assert.AreEqual(0x0804837d, parser.EntryPointAddress);

            Byte[] code = parser.GetBytes();
            CollectionAssert.AreEqual(new Byte[] {0xc9, 0xc3}, code);
            Assert.AreEqual(2, code.Length);
            Assert.AreEqual(0, parser.ExpectedReportItems.Count);
        }

        [Test]
        public void MainIsNotLastFunction()
        {
            writer.WriteLine(" 804837d:	c3                   	ret    ");
            writer.WriteLine();
            writer.WriteLine("0804837e <nonmain2>:");
            writer.WriteLine(" 804837e:	90                   	nop  ");
            writer.Flush();
            parser = new DumpFileParser(stream, "main");

            Assert.AreEqual(0x0804837c, parser.BaseAddress);
            Assert.AreEqual(0x0804837d, parser.EntryPointAddress);

            Byte[] code = parser.GetBytes();
            CollectionAssert.AreEqual(new Byte[] {0xc9, 0xc3, 0x90}, code);
            Assert.AreEqual(3, code.Length);        
            Assert.AreEqual(0, parser.ExpectedReportItems.Count);
        }

        [Test]
        public void ParseNonMain()
        {
            writer.WriteLine(" 804837d:	c3                   	ret    ");
            writer.WriteLine();
            writer.Flush();
            parser = new DumpFileParser(stream, "_start");

            Assert.AreEqual(0x0804837c, parser.BaseAddress);
            Assert.AreEqual(0x0804837c, parser.EntryPointAddress);

            Byte[] code = parser.GetBytes();
            Assert.AreEqual(0xc9, code[0]);        
        }

        [Test]
        public void WithExpectedReportItmes()
        {
            writer.WriteLine(" //<OutOfBoundsMemoryAccess Location=0x8000ffff Exploitable=True/>");
            writer.WriteLine(" //<OutOfBoundsMemoryAccess Location=0x8000FFFA Exploitable=False/>");
            writer.Flush();
            parser = new DumpFileParser (stream, "main");
            Assert.AreEqual(2, parser.ExpectedReportItems.Count);

            Assert.AreEqual(0x8000ffff, parser.ExpectedReportItems[0].InstructionPointer);
            Assert.AreEqual(true, parser.ExpectedReportItems[0].IsTainted);

            Assert.AreEqual(0x8000FFFA, parser.ExpectedReportItems[1].InstructionPointer);
            Assert.AreEqual(false, parser.ExpectedReportItems[1].IsTainted);
        }

        [Test]
        public void GetBytesReturnAllInstructions()
        {
            writer.WriteLine(" 804837d:	c3                   	ret    ");
            writer.WriteLine(" 804837e:	90                   	nop    ");
            writer.Flush();
            parser = new DumpFileParser(stream, "main");

            Byte[] code = parser.GetBytes();
            CollectionAssert.AreEqual(new Byte[] {0xc9, 0xc3, 0x90}, code);
        }

        [Test]
        public void HasColonButNoHex()
        {
            writer.WriteLine("        : ");
            writer.Flush();
            parser = new DumpFileParser(stream, "main");
            CollectionAssert.AreEqual(new Byte[] {0xc9}, parser.GetBytes());
        }

        [Test]
        public void HasColonInWrongPlace()
        {
            writer.WriteLine(":");
            writer.Flush();
            parser = new DumpFileParser(stream, "main");
            CollectionAssert.AreEqual(new Byte[] {0xc9}, parser.GetBytes());
        }


        [Test]
        public void LineWithSingleHex()
        {
            writer.WriteLine(" 804837c:       55                      push   ebp");
            writer.Flush();
            parser = new DumpFileParser(stream, "main");

            Byte[] expectedResult = new Byte[] {0xc9, 0x55};
            CollectionAssert.AreEqual(expectedResult, parser.GetBytes());
        }

        [Test]
        public void LineWithMultipleHex()
        {
            writer.WriteLine(" 8048385:       83 ec 10                sub    esp,0x10");
            writer.Flush();
            parser = new DumpFileParser(stream, "main");

            Byte[] expectedResult = new Byte[] {0xc9, 0x83, 0xec, 0x10};        
            CollectionAssert.AreEqual(expectedResult, parser.GetBytes());
        }

        [Test]
        public void LineWithMuchoHex()
        {
            writer.WriteLine(" 8048388:       c7 04 24 10 00 00 00    mov    DWORD PTR [esp],0x10");
            writer.Flush();
            parser = new DumpFileParser(stream, "main");

            Byte[] expectedResult = new Byte[] {0xc9, 0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};        
            CollectionAssert.AreEqual(expectedResult, parser.GetBytes());
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void LineWithBadHex()
        {
            writer.WriteLine(" 8048385:       83 ej 10                sub    esp,0x10");
            writer.Flush();
            parser = new DumpFileParser(stream, "main");        
        }

        [Test]
        public void NextLineSkipsBadLines()
        {
            writer.WriteLine("BADLINE");
            writer.WriteLine(" 8048385:       83 ec 10                sub    esp,0x10");
            writer.Flush();
            parser = new DumpFileParser(stream, "main");

            Byte[] expectedResult = new Byte[] {0xc9, 0x83, 0xec, 0x10};        
            CollectionAssert.AreEqual(expectedResult, parser.GetBytes());
        }

        [Test]
        public void LineWithSpaceTab()
        {
            writer.WriteLine(" 8048388:       c7 04 24 10 00 00 00 \tmov    DWORD PTR [esp],0x10");
            writer.Flush();
            parser = new DumpFileParser(stream, "main");

            Byte[] expectedResult = new Byte[] {0xc9, 0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};        
            CollectionAssert.AreEqual(expectedResult, parser.GetBytes());
        }
    }

    [TestFixture]
    public class WithNoStartTest : DumpFileParserFixture
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            writer.WriteLine("0804837f <nostart>:");
        }

        [Test]
        public void GetBytesIsNull()
        {
            writer.WriteLine(" 804837f:       55                      push   ebp");
            writer.Flush();
            parser = new DumpFileParser(stream, "main");

            Assert.IsNull(parser.GetBytes());
            Assert.AreEqual(0, parser.BaseAddress);
            Assert.AreEqual(0, parser.EntryPointAddress);
        }
    }
}