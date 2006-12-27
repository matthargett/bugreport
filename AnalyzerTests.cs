// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;
using NUnit.Mocks;
using System.IO;
using System.Collections.Generic;

namespace bugreport
{
	[TestFixture]
	public class AnalyzerTests
	{
		Analyzer analyzer;
		
		public class AnalyzerWithReports : Analyzer
		{
			public AnalyzerWithReports(Stream stream) : base (stream) {}
			
			protected override MachineState runCode(MachineState _machineState, byte[] _instructionBytes)
			{
				reportItems.Add(new ReportItem(0, false));
				return _machineState;
			}
			
			protected override IParsable createDumpFileParser(Stream _stream)
			{
				DynamicMock control= new DynamicMock(typeof(IParsable));
				control.ExpectAndReturn("get_EndOfFunction", false, null);
				control.ExpectAndReturn("get_EndOfFunction", false, null);
				control.ExpectAndReturn("get_EndOfFunction", false, null);
				return control.MockInstance as IParsable;
			}
		}
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
			analyzer.Run(false);
		}	

		[Test]
		public void NoReportItems()
		{
			MemoryStream stream = new MemoryStream(new Byte[] {0, 1, 2});
			analyzer = new Analyzer(stream);	
			Assert.AreEqual(0, analyzer.ReportItems.Count);
		}

		[Test]
		public void WithReportItems()
		{
			MemoryStream stream = new MemoryStream(new Byte[] {0, 1, 2});
			analyzer = new AnalyzerWithReports(stream);
			analyzer.Run(false);
			Assert.AreEqual(1, analyzer.ReportItems.Count);
		}
	}
}
