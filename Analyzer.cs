// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Collections.ObjectModel;

namespace bugreport
{
    public class EmulationEventArgs : EventArgs
    {
        private readonly ReadOnlyCollection<Byte> code;
        private readonly MachineState state;

        public EmulationEventArgs(MachineState state, ReadOnlyCollection<Byte> code)
        {
            this.state = state;
            this.code = code;
        }

        public MachineState MachineState
        {
            get { return state; }
        }

        public ReadOnlyCollection<Byte> Code
        {
            get { return code; }
        }
    }

    public class ReportEventArgs : EventArgs
    {
        private readonly ReportItem reportItem;

        public ReportEventArgs(ReportItem reportItem)
        {
            this.reportItem = reportItem;
        }

        public ReportItem ReportItem
        {
            get { return reportItem; }
        }
    }

    public class ReportCollection : Collection<ReportItem>
    {
        public EventHandler<ReportEventArgs> OnReport;

        protected override void InsertItem(int index, ReportItem item)
        {
            base.InsertItem(index, item);

            if (null != OnReport)
            {
                OnReport(this, new ReportEventArgs(item));
            }
        }
    }

    public class Analyzer
    {
        private readonly Opcode opcode = new X86Opcode();
        private readonly IParsable parser;
        public EventHandler<EmulationEventArgs> OnEmulationComplete;

        protected ReportCollection reportItems;

        public Analyzer(IParsable parser)
        {
            if (null == parser)
            {
                throw new ArgumentNullException("parser");
            }

            this.parser = parser;

            reportItems = new ReportCollection();
        }

        public ReadOnlyCollection<ReportItem> ActualReportItems
        {
            get { return new ReadOnlyCollection<ReportItem>(reportItems); }
        }

        public ReadOnlyCollection<ReportItem> ExpectedReportItems
        {
            get { return parser.ExpectedReportItems; }
        }

        public EventHandler<ReportEventArgs> OnReport
        {
            get { return reportItems.OnReport; }
            set { reportItems.OnReport = value; }
        }


        private static RegisterCollection getRegistersForLinuxStart()
        {
            var linuxMainDefaultValues = new RegisterCollection();

            AbstractValue arg0 = new AbstractValue(1).AddTaint();

            var argvBuffer = new[] {arg0};
            var argvPointer = new AbstractValue(argvBuffer);
            var argvPointerBuffer = new[] {argvPointer};
            var argvPointerPointer = new AbstractValue(argvPointerBuffer);
            AbstractValue[] stackBuffer = AbstractValue.GetNewBuffer(0x200);

            var buffer = new AbstractBuffer(stackBuffer);
            AbstractBuffer modifiedBuffer = buffer.DoOperation(OperatorEffect.Add, new AbstractValue(0x100));

            // linux ABI dictates
            modifiedBuffer[5] = argvPointerPointer;

            // gcc generates code that accesses this at some optimization levels
            modifiedBuffer[0xfc] = new AbstractValue(1);

            var stackPointer = new AbstractValue(modifiedBuffer);
            linuxMainDefaultValues[RegisterName.ESP] = stackPointer;

            return linuxMainDefaultValues;
        }


        protected virtual MachineState runCode(MachineState _machineState, Byte[] code)
        {
            return X86Emulator.Run(reportItems, _machineState, code);
        }

        public void Run()
        {
            var machineState = new MachineState(getRegistersForLinuxStart());
            machineState.InstructionPointer = parser.EntryPointAddress;
            Byte[] instructions = parser.GetBytes();
            UInt32 index = machineState.InstructionPointer - parser.BaseAddress;

            while (index < instructions.Length)
            {
                MachineState savedState = machineState;
                Byte[] instruction = extractInstruction(instructions, index);

                machineState = runCode(machineState, instruction);
                if (null != OnEmulationComplete)
                {
                    OnEmulationComplete(
                        this, new EmulationEventArgs(savedState, new ReadOnlyCollection<Byte>(instruction)));
                }

                index = machineState.InstructionPointer - parser.BaseAddress;

                if (opcode.TerminatesFunction(instruction))
                {
                    break;
                }
            }
        }

        private Byte[] extractInstruction(Byte[] instructions, UInt32 index)
        {
            Byte instructionLength = opcode.GetInstructionLength(instructions, index);
            var instruction = new Byte[instructionLength];
            for (UInt32 count = index; count < index + instructionLength; count++)
            {
                instruction[count - index] = instructions[count];
            }

            return instruction;
        }
    }
}