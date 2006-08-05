/*
 * Created by SharpDevelop.
 * User: bsiepert
 * Date: 8/4/2006
 * Time: 8:02 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace bugreport
{
	public abstract class TestFileParserFixture
	{
		protected StreamWriter writer;
		protected DumpFileParser parser;
		protected MemoryStream stream;
		
		public virtual void SetUp()
		{
			stream = new MemoryStream();
			writer = new StreamWriter(stream);
			parser = new DumpFileParser(stream);
		}
		
	}
	
	[TestFixture]
	public class WithNothingElseTests : TestFileParserFixture
	{
		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CurrentLineBeforeFirstBytes()
		{
			String none = parser.CurrentLine;
		}
		
		[Test] 
		public void EmptyLine()
		{
			writer.WriteLine(String.Empty);
			writer.Flush();
			stream.Position = 0;

			Assert.IsNull(parser.GetNextInstructionBytes());
		} 
	}

	[TestFixture]
	public class MainAfterNonMainTests : TestFileParserFixture
	{
		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			writer.WriteLine("0804837c <nomain>:");
			writer.WriteLine(" 8048398:	c9                   	leave  ");
			writer.WriteLine("");
			writer.WriteLine("0804837c <main>:");
		}
		
		[Test] 
		public void MainIsLastFunction()
		{
			writer.WriteLine(" 8048399:	c3                   	ret    ");
			writer.Flush();
			stream.Position = 0;

			Byte[] code = parser.GetNextInstructionBytes();
			Assert.AreEqual(0xc3, code[0]);
		} 

		[Test] 
		public void CurrentLine()
		{
			const String line = " 8048399:	c3                   	ret    ";
			writer.WriteLine(line);
			writer.Flush();
			stream.Position = 0;

			parser.GetNextInstructionBytes();
			Assert.AreEqual(line, parser.CurrentLine);
		} 

		[Test] 
		public void MainIsNotLastFunction()
		{
			writer.WriteLine(" 8048399:	c3                   	ret    ");
			writer.WriteLine("");
			writer.WriteLine("0804837c <nonmain2>:");
			writer.WriteLine(" 8048398:	90                   	nop  ");
			
			writer.Flush();
			stream.Position = 0;

			Byte[] code = parser.GetNextInstructionBytes();
			Assert.AreEqual(0xc3, code[0]);
			Assert.IsFalse(parser.EndOfFunction);
			
			Assert.IsNull(parser.GetNextInstructionBytes());
			Assert.IsTrue(parser.EndOfFunction);
		} 

	}

	[TestFixture]
	public class WithNoMainTests : TestFileParserFixture
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
			stream.Position = 0;

			Assert.IsNull(parser.GetNextInstructionBytes());
		}
	}

	[TestFixture]
	public class WithMainOnlyTests : TestFileParserFixture
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
			writer.WriteLine(" : ");
			writer.Flush();
			stream.Position = 0;

			Assert.IsNull(parser.GetNextInstructionBytes());
		}
		
		[Test]
		public void HasColonNothingElse()
		{
			writer.WriteLine(":");
			writer.Flush();
			stream.Position = 0;
			
			Assert.IsNull(parser.GetNextInstructionBytes());
		}
	

		[Test]
		public void LineWithSingleHex()
		{
			writer.WriteLine(" 804837c:       55                      push   ebp");
			writer.Flush();
			stream.Position = 0;

			Byte[] result = parser.GetNextInstructionBytes();
			Assert.AreEqual(0x55, result[0]);
		}
			
		[Test]
		public void LineWithMultipleHex()
		{
			writer.WriteLine(" 8048385:       83 ec 10                sub    esp,0x10");
			writer.Flush();
			stream.Position = 0;

			Byte[] expectedResult = new Byte[] {0x83, 0xec, 0x10};
			Byte[] result = parser.GetNextInstructionBytes();
			Assert.AreEqual(expectedResult, result);
		}

		[Test]
		public void LineWithMuchoHex()
		{
			writer.WriteLine(" 8048388:       c7 04 24 10 00 00 00    mov    DWORD PTR [esp],0x10");
			writer.Flush();
			stream.Position = 0;

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
			stream.Position = 0;

			Byte[] expectedResult = new Byte[] {0x83, 0xec, 0x10};
			Byte[] result = parser.GetNextInstructionBytes();
		}	
		
		[Test]
		public void NextLineSkipsBadLines()
		{
			writer.WriteLine("BADLINE");
			writer.WriteLine(" 8048385:       83 ec 10                sub    esp,0x10");
			writer.Flush();
			stream.Position = 0;
						
			Byte[] result = parser.GetNextInstructionBytes();
			Byte[] expectedResult = new Byte[] {0x83, 0xec, 0x10};

			Assert.AreEqual(expectedResult, result);
		}
		
		[Test]
		public void LineWithSpaceTab()
		{
			writer.WriteLine(" 8048388:       c7 04 24 10 00 00 00 \tmov    DWORD PTR [esp],0x10");
			writer.Flush();
			stream.Position = 0;

			Byte[] expectedResult = new Byte[] {0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};
			Byte[] result = parser.GetNextInstructionBytes();
			Assert.AreEqual(expectedResult, result);			
		}
	}
}
