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
		Analyzer analyzer = new Analyzer();

		public String[] Messages 
		{
			get 
			{
				return analyzer.Messages;
			}
		}

		public static void Main(String[] args)
		{
			Console.WriteLine("bugreport " + VERSION);

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
			
			new MainClass().AnalyzeWrapper(fileArgument, isTracing);
		}
		
		
		public void AnalyzeWrapper(String fileArgument, Boolean isTracing)
		{
			analyzer.Analyze(fileArgument, isTracing);			
		}
	} 
}
