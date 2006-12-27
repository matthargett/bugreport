// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
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
	}

	public class Analyzer
	{
		protected List<ReportItem> reportItems = new List<ReportItem>();
		private Stream stream;

		public Analyzer(Stream stream)
		{
			if (null == stream)
				throw new ArgumentNullException("stream");
			
			this.stream = stream;
		}

		public IList<ReportItem> ReportItems
		{
			get
			{
				return reportItems;
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

		
		protected virtual IParsable createDumpFileParser(Stream _stream)
		{
			return new DumpFileParser(_stream);
		}
		
		public void Run(Boolean _isTracing) 
		{
			IParsable parser = createDumpFileParser(stream);
			
			MachineState machineState = new MachineState(getRegistersForLinuxMain());

			while (!parser.EndOfFunction)
			{
				Byte[] instructionBytes = parser.GetNextInstructionBytes();
				
				try
				{
					// TODO: This may ignore the last instructions in the method.  Investigate + fix.
					if (!parser.EndOfFunction)						
					{
						if (_isTracing)
						{
							Console.WriteLine();
							Console.WriteLine("topOfStack=" + machineState.TopOfStack + "  " + machineState.Registers);
							Console.WriteLine(parser.CurrentLine);
						}
						
						machineState = runCode(machineState, instructionBytes);
					}
				}

				catch (Exception e)
				{
					StreamWriter writer = new StreamWriter(Console.OpenStandardError());
					writer.WriteLine(e.ToString());
					writer.Close();
					break;
				}
			}
		}
	}
}
