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
		Byte[] code = new Byte[] { 0x90 }; 
		MemoryStream stream = new MemoryStream(new Byte[] {0, 1, 2});
		
		public class AnalyzerWithReports : Analyzer
		{
			Byte[] code = new Byte[] { 0x90 }; 

			public AnalyzerWithReports(Stream stream) : base (stream) {}
			
			protected override MachineState runCode(MachineState _machineState, byte[] _instructionBytes)
			{
				reportItems.Add(new ReportItem(0, false));
				return _machineState;
			}
			
			protected override IParsable createFileParser(Stream _stream)
			{
				DynamicMock control= new DynamicMock(typeof(IParsable));
				control.ExpectAndReturn("get_EndOfFunction", false, null);
				control.ExpectAndReturn("GetNextInstructionBytes", code, null);
				control.ExpectAndReturn("get_EndOfFunction", true, null);
				return control.MockInstance as IParsable;
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullFileName() 
		{
			analyzer = new Analyzer(null);
			analyzer.Run();
		}	

		[Test]
		public void NoReportItems()
		{
			analyzer = new Analyzer(stream);
			Assert.AreEqual(0, analyzer.ReportItems.Count);
		}

		private void onEmulation(MachineState state, Byte[] code)
		{
			Assert.AreEqual(0x90, code[0]);
		}	

		[Test]
		public void WithReportItems()
		{
			analyzer = new AnalyzerWithReports(stream);
			analyzer.OnEmulationComplete += onEmulation;
			analyzer.Run();
			Assert.AreEqual(1, analyzer.ReportItems.Count);
		}
	}
}
