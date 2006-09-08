// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace bugreport
{
	[TestFixture]
	public class MainTests
	{
		// TODO: This assumes that the test runner is run from the build directory.
		private string testRoot = 
			Directory.GetCurrentDirectory() + @"/../../tests/simple/heap/";			
		
		private string testDataFile = Directory.GetCurrentDirectory() + @"/../../systemTestsList.txt";
		
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
			string[] tests = File.ReadAllLines(testDataFile);
									
			foreach(string s in tests) {
				string test = s.Trim();
				if (test.StartsWith("#") || test.Length == 0) {
					continue;
				}
				
				MainClass analyzer = new MainClass();
				
				// format: filename.dump[,expected output]
				string[] args = test.Split(',');
				string argv = (testRoot + args[0]).Trim();
				string expected = args[1].Trim();				
				
				Assert.IsTrue(File.Exists(argv), 
				              argv + " does not exist.  Fix paths in test data?");
				
				analyzer.Analyze(new String[]{argv});				
				
				string[] messages = analyzer.Messages;
				
				if (expected == "") {
					Assert.IsEmpty(messages,
					               argv + " ==> not empty: " + String.Join(":", messages));
				} else {
					Assert.Contains(expected, messages);
				}
			}
		}		
	}
}
