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
	//[Ignore("long")]
	public class MainTests
	{
		// TODO: This assumes that the test runner is run from the build directory.
		private String testRoot = 
			Directory.GetCurrentDirectory() + @"/../../tests/simple/heap/";			

		private String testDataFile = Directory.GetCurrentDirectory() + @"/../../systemTestsList.txt";

		[Test]
		[Category("long")]
		public void SystemTest()
		{
			String[] tests = File.ReadAllLines(testDataFile);

			foreach(String s in tests) 
			{
				String test = s.Trim();
				if (test.StartsWith("#") || test.Length == 0) 
				{
					continue;
				}

				// format: filename.dump[,expected output]
				String[] args = test.Split(',');
				String fileName = (testRoot + args[0]).Trim();
				String expected = args[1].Trim();				

				Assert.IsTrue(File.Exists(fileName), fileName + " does not exist.  Fix paths in test data?");

				MainClass.Main(new String[] {fileName});	

				String[] messages = MainClass.Messages;

				if (expected == "") 
				{
					Assert.IsEmpty(messages,
						fileName + " ==> not empty: " + String.Join(":", messages));
				} 
				else 
				{
					Assert.Contains(expected, messages);
				}
			}
		}		
	}
}
