// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace bugreport
{
    public static class Options
    {
        private static FileResolver fileResolver = new FileResolver();

        public static String FunctionToAnalyze { get; private set; }

        public static ReadOnlyCollection<String> Filenames { get; private set; }

        public static Boolean IsTracing { get; private set; }

        public static Boolean IsDebugging { get; private set; }

        internal static FileResolver FileResolver
        {
            set { fileResolver = value; }
        }

        public static void ParseArgumentsFrom(String[] commandLine)
        {
            FunctionToAnalyze = GetFunctionToAnalyzeFrom(commandLine);
            Filenames = GetFilenamesFrom(commandLine);
            IsTracing = GetIsTracingFrom(commandLine);
            IsDebugging = GetIsDebuggingFrom(commandLine);
            if (IsDebugging)
            {
                IsTracing = true;
            }
        }

        private static String GetFunctionToAnalyzeFrom(String[] arguments)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].StartsWith("--function"))
                {
                    continue;
                }

                var indexOfEquals = arguments[i].IndexOf("=", StringComparison.Ordinal);

                if (indexOfEquals == -1)
                {
                    throw new ArgumentException("--function malformed, should be in the form of --function=name");
                }

                return arguments[i].Substring(indexOfEquals + 1);
            }

            return "_start";
        }

        private static ReadOnlyCollection<String> GetFilenamesFrom(String[] arguments)
        {
            var fileArgument = arguments[arguments.Length - 1];

            if (fileArgument.Contains("*"))
            {
                String path;

                if (fileArgument.Contains(Path.DirectorySeparatorChar.ToString()) ||
                    fileArgument.Contains(Path.AltDirectorySeparatorChar.ToString()))
                {
                    var directorySeparatorIndex = fileArgument.LastIndexOf(Path.DirectorySeparatorChar);
                    if (-1 == directorySeparatorIndex)
                    {
                        directorySeparatorIndex = fileArgument.LastIndexOf(Path.AltDirectorySeparatorChar);
                    }

                    path = fileArgument.Substring(0, directorySeparatorIndex);
                }
                else
                {
                    path = Environment.CurrentDirectory;
                }

                var fileName = Path.GetFileName(fileArgument);

                return fileResolver.GetFilesFromDirectory(path, fileName);
            }

            var fileNames = new List<String>();
            foreach (var argument in arguments)
            {
                if (!argument.StartsWith("--"))
                {
                    fileNames.Add(argument);
                }
            }

            return fileNames.AsReadOnly();
        }

        private static Boolean GetIsTracingFrom(IEnumerable<string> arguments)
        {
            foreach (var argument in arguments)
            {
                if (argument == "--trace")
                {
                    return true;
                }
            }

            return false;
        }

        private static Boolean GetIsDebuggingFrom(IEnumerable<string> arguments)
        {
            foreach (var argument in arguments)
            {
                if (argument == "--debug")
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal class FileResolver
    {
        public virtual ReadOnlyCollection<String> GetFilesFromDirectory(String path, String fileName)
        {
            return new ReadOnlyCollection<String>(Directory.GetFiles(path, fileName));
        }
    }
}