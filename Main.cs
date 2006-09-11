// Copyright (c) 2006 Luis Miras
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.IO;

namespace bugreport
{
	public class MainClass
	{
		const String VERSION = "0.1";
				
		private List<String> collector = new List<String>();
		
		public string[] Messages {
			get {
				return collector.ToArray();
			}
		}
		
		private void onReportOOB(Object sender, NewReportEventArgs e)
		{
			String message = String.Empty;
			if (e.IsTainted)
				message += "Exploitable ";
			message += String.Format("OOB at EIP 0x{0:x4}", e.InstructionPointer);
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
				AbstractValue[]  stackBuffer = new AbstractValue[0x200];
								
				AbstractBuffer buffer = new AbstractBuffer(stackBuffer);
				AbstractBuffer modifiedBuffer = AbstractBuffer.Add(buffer, 0x100);
				
				// linux ABI dictates **argv is ebp+12
				modifiedBuffer[12] = argvPointerPointer;

				// gcc generates code that accesses this at some optimization levels
				modifiedBuffer[0xfc] = new AbstractValue(1); 
				
				AbstractValue stackPointer = new AbstractValue(modifiedBuffer);
				linuxMainDefaultValues[RegisterName.ESP] = stackPointer;
				
				return linuxMainDefaultValues;
		}

		public static void Main(string[] args)
		{			
			MainClass mc = new MainClass();
			mc.Analyze(args);
		}
		
		public void Analyze(string[] args) {
			Console.WriteLine("bugreport " + VERSION);
			DumpFileParser parser;
			X86emulator interpreter;
			
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: bugreport.exe [--trace] file.test");
				return;	
			}
			
			String fileArgument = args[args.Length - 1];
			Boolean isTracing = false;
			
			if (args[0].Equals("--trace")) 
			{
				isTracing = true;
			}		
			
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
				if (null == parser) {
					return;
				}
				
				interpreter = new X86emulator(getRegistersForLinuxMain());
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
						// TODO: This may ignore the last instructions in the method.  Investigate + fix.
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
						collector.Add(e.ToString());
						writer.Close();
						break;
					}
				}
			}
		}
	} 
}
