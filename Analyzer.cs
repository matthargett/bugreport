// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert, Cullen Bryan
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using System.IO;
using System.Collections.Generic;

namespace bugreport
{
	public struct ReportItem
	{
		public UInt32 InstructionPointer;
		public Boolean IsTainted;

		public ReportItem(UInt32 instructionPointer, Boolean isTainted)
		{
			this.InstructionPointer = instructionPointer;
			this.IsTainted = isTainted;
		}
		
		public override bool Equals(object obj)
		{
			ReportItem report = (ReportItem)obj;
			return this.InstructionPointer == report.InstructionPointer &&
				this.IsTainted == report.IsTainted;
		}
	
		public override int GetHashCode()
		{
			return this.InstructionPointer.GetHashCode() ^ this.IsTainted.GetHashCode();
		}
		
		public static bool operator== (ReportItem a, ReportItem b)
		{
			return a.Equals(b);
		}
		
		public static bool operator!= (ReportItem a, ReportItem b)
		{
			return !a.Equals(b);
		}
	}

	public class Analyzer
	{
		public delegate void EmulationComplete(MachineState state, Byte[] code);
		public event EmulationComplete OnEmulationComplete; 

		protected List<ReportItem> reportItems = new List<ReportItem>();
		private Stream stream;
		private IParsable parser;		

		public Analyzer(Stream stream)
		{
			if (null == stream)
				throw new ArgumentNullException("stream");
			
			this.stream = stream;			
		}

		public List<ReportItem> ActualReportItems
		{
			get
			{
				return reportItems;
			}
		}
		
		public List<ReportItem> ExpectedReportItems
		{
			get
			{
				return parser.ExpectedReportItem;
			}
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
			
			// linux ABI dictates **argv is ebp+12
			modifiedBuffer[12] = argvPointerPointer;

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

		
		protected virtual IParsable createFileParser(Stream _stream)
		{
			return new DumpFileParser(_stream);
		}

		public void Run() 
		{
			parser = createFileParser(stream);			
			
			MachineState machineState = new MachineState(getRegistersForLinuxMain());

			while (!parser.EndOfFunction)
			{
				Byte[] instructionBytes = parser.GetNextInstructionBytes();
				
				machineState = runCode(machineState, instructionBytes);
				if (null != this.OnEmulationComplete)
				{
					OnEmulationComplete(machineState, instructionBytes);
				}
			}
		}
	}
}
