// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace bugreport
{
	[TestFixture]
	[Ignore("long")]
	public class MainTests
	{
		// TODO: This assumes that the test runner is run from the build directory.
		private String testRoot = 
			Directory.GetCurrentDirectory() + @"/../../tests/simple/heap/";			
		
		private String testDataFile = Directory.GetCurrentDirectory() + @"/../../systemTestsList.txt";
		
		[Test]
		public void GetParserFileDoesNotExist() {
			MainClass analyzer = new MainClass();
			Assert.IsNull(analyzer.getParserForFilename("this file does not exist"));
		}
		
		[Test]
		public void GetParserFileExists() {
			MainClass analyzer = new MainClass();
			Assert.IsNotNull(analyzer.getParserForFilename(testDataFile));
		}
		
		[Test]
		[Category("long")]
		public void SystemTest()
		{											
			String[] tests = File.ReadAllLines(testDataFile);
									
			foreach(String s in tests) {
				String test = s.Trim();
				if (test.StartsWith("#") || test.Length == 0) {
					continue;
				}
				
				MainClass analyzer = new MainClass();
				
				// format: filename.dump[,expected output]
				String[] args = test.Split(',');
				String fileName = (testRoot + args[0]).Trim();
				String expected = args[1].Trim();				
				
				Assert.IsTrue(File.Exists(fileName), 
				              fileName + " does not exist.  Fix paths in test data?");
				
				analyzer.Analyze(fileName, false);				
				
				String[] messages = analyzer.Messages;
				
				if (expected == "") {
					Assert.IsEmpty(messages,
					               fileName + " ==> not empty: " + String.Join(":", messages));
				} else {
					Assert.Contains(expected, messages);
				}
			}
		}		
	}
}
