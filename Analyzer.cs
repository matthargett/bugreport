// Copyright (c) 2006-2007 Luis Miras, Doug Coker, Todd Nagengast,
// Anthony Lineberry, Dan Moniz, Bryan Siepert, Mike Seery, Cullen Bryan
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace bugreport
{
public class EmulationEventArgs : EventArgs
{
    private MachineState state;
    private ReadOnlyCollection<Byte> code;

    public EmulationEventArgs(MachineState state, ReadOnlyCollection<Byte> code) : base()
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
    private ReportItem reportItem;

    public ReportEventArgs(ReportItem reportItem) : base()
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

        if (null != this.OnReport)
        {
            OnReport(this, new ReportEventArgs(item));
        }
    }
}

public class Analyzer
{
    public EventHandler<EmulationEventArgs> OnEmulationComplete;

    protected ReportCollection reportItems;
    private IParsable parser;
    private Opcode opcode = new X86Opcode();

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
        get
        {
            return new ReadOnlyCollection<ReportItem>(reportItems);
        }
    }

    public ReadOnlyCollection<ReportItem> ExpectedReportItems
    {
        get
        {
            return parser.ExpectedReportItems;
        }
    }

    public EventHandler<ReportEventArgs> OnReport
    {
        get { return reportItems.OnReport; }
        set { reportItems.OnReport = value; }
    }


    private static RegisterCollection getRegistersForLinuxMain()
    {
        RegisterCollection linuxMainDefaultValues = new RegisterCollection();

        AbstractValue arg0 = new AbstractValue(1).AddTaint();

        AbstractValue[] argvBuffer = new AbstractValue[] {arg0};
        AbstractValue argvPointer = new AbstractValue(argvBuffer);
        AbstractValue[] argvPointerBuffer = new AbstractValue[] {argvPointer};
        AbstractValue argvPointerPointer = new AbstractValue(argvPointerBuffer);
        AbstractValue[]  stackBuffer = AbstractValue.GetNewBuffer(0x200);

        AbstractBuffer buffer = new AbstractBuffer(stackBuffer);
        AbstractBuffer modifiedBuffer = buffer.DoOperation(OperatorEffect.Add, new AbstractValue(0x100));

        // linux ABI dictates
        modifiedBuffer[13] = argvPointerPointer;

        // gcc generates code that accesses this at some optimization levels
        modifiedBuffer[0xfc] = new AbstractValue(1);

        AbstractValue stackPointer = new AbstractValue(modifiedBuffer);
        linuxMainDefaultValues[RegisterName.ESP] = stackPointer;

        return linuxMainDefaultValues;
    }


    protected virtual MachineState runCode(MachineState _machineState, Byte [] _instructionBytes)
    {
        return X86emulator.Run(reportItems, _machineState, _instructionBytes);
    }

    public void Run()
    {
        MachineState machineState = new MachineState(getRegistersForLinuxMain());
        machineState.InstructionPointer = parser.EntryPointAddress;
        Byte[] instructions = parser.GetBytes();
        UInt32 index = machineState.InstructionPointer - parser.EntryPointAddress;
        
        while (index < instructions.Length)
        {
            MachineState savedState = machineState;
            Byte[] instruction = extractInstruction(instructions, index);
            machineState = runCode(machineState, instruction);
            if (null != this.OnEmulationComplete)
            {
                OnEmulationComplete(this, new EmulationEventArgs(savedState, new ReadOnlyCollection<Byte>(instruction)));
            }
            
            index = machineState.InstructionPointer - parser.EntryPointAddress;
        }
    }
    
    private Byte[] extractInstruction(Byte[] instructions, UInt32 index)
    {
        Byte instructionLength = opcode.GetInstructionLength(instructions, index);
        Byte[] instruction = new Byte[instructionLength];
        for (UInt32 count=index; count < index+instructionLength; count++)
        {
            instruction[count-index] = instructions[count];
        }
        
        return instruction;
    }
}
}
