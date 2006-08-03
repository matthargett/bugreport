// Copyright (c) 2006 Luis Miras
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.IO;

namespace bugreport
{
	class MainClass
	{
		const String VERSION = "0.1";
		
		private static void onReportOOB(Object sender, NewReportEventArgs e)
		{
			String message = String.Empty;
			if (e.IsTainted)
				message += "Exploitable ";
			message += String.Format("OOB at EIP 0x{0:x4}", e.InstructionPointer);
			Console.WriteLine(message);
		}
		
		private static TestFileParser getParserForFilename(String _fileName)
		{
			FileStream file = null;
			
			try
			{
				file = File.OpenRead(_fileName);
			}
			
			catch(FileNotFoundException)
			{
				Console.WriteLine("File not found: " + _fileName);
				Environment.Exit(1);
			}
			
			catch(Exception e)
			{
				Console.WriteLine("Couldn't open file due to error:");
				Console.WriteLine(e.ToString());
				Environment.Exit(1);
			}
			
			return new TestFileParser(file);
		}
			
		
		private static RegisterCollection getRegistersForLinuxMain()
		{
				RegisterCollection linuxMainDefaultValues = new RegisterCollection();
					
				AbstractValue arg0 = new AbstractValue(1).AddTaint();
				
				AbstractValue[] argvBuffer = new AbstractValue[] {arg0};
				AbstractValue argvPointer = new AbstractValue(argvBuffer);
				AbstractValue[] argvPointerBuffer = new AbstractValue[] {argvPointer};
				AbstractValue argvPointerPointer = new AbstractValue(argvPointerBuffer);
				AbstractValue[]  stackBuffer = new AbstractValue[0x200];
				
					
				
				AbstractBuffer buffer = new AbstractBuffer(stackBuffer);
				AbstractBuffer modifiedBuffer = AbstractBuffer.Add(buffer, 0x100);
				modifiedBuffer[12] = argvPointerPointer;

				// HACK: gcc generates code that accesses this for no good reason
				modifiedBuffer[0xfc] = new AbstractValue(1); 
				
				AbstractValue stackPointer = new AbstractValue(modifiedBuffer);
				linuxMainDefaultValues[RegisterName.ESP] = stackPointer;
				
				return linuxMainDefaultValues;

		}

		public static void Main(string[] args)
		{
			Console.WriteLine("bugreport " + VERSION);
			TestFileParser parser;
			Interpreter interpreter;
			
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: bugreport.exe [--trace] file.test");
				return;	
			}
			
			String fileArgument = args[args.Length - 1];
			Boolean isTracing = false;
			
			if (args[0].Equals("--trace"))
				isTracing = true;
			
			String[] fileNames;
			
			if (fileArgument.Contains("*"))
			{
				fileNames = Directory.GetFiles(Environment.CurrentDirectory, fileArgument);
			}
			else
			{
				fileNames = new String[] { fileArgument };
			}
			
			foreach(String fileName in fileNames)
			{
				parser = getParserForFilename(fileName);
				
				interpreter = new Interpreter(getRegistersForLinuxMain());
				interpreter.NewReport += onReportOOB;
				
				if (isTracing)
				{
					Console.WriteLine();
					Console.WriteLine("Interpreting file: " + fileName);
				}

				
				while (!parser.EndOfFunction)
				{
					Byte[] instructionBytes = parser.GetNextInstructionBytes();
					
					try
					{
						if (!parser.EndOfFunction)						
						{
							if (isTracing)
							{
								Console.WriteLine();
								Console.WriteLine("topOfStack=" + interpreter.TopOfStack + "  " + interpreter.Registers);
								Console.WriteLine(parser.CurrentLine);
							}

							interpreter.Run(instructionBytes);
						}
					}
					catch (Exception e)
					{
						StreamWriter writer = new StreamWriter(Console.OpenStandardError());
						writer.WriteLine(e.ToString());
						
						writer.Close();
						Environment.Exit(-1);
					}
				}
			}
			
		}
	} 
}
