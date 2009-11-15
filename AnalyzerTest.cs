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

        private IParsable CreateMockParser(UInt32 expectedReportItemCount)
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

        private sealed class FakeAnalyzer : Analyzer
        {
            private readonly UInt32 actualReportItemCount;

            public FakeAnalyzer(IParsable parser, UInt32 actualReportItemCount)
                : base(parser)
            {
                this.actualReportItemCount = actualReportItemCount;
            }

            protected override MachineState RunCode(MachineState machineState, Byte[] instructionBytes)
            {
                for (UInt32 i = 0; i < actualReportItemCount; i++)
                {
                    ReportItems.Add(new ReportItem(i, false));
                }

                machineState.InstructionPointer += (UInt32)instructionBytes.Length;

                return machineState;
            }
        }

        [Test]
        public void NoReportItems()
        {
            analyzer = new Analyzer(CreateMockParser(0));
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
            analyzer = new FakeAnalyzer(CreateMockParser(2), 2);
            analyzer.Run();
            Assert.AreEqual(analyzer.ActualReportItems.Count, analyzer.ExpectedReportItems.Count);

            analyzer = new FakeAnalyzer(CreateMockParser(2), 3);
            analyzer.Run();
            Assert.AreNotEqual(analyzer.ActualReportItems.Count, analyzer.ExpectedReportItems.Count);
        }

        [Test]
        public void WithReportItems()
        {
            var reportItems = new List<ReportItem>();

            analyzer = new FakeAnalyzer(CreateMockParser(2), 2);
            analyzer.OnEmulationComplete +=
                delegate(object sender, EmulationEventArgs e)
                {
                    Assert.AreEqual(code[0], e.Code[0]);
                    Assert.AreEqual(0, e.MachineState.InstructionPointer);
                };

            analyzer.OnReport +=
                (sender, e) => reportItems.Add(e.ReportItem);

            analyzer.Run();
            Assert.AreEqual(2, analyzer.ActualReportItems.Count);
            Assert.AreEqual(2, analyzer.ExpectedReportItems.Count);

            Assert.AreEqual(0, reportItems[0].InstructionPointer);
            Assert.AreEqual(1, reportItems[1].InstructionPointer);
        }
    }
}