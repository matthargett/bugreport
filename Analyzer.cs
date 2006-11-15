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
		const String VERSION = "0.1";
				
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

		internal DumpFileParser getParserForFilename(String _fileName)
		{
			FileStream file = null;
			
			try
			{
				file = File.OpenRead(_fileName);
			}
			
			catch(FileNotFoundException)
			{
				Console.WriteLine("File not found: " + _fileName);
				return null;				
			}
			
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
			X86emulator interpreter;
			
			String[] fileNames;
			
			if (_fileArgument.Contains("*"))
			{
				fileNames = Directory.GetFiles(Environment.CurrentDirectory, _fileArgument);
			}
			else
			{
				fileNames = new String[] { _fileArgument };
			}
			
			foreach(String fileName in fileNames)
			{							
				parser = getParserForFilename(fileName);
				if (null == parser) 
				{
					return;
				}
				
				interpreter = new X86emulator(getRegistersForLinuxMain());
				
				if (_isTracing)
				{
					Console.WriteLine();
					Console.WriteLine("Interpreting file: " + fileName);
				}

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
								Console.WriteLine("topOfStack=" + interpreter.TopOfStack + "  " + interpreter.Registers);
								Console.WriteLine(parser.CurrentLine);
							}
							
							try
							{
							    interpreter.Run(instructionBytes);
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
