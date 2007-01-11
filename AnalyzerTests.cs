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
		
		private class FakeAnalyzer : Analyzer
		{
			Byte[] code = new Byte[] { 0x90 }; 
			ReportItem reportItem = new ReportItem(0, false);
			private UInt32 expectedReportItemCount;
			private UInt32 actualReportItemCount;			

			public FakeAnalyzer(Stream stream, UInt32 expectedReportItemCount, UInt32 actualReportItemCount) : base (stream) 
			{
				this.expectedReportItemCount = expectedReportItemCount;
				this.actualReportItemCount = actualReportItemCount;
			}
			
			protected override MachineState runCode(MachineState _machineState, byte[] _instructionBytes)
			{
				for (UInt32 i = 0; i < actualReportItemCount; i++)
				{
					reportItems.Add(reportItem);
				}
				
				return _machineState;
			}
			
			protected override IParsable createFileParser(Stream _stream)
			{				
				DynamicMock control= new DynamicMock(typeof(IParsable));
				control.ExpectAndReturn("get_EndOfFunction", false, null);
				control.ExpectAndReturn("GetNextInstructionBytes", code, null);
				control.ExpectAndReturn("get_EndOfFunction", true, null);
				
				List<ReportItem> reportItemList = new List<ReportItem>();
				
				for (UInt32 i = 0; i < expectedReportItemCount; i++)
				{
					reportItemList.Add(reportItem);
				}
				control.ExpectAndReturn("get_ExpectedReportItem", reportItemList, null);
				control.ExpectAndReturn("get_ExpectedReportItem", reportItemList, null);
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
			Assert.AreEqual(0, analyzer.ActualReportItems.Count);
		}

		private void onEmulation(MachineState state, Byte[] code)
		{
			Assert.AreEqual(0x90, code[0]);
		}	

		[Test]
		public void WithReportItems()
		{
			analyzer = new FakeAnalyzer(stream,1 ,1);
			analyzer.OnEmulationComplete += onEmulation;
			analyzer.Run();
			Assert.AreEqual(1, analyzer.ActualReportItems.Count);
		}
		
		[Test]
		public void VerifyExpectedAndActualReports()
		{				
			analyzer = new FakeAnalyzer(stream, 2, 2);
			analyzer.Run();			
			Assert.AreEqual(analyzer.ActualReportItems.Count, analyzer.ExpectedReportItems.Count);	
			
			analyzer = new FakeAnalyzer(stream, 2, 3);
			analyzer.Run();
			Assert.AreNotEqual(analyzer.ActualReportItems.Count, analyzer.ExpectedReportItems.Count);	
		}
	}
}
