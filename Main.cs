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
				analyzer.Run(_isTracing);
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
