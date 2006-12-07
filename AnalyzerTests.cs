// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace bugreport
{
	[TestFixture]
	public class AnalyzerTests
	{
		Analyzer analyzer;
/*
		[Test]
		[ExpectedException(typeof(FileNotFoundException))]
		public void FileDoesNotExist() 
		{
			analyzer = new Analyzer();
			analyzer.Analyze("this file does not exist", false);
		}

		[Test]
		[ExpectedException(typeof(FileNotFoundException))]
		public void NoMatchWildcardDoesNotExist() 
		{
			analyzer = new Analyzer();
			analyzer.Analyze("thisfiledoesnotexist*", false);
		}
*/
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullFileName() 
		{
			analyzer = new Analyzer(null);
			analyzer.Analyze(false);
		}	

		[Test]
		public void NoReportItems()
		{
			MemoryStream stream = new MemoryStream(new Byte[] {0, 1, 2});
			analyzer = new Analyzer(stream);	
			Assert.AreEqual(0, analyzer.ReportItems.Count);
		}

		[Test]
		[Ignore("TODO")]
		public void WithReportItems()
		{
			MemoryStream stream = new MemoryStream(new Byte[] {0, 1, 2});
			// TODO: need to inject a mock emulator that will cause reportItems to generate
			analyzer = new Analyzer(stream);
			analyzer.Analyze(false);
			Assert.AreEqual(1, analyzer.ReportItems.Count);
		}
	}
}
