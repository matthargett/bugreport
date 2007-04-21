// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert, 
// Cullen Bryan, Mike Seery
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;
using NUnit.Framework;
using NUnit.Mocks;
using System.IO;
using System.Collections.Generic;

namespace bugreport
{
	[TestFixture]
	public sealed class AnalyzerTests : IDisposable
	{
		Analyzer analyzer;
		MemoryStream stream = new MemoryStream(new Byte[] {0, 1, 2});
		Byte[] code = new Byte[] {0x90};
		
		private class FakeAnalyzer : Analyzer
		{
			Byte[] code = new Byte[] { 0x90 }; 
			private UInt32 actualReportItemCount;			
			
			public FakeAnalyzer(IParsable parser, UInt32 actualReportItemCount) : base (parser) 
			{
				this.actualReportItemCount = actualReportItemCount;
			}
			
			protected override MachineState runCode(MachineState _machineState, Byte[] _instructionBytes)
			{
				for (UInt32 i = 0; i < actualReportItemCount; i++)
				{
					reportItems.Add(new ReportItem(i, false));
				}
				
				_machineState.InstructionPointer += (UInt32)_instructionBytes.Length;
				
				return _machineState;
			}			
		}

		public void Dispose()
		{
			if(null != stream)
			{
				stream.Dispose();
			}
		}

		private IParsable createMockParser(UInt32 expectedReportItemCount)
		{				
			DynamicMock control = new DynamicMock(typeof(IParsable));
			control.ExpectAndReturn("get_EndOfFunction", false, null);
			control.ExpectAndReturn("GetNextInstructionBytes", code, null);
			control.ExpectAndReturn("get_EndOfFunction", true, null);
			
			List<ReportItem> reportItemList = new List<ReportItem>();
			
			for (UInt32 i = 0; i < expectedReportItemCount; i++)
			{
				reportItemList.Add(new ReportItem(i, false));
			}
			control.ExpectAndReturn("get_ExpectedReportItems", reportItemList.AsReadOnly(), null);
			control.ExpectAndReturn("get_ExpectedReportItems", reportItemList.AsReadOnly(), null);
			return control.MockInstance as IParsable;
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullStream() 
		{
			analyzer = new Analyzer(null);
			analyzer.Run();
		}	

		[Test]
		public void NoReportItems()
		{
			analyzer = new Analyzer(createMockParser(0));
			Assert.AreEqual(0, analyzer.ActualReportItems.Count);
			analyzer.Run();
			Assert.AreEqual(0, analyzer.ExpectedReportItems.Count);
		}
		
		[Test]
		public void WithReportItems()
		{
			List<ReportItem> reportItems = new List<ReportItem>();
			
			analyzer = new FakeAnalyzer(createMockParser(2), 2);
			analyzer.OnEmulationComplete += 
				delegate(object sender, EmulationEventArgs e)
				{
					Assert.AreEqual(code[0], e.Code[0]);
					Assert.AreEqual(0, e.MachineState.InstructionPointer);
				};
			
			analyzer.OnReport += 
				delegate(object sender, ReportEventArgs e) 
				{ 
					reportItems.Add(e.ReportItem); 
				};
			
			analyzer.Run();
			Assert.AreEqual(2, analyzer.ActualReportItems.Count);
			Assert.AreEqual(2, analyzer.ExpectedReportItems.Count);
			
			Assert.AreEqual(0, reportItems[0].InstructionPointer);
			Assert.AreEqual(1, reportItems[1].InstructionPointer);
		}
		
		[Test]
		public void VerifyExpectedAndActualReports()
		{				
			analyzer = new FakeAnalyzer(createMockParser(2), 2);
			analyzer.Run();			
			Assert.AreEqual(analyzer.ActualReportItems.Count, analyzer.ExpectedReportItems.Count);	
			
			analyzer = new FakeAnalyzer(createMockParser(2), 3);
			analyzer.Run();
			Assert.AreNotEqual(analyzer.ActualReportItems.Count, analyzer.ExpectedReportItems.Count);	
		}
	}
}
