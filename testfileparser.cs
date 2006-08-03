// Copyright (c) 2006 Luis Miras
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using NUnit.Framework;

namespace bugreport
{


	public class TestFileParser
	{
		private Stream stream;
		private StreamReader reader;
		private Boolean inMain;
		private String currentLine;

		public TestFileParser(Stream _stream)
		{
			stream = _stream;
			stream.Position = 0;
			reader = new StreamReader(stream);
			inMain = false;
		}
		
		public String CurrentLine
		{
			get 
			{ 
				if (currentLine == null)
				{
					throw new InvalidOperationException("Call GetNextInstructionBytes before accessing CurrentLine");
				}
				return currentLine; 
			}
		}
			
		public Byte[] GetNextInstructionBytes()
		{
			Byte[] hexBytes = null;
			
			while (null == hexBytes && !reader.EndOfStream)
			{
				currentLine = reader.ReadLine();
				if (isInMain(currentLine))
					hexBytes = getHexFromString(currentLine);
				else
					hexBytes = null;
			} 
			
			return hexBytes;
		}
		
		public Boolean EndOfFunction
		{
			get
			{
				return reader.EndOfStream;
			}
		}
		
		private Boolean isInMain(String line)
		{
			if (line == null)
				return inMain;
			
			if (line.Length > 0 && line[0] >= '0' && line[0] <= '7')
			{
				if (line.Contains("<main>:"))
					inMain = true;
				else 
					inMain = false;
			}
			
			return inMain;	
		}
		
		private String getHexWithSpaces(String line)
		{
			Int32 colonIndex = line.IndexOf(':');
			
			if (colonIndex == -1)
				return null;
			else if (colonIndex != 8)
				return null;
			
			String afterColonToEnd = line.Substring(colonIndex+1).Trim();
		    Int32 doubleSpaceIndex = afterColonToEnd.IndexOf("  ");
		    Int32 spaceTabIndex = afterColonToEnd.IndexOf(" \t");
		    Int32 endOfHexIndex;
		    
			if ( doubleSpaceIndex >= 0 && spaceTabIndex >= 0)
				endOfHexIndex = doubleSpaceIndex < spaceTabIndex ? doubleSpaceIndex : spaceTabIndex;
			else
				endOfHexIndex = doubleSpaceIndex > spaceTabIndex ? doubleSpaceIndex : spaceTabIndex;
				
			if (endOfHexIndex == -1)
				throw new ArgumentException("line", "Line doesn't contain hexvalues ");
				
			String hexString = afterColonToEnd.Substring(0, endOfHexIndex).Trim();

			return hexString;
		}

		private Byte[] getByteArrayFromHexString(String hex)
		{
			String[] hexStrings = hex.Split(new Char[] {' '});		
			
			Byte[] hexBytes = new Byte[hexStrings.Length];
		
			for (Int32 i = 0; i < hexStrings.Length; ++i)
			{
				hexBytes[i] = Byte.Parse(hexStrings[i], NumberStyles.HexNumber);
			}

			return hexBytes;
		}
	
		private Byte[] getHexFromString(String line)
		{
			
			if (line.Trim().Length == 0)
				return null;

			String hex = getHexWithSpaces(line);
			
			if (null == hex)
				return null;
			
			Byte[] hexBytes = getByteArrayFromHexString(hex);	
			
			return hexBytes;
		}

	}
	
	public abstract class TestFileParserFixture
	{
		protected StreamWriter writer;
		protected TestFileParser parser;
		protected MemoryStream stream;
		
		public virtual void SetUp()
		{
			stream = new MemoryStream();
			writer = new StreamWriter(stream);
			parser = new TestFileParser(stream);
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
