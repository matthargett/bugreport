// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert,
// Cullen Bryan, Mike Seery
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace bugreport
{
	public static class MainClass
	{
		const String VERSION = "0.1";
		static Analyzer analyzer;

		public static void Main(String[] arguments)
		{
			Console.WriteLine("bugreport " + VERSION);

			if (arguments.Length < 1)
			{
				printUsage();
				return;
			}

			try 
			{
				Options.ParseArguments(arguments);
			}
			catch(ArgumentException e)
			{
				Console.WriteLine(e.Message);
				printUsage();
				Environment.Exit(-1);
			}
			
			if (0 == Options.Filenames.Count)
			{
				Console.WriteLine("No files found by name specified");
				Environment.Exit(-1);
			}

			analyzeFiles(Options.Filenames, Options.IsTracing);	
			
			if (analyzer.ExpectedReportItems.Count != 0 && (analyzer.ExpectedReportItems.Count != analyzer.ActualReportItems.Count) )
			{
				Console.WriteLine("Expectations Were Not Met::");
				Console.WriteLine("Expected: " + analyzer.ExpectedReportItems.Count + " Actual: " + analyzer.ActualReportItems.Count);
				Environment.Exit(-1);
			}
		}

		private static void analyzeFiles(ICollection<String> _fileNames, Boolean _isTracing)
		{
			foreach(String fileName in _fileNames)
			{
				Console.WriteLine();
				Console.WriteLine("Interpreting file: " + fileName);
				FileStream fileStream;

				try
				{
					fileStream = File.OpenRead(fileName);
				}
				catch (IOException e)
				{
					Console.WriteLine(e.Message);
					Environment.Exit(-1);
					return; // needed to hush up a false warning in mono
				}

				analyzer = new Analyzer(fileStream);

				analyzer.OnReport += printReportItem;
				
				if (_isTracing)
				{
					analyzer.OnEmulationComplete += printInfo;
				}

				analyzer.Run();
			}
		}
	
		private static void printReportItem(object sender, ReportEventArgs e)
		{
			ReportItem item = e.ReportItem;
			
			String message = "***";
			if (item.IsTainted)
				message += "Exploitable ";
			message += String.Format("OOB at EIP 0x{0:x4}", item.InstructionPointer);
			Console.WriteLine(message);
			Console.WriteLine();
		}
		
		public static void printInfo(object sender, EmulationEventArgs e)
		{
			Byte[] code = new Byte[e.Code.Count];
			e.Code.CopyTo(code, 0);
			MachineState state = e.MachineState;
			String address = String.Format("{0:x8}", state.InstructionPointer);
			Console.Write(address + ":");
			Console.Write("\t");

			foreach (Byte codeByte in code)
			{
				Console.Write(String.Format("{0:x2}", codeByte) + " ");
			}

			// magic numbers that happen to look good :)
			Int32 numberOfTabs = 3 - code.Length / 3;
			for (Int32 i = 0; i < numberOfTabs; i++)
				Console.Write("\t");

			Console.Write(OpcodeFormatter.GetInstructionName(code));
			Console.Write("\t");

			String operands = OpcodeFormatter.GetOperands(code);
			Console.Write(operands);
			if (operands.Length < 8)
				Console.Write("\t");

			String encoding = OpcodeFormatter.GetEncoding(code);
			Console.Write("\t");
			Console.WriteLine(encoding);

			Console.WriteLine(state.Registers);
			Console.WriteLine();
		}
		
		private static void printUsage()
		{
			Console.WriteLine("Usage: bugreport.exe [--trace] [--function=funcname] file.test");
		}
	}
}
