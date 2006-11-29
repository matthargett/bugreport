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
	//[Ignore("long")]
	public class AnalyzerTests
	{
		// TODO: This assumes that the test runner is run from the build directory.
		private String testRoot = 
			Directory.GetCurrentDirectory() + @"/../../tests/simple/heap/";			

		private String testDataFile = Directory.GetCurrentDirectory() + @"/../../systemTestsList.txt";

		[Test]
		public void GetParserFileDoesNotExist() 
		{
			Analyzer analyzer = new Analyzer();
			Assert.IsNull(analyzer.getParserForFilename("this file does not exist"));
		}

		[Test]
		public void GetParserFileExists() 
		{
			Analyzer analyzer = new Analyzer();
			Assert.IsNotNull(analyzer.getParserForFilename(testDataFile));
		}
	}
}
