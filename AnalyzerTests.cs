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
			analyzer = new Analyzer();
			analyzer.Analyze(null, false);
		}
	}
}
