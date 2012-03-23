// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace bugreport
{
    [TestFixture]
    ////[Ignore("long")]
    [Platform(Exclude = "Mono")]
    public class MainTest
    {
        // TODO: This assumes that the test runner is run from the build directory.
        private readonly String testRoot = Directory.GetCurrentDirectory() + @"/../../tests/simple/heap/";
        private readonly String testDataFile = Directory.GetCurrentDirectory() + @"/../../systemTestsList.txt";

        private static void WaitForAnalysisToFinish(Process analysisProcess)
        {
            var maximumTimeAllowed = TimeSpan.FromSeconds(10);
            while (!analysisProcess.HasExited && (DateTime.Now - analysisProcess.StartTime < maximumTimeAllowed))
            {
                Thread.Sleep(100);
            }
        }

        private static List<String> GetOutputFrom(Process analysisProcess)
        {
            var messages = new List<String>();

            analysisProcess.StandardOutput.ReadLine(); // version string
            analysisProcess.StandardOutput.ReadLine(); // blank line
            analysisProcess.StandardOutput.ReadLine(); // file name
            while (!analysisProcess.StandardOutput.EndOfStream)
            {
                messages.Add(analysisProcess.StandardOutput.ReadLine());
            }

            return messages;
        }

        private static Process GetAnalysisProcessFor(String fileName)
        {
            var analysisProcess = new Process
                                  {
                                      StartInfo =
                                      {
                                          FileName = "bugreport.exe",
                                          Arguments = "\"" + fileName + "\"",
                                          RedirectStandardOutput = true,
                                          UseShellExecute = false,
                                          CreateNoWindow = false
                                      }
                                  };

            return analysisProcess;
        }

        private static List<String> GetOutputFor(String fileName)
        {
            var analysisProcess = GetAnalysisProcessFor(fileName);
            analysisProcess.Start();

            WaitForAnalysisToFinish(analysisProcess);

            return GetOutputFrom(analysisProcess);
        }

        [Test]
        [Category("long")]
        public void SystemTest()
        {
            var testSpecifications = File.ReadAllLines(testDataFile);

            foreach (var testSpecification in testSpecifications)
            {
                if (testSpecification.Trim().StartsWith("#") || testSpecification.Trim().Length == 0)
                {
                    continue;
                }

                // format: filename.dump[,expectedOutput output]
                var args = testSpecification.Split(',');
                var fileName = (testRoot + args[0]).Trim();
                var expectedOutput = args[1].Trim();

                Assert.IsTrue(File.Exists(fileName), fileName + " does not exist.  Fix paths in test data?");

                var messages = GetOutputFor(fileName);

                try
                {
                    if (String.IsNullOrEmpty(expectedOutput))
                    {
                        Assert.IsEmpty(messages, fileName + " ==> not empty: " + messages);
                    }
                    else
                    {
                        StringAssert.Contains(expectedOutput, messages[0]);
                    }
                }
                catch (AssertionException)
                {
                    foreach (var line in messages)
                    {
                        Console.WriteLine(line);
                    }

                    throw;
                }
            }
        }
    }
}