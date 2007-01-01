// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
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
		static Analyzer analyzer;

		public static void Main(String[] args)
		{
			Console.WriteLine("bugreport " + VERSION);

			if (args.Length < 1)
			{
				Console.WriteLine("Usage: bugreport.exe [--trace] file.test");
				return;	
			}
			
			Boolean isTracing = getTracingOptionFromArguments(args);
			String[] fileNames = getFileNamesFromArguments(args);	
			if (0 == fileNames.Length)
			{
				Console.WriteLine("No files found by name specified");
				Environment.Exit(-1);
			}

			analyzeFiles(fileNames, isTracing);
		}

		public static Boolean getTracingOptionFromArguments(String[] _args)
		{
			if (_args[0].Equals("--trace")) 
			{
				return true;
			}

			return false;
		}

		public static String[] getFileNamesFromArguments(String[] _args)
		{
			String fileArgument = _args[_args.Length - 1];
			String[] fileNames;
			if (fileArgument.Contains("*"))
			{
				fileNames = Directory.GetFiles(Environment.CurrentDirectory, fileArgument);
			}
			else
			{
				fileNames = new String[] { fileArgument };
			}

			return fileNames;
		}

		private static void printInstructionName(Byte[] code)
		{
			if (OpcodeHelper.GetStackEffect(code) != StackEffect.None)
			{
				Console.Write(OpcodeHelper.GetStackEffect(code).ToString().ToLower());
			}
			else
			{
				OperatorEffect effect = OpcodeHelper.GetOperatorEffect(code);
				if (OperatorEffect.Assignment == effect)
					Console.Write("mov");
				else if (OperatorEffect.None == effect)
					Console.Write("nop");
				else
					Console.Write(effect.ToString().ToLower());
			}

			Console.Write("\t");
		}

		private static void printOperands(Byte[] code)
		{
			if (OpcodeHelper.HasModRM(code) )
			{
				if (!ModRM.HasSIB(code))
				{
					Boolean evDereferenced = ModRM.IsEffectiveAddressDereferenced(code);
					if (evDereferenced)
						Console.Write("[");
		
					Console.Write(ModRM.GetEv(code).ToString().ToLower());

					if (evDereferenced)
						Console.Write("]");
				}
				else
				{
					Console.Write("[" + SIB.GetBaseRegister(code).ToString().ToLower() + "]");
				}

				Console.Write(", ");
			}

			if (OpcodeHelper.HasImmediate(code))
				Console.Write(String.Format("0x{0:x}", OpcodeHelper.GetImmediate(code))); 
		}

		private static void printEncoding(Byte[] code)
		{
			Console.Write("\t");
			if (OpcodeEncoding.None != OpcodeHelper.GetEncoding(code))
				Console.WriteLine("\t(" + OpcodeHelper.GetEncoding(code) + ")");
			else
				Console.WriteLine();
		}

		private static void printInfo(MachineState state, Byte[] code)
		{
			Console.WriteLine();
			Console.Write(String.Format("{0:x8}", state.InstructionPointer) + ":\t");
			foreach (Byte codeByte in code)
			{
				Console.Write(String.Format("{0:x2}", codeByte) + " ");
			}

			// magic numbers that happen to look good :)
			Int32 numberOfTabs = 3 - code.Length / 3;
			for (Int32 i=0; i < numberOfTabs; i++)
				Console.Write("\t");

			printInstructionName(code);

			printOperands(code);

			printEncoding(code);

			Console.WriteLine("topOfStack="+ state.TopOfStack +"  "+ state.Registers);
		}

		private static void analyzeFiles(String[] _fileNames, Boolean _isTracing)
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
				catch (FileNotFoundException e)
				{
					Console.WriteLine(e.Message);
					Environment.Exit(-1);
					return; // needed to hush up a false warning in mono
				}

				analyzer = new Analyzer(fileStream);

				if (_isTracing)
				{
					analyzer.OnEmulationComplete += printInfo;
				}

				analyzer.Run();
				IList<ReportItem> reportItems = analyzer.ReportItems;
				foreach (ReportItem item in reportItems)
				{
					String message = String.Empty;
					if (item.IsTainted)
						message += "Exploitable ";
					message += String.Format("OOB at EIP 0x{0:x4}", item.InstructionPointer);
					Console.WriteLine(message);					
				}
			}
		}
	} 
}
