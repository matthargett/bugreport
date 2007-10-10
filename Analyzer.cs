// Copyright (c) 2006-2007 Luis Miras, Doug Coker, Todd Nagengast,
// Anthony Lineberry, Dan Moniz, Bryan Siepert, Mike Seery, Cullen Bryan
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;
using System.Collections.ObjectModel;

namespace bugreport
{
public class EmulationEventArgs : EventArgs
{
    private readonly MachineState state;
    private readonly ReadOnlyCollection<Byte> code;

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
    public EventHandler<EmulationEventArgs> OnEmulationComplete;

    protected ReportCollection reportItems;
    private readonly IParsable parser;
    private readonly Opcode opcode = new X86Opcode();

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


    private static RegisterCollection getRegistersForLinuxStart()
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
        modifiedBuffer[5] = argvPointerPointer;

        // gcc generates code that accesses this at some optimization levels
        modifiedBuffer[0xfc] = new AbstractValue(1);

        AbstractValue stackPointer = new AbstractValue(modifiedBuffer);
        linuxMainDefaultValues[RegisterName.ESP] = stackPointer;

        return linuxMainDefaultValues;
    }


    protected virtual MachineState runCode(MachineState _machineState, Byte[] code)
    {
        return X86emulator.Run(reportItems, _machineState, code);
    }

    public void Run()
    {
        MachineState machineState = new MachineState(getRegistersForLinuxStart());
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
                OnEmulationComplete(this, new EmulationEventArgs(savedState, new ReadOnlyCollection<Byte>(instruction)));
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
        Byte[] instruction = new Byte[instructionLength];
        for (UInt32 count=index; count < index+instructionLength; count++)
        {
            instruction[count-index] = instructions[count];
        }
        
        return instruction;
    }
}
}
