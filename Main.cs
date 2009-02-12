// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.IO;

namespace bugreport
{
    public static class MainClass
    {
        private const String VERSION = "0.1";
        private static Analyzer analyzer;

        internal static void Main(String[] arguments)
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
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                printUsage();
                Environment.Exit(1);
            }

            if (0 == Options.Filenames.Count)
            {
                Console.WriteLine("No files found by name specified");
                Environment.Exit(1);
            }

            analyzeFiles(Options.Filenames, Options.IsTracing, Options.IsDebugging);

            if (analyzer.ExpectedReportItems.Count == 0 ||
                (analyzer.ExpectedReportItems.Count == analyzer.ActualReportItems.Count))
            {
                return;
            }

            Console.WriteLine("Expectations Were Not Met:");
            Console.WriteLine(
                "Expected: " + analyzer.ExpectedReportItems.Count + " Actual: " + analyzer.ActualReportItems.Count);
            Environment.Exit(2);
        }

        private static void analyzeFiles(IEnumerable<string> _fileNames, Boolean _isTracing, Boolean _isDebugging)
        {
            foreach (var fileName in _fileNames)
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
                    Environment.Exit(3);
                    return; // needed to hush up a false warning in mono
                }

                // TODO (matt_hargett): have a factory auto-detect the file type and return the right kind of IParsable
                // IParsable parser = new DumpFileParser(fileStream, Options.FunctionToAnalyze);
                IParsable parser = new ElfFileParser(fileStream);

                analyzer = new Analyzer(parser);
                analyzer.OnReport += printReportItem;

                if (_isTracing)
                {
                    analyzer.OnEmulationComplete += new DebuggerView(_isDebugging).printInfo;
                }

                analyzer.Run();
            }
        }

        private static void printReportItem(object sender, ReportEventArgs e)
        {
            var item = e.ReportItem;

            var message = "***";
            if (item.IsTainted)
            {
                message += "Exploitable ";
            }

            message += String.Format("OOB at EIP 0x{0:x4}", item.InstructionPointer);
            Console.WriteLine(message);
            Console.WriteLine();
        }

        private static void printUsage()
        {
            Console.WriteLine("Usage: bugreport.exe [--trace] file.test");
        }
    }
}