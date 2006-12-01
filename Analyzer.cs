// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using System.IO;
using System.Collections.Generic;

namespace bugreport
{
	public class Analyzer
	{
		private List<String> collector = new List<String>();
		
		public string[] Messages 
		{
			get 
			{
				return collector.ToArray();
			}
		}

		private void onReportOOB(UInt32 _instructionPointer, Boolean _isTainted)
		{
			String message = String.Empty;
			if (_isTainted)
				message += "Exploitable ";
			message += String.Format("OOB at EIP 0x{0:x4}", _instructionPointer);
			Console.WriteLine("Found defect: " + message);
			collector.Add(message);
		}

		private DumpFileParser getParserForFilename(String _fileName)
		{
			FileStream file;
			
			file = File.OpenRead(_fileName);
			
			return new DumpFileParser(file);
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

		public void Analyze(String _fileArgument, Boolean _isTracing) 
		{
			DumpFileParser parser;
			String[] fileNames;

			if (null == _fileArgument)
				throw new ArgumentNullException("_fileArgument");
			
			if (_fileArgument.Contains("*"))
			{
				fileNames = Directory.GetFiles(Environment.CurrentDirectory, _fileArgument);
			}
			else
			{
				fileNames = new String[] { _fileArgument };
			}

			if (0 == fileNames.Length)
			{
				throw new FileNotFoundException("Wildcard doesn't match any files");
			}
			
			foreach(String fileName in fileNames)
			{							
				parser = getParserForFilename(fileName);
				if (null == parser) 
				{
					throw new ApplicationException("Couldn't create a parser for filename: " + fileName);
				}
				
				if (_isTracing)
				{
					Console.WriteLine();
					Console.WriteLine("Interpreting file: " + fileName);
				}

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
							
							try
							{
							    machineState = X86emulator.Run(machineState, instructionBytes);
							}
							catch (OutOfBoundsMemoryAccessException e)
							{
								onReportOOB(e.InstructionPointer, e.IsTainted);
							}
						}
					}
					catch (Exception e)
					{
						StreamWriter writer = new StreamWriter(Console.OpenStandardError());
						writer.WriteLine(e.ToString());
						collector.Add(e.ToString());
						writer.Close();
						break;
					}
				}
			}
		}
	}
}
