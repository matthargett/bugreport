// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using NUnit.Mocks;

namespace bugreport
{
    [TestFixture]
    public sealed class AnalyzerTest : IDisposable
    {
        private readonly MemoryStream stream = new MemoryStream(new Byte[] {0, 1, 2});
        private readonly Byte[] code = new Byte[] {0x90};
        private Analyzer analyzer;

        public void Dispose()
        {
            if (null != stream)
            {
                stream.Dispose();
            }
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullStream()
        {
            analyzer = new Analyzer(null);
            analyzer.Run();
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

        [Test]
        public void WithReportItems()
        {
            var reportItems = new List<ReportItem>();

            analyzer = new FakeAnalyzer(createMockParser(2), 2);
            analyzer.OnEmulationComplete +=
                delegate(object sender, EmulationEventArgs e)
                {
                    Assert.AreEqual(code[0], e.Code[0]);
                    Assert.AreEqual(0, e.MachineState.InstructionPointer);
                };

            analyzer.OnReport +=
                delegate(object sender, ReportEventArgs e) { reportItems.Add(e.ReportItem); };

            analyzer.Run();
            Assert.AreEqual(2, analyzer.ActualReportItems.Count);
            Assert.AreEqual(2, analyzer.ExpectedReportItems.Count);

            Assert.AreEqual(0, reportItems[0].InstructionPointer);
            Assert.AreEqual(1, reportItems[1].InstructionPointer);
        }
        
        private IParsable createMockParser(UInt32 expectedReportItemCount)
        {
            var control = new DynamicMock(typeof(IParsable));
            control.ExpectAndReturn("GetBytes", code, null);

            var reportItemList = new List<ReportItem>();

            for (UInt32 i = 0; i < expectedReportItemCount; i++)
            {
                reportItemList.Add(new ReportItem(i, false));
            }
            
            control.ExpectAndReturn("get_ExpectedReportItems", reportItemList.AsReadOnly(), null);
            control.ExpectAndReturn("get_ExpectedReportItems", reportItemList.AsReadOnly(), null);
            return control.MockInstance as IParsable;
        }
        
        private class FakeAnalyzer : Analyzer
        {
            private readonly UInt32 actualReportItemCount;

            public FakeAnalyzer(IParsable parser, UInt32 actualReportItemCount)
                : base(parser)
            {
                this.actualReportItemCount = actualReportItemCount;
            }

            protected override MachineState runCode(MachineState _machineState, Byte[] _instructionBytes)
            {
                for (UInt32 i = 0; i < actualReportItemCount; i++)
                {
                    ReportItems.Add(new ReportItem(i, false));
                }

                _machineState.InstructionPointer += (UInt32) _instructionBytes.Length;

                return _machineState;
            }
        }
    } 
}