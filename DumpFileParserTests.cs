// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert,
// Cullen Bryan, Mike Seery
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using System.IO;
using NUnit.Framework;

namespace bugreport.DumpFileParserTests
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
	public class WithNothingElseTests : DumpFileParserFixture
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

			Assert.IsNull(parser.GetNextInstructionBytes());
		}

		[Test]
		public void NoLines()
		{
			parser = new DumpFileParser(stream, "main");
			Assert.IsNull(parser.GetNextInstructionBytes());
		}
	}

	[TestFixture]
	public class MainAfterNonMainTests : DumpFileParserFixture
	{
		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			writer.WriteLine("0804837c <nomain>:");
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

			Byte[] code = parser.GetNextInstructionBytes();
			Assert.AreEqual(0xc3, code[0]);
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
			
			Assert.AreEqual(0x0804837d, parser.BaseAddress);

			Byte[] code = parser.GetNextInstructionBytes();
			Assert.AreEqual(0xc3, code[0]);
			Assert.IsTrue(parser.EndOfFunction);
			
			Assert.IsNull(parser.GetNextInstructionBytes());
			Assert.IsTrue(parser.EndOfFunction);
			Assert.AreEqual(0, parser.ExpectedReportItems.Count);
		}

		[Test]
		public void ParseNonMain()
		{
			writer.WriteLine(" 804837d:	c3                   	ret    ");
			writer.WriteLine();
			writer.Flush();
			parser = new DumpFileParser(stream, "nomain");
			
			Assert.AreEqual(0x0804837c, parser.BaseAddress);

			Byte[] code = parser.GetNextInstructionBytes();
			Assert.AreEqual(0xc9, code[0]);
			Assert.IsTrue(parser.EndOfFunction);
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
	}

	[TestFixture]
	public class WithNoMainTests : DumpFileParserFixture
	{
		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			writer.WriteLine("0804837c <nomain>:");
		}
		
		[Test]
		public void LineWithSingleHex()
		{
			writer.WriteLine(" 804837c:       55                      push   ebp");
			writer.Flush();
			parser = new DumpFileParser(stream, "main");

			Assert.IsNull(parser.GetNextInstructionBytes());
		}
	}

	[TestFixture]
	public class WithMainOnlyTests : DumpFileParserFixture
	{
		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			writer.WriteLine("0804837c <main>:");
		}
		
		[Test]
		public void HasColonButNoHex()
		{
			writer.WriteLine("        : ");
			writer.Flush();
			parser = new DumpFileParser(stream, "main");

			Assert.IsNull(parser.GetNextInstructionBytes());
		}
		
		[Test]
		public void HasColonInWrongPlace()
		{
			writer.WriteLine(":");
			writer.Flush();
			parser = new DumpFileParser(stream, "main");
			
			Assert.IsNull(parser.GetNextInstructionBytes());
		}
		

		[Test]
		public void LineWithSingleHex()
		{
			writer.WriteLine(" 804837c:       55                      push   ebp");
			writer.Flush();
			parser = new DumpFileParser(stream, "main");

			Byte[] result = parser.GetNextInstructionBytes();
			Assert.AreEqual(0x55, result[0]);
		}
		
		[Test]
		public void LineWithMultipleHex()
		{
			writer.WriteLine(" 8048385:       83 ec 10                sub    esp,0x10");
			writer.Flush();
			parser = new DumpFileParser(stream, "main");

			Byte[] expectedResult = new Byte[] {0x83, 0xec, 0x10};
			Byte[] result = parser.GetNextInstructionBytes();
			Assert.AreEqual(expectedResult, result);
		}

		[Test]
		public void LineWithMuchoHex()
		{
			writer.WriteLine(" 8048388:       c7 04 24 10 00 00 00    mov    DWORD PTR [esp],0x10");
			writer.Flush();
			parser = new DumpFileParser(stream, "main");

			Byte[] expectedResult = new Byte[] {0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};
			Byte[] result = parser.GetNextInstructionBytes();
			Assert.AreEqual(expectedResult, result);
		}

		[Test]
		[ExpectedException(typeof(FormatException))]
		public void LineWithBadHex()
		{
			writer.WriteLine(" 8048385:       83 ej 10                sub    esp,0x10");
			writer.Flush();
			parser = new DumpFileParser(stream, "main");

			parser.GetNextInstructionBytes();
		}
		
		[Test]
		public void NextLineSkipsBadLines()
		{
			writer.WriteLine("BADLINE");
			writer.WriteLine(" 8048385:       83 ec 10                sub    esp,0x10");
			writer.Flush();
			parser = new DumpFileParser(stream, "main");
			
			Byte[] result = parser.GetNextInstructionBytes();
			Byte[] expectedResult = new Byte[] {0x83, 0xec, 0x10};

			Assert.AreEqual(expectedResult, result);
		}
		
		[Test]
		public void LineWithSpaceTab()
		{
			writer.WriteLine(" 8048388:       c7 04 24 10 00 00 00 \tmov    DWORD PTR [esp],0x10");
			writer.Flush();
			parser = new DumpFileParser(stream, "main");

			Byte[] expectedResult = new Byte[] {0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};
			Byte[] result = parser.GetNextInstructionBytes();
			Assert.AreEqual(expectedResult, result);
		}
	}
}
