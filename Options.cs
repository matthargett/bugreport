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
        private static ReadOnlyCollection<String> filenames;
        private static FileResolver fileResolver = new FileResolver();

        private static String functionToAnalyze;
        private static Boolean isDebugging;
        private static Boolean isTracing;

        public static String FunctionToAnalyze
        {
            get { return functionToAnalyze; }
        }

        public static ReadOnlyCollection<String> Filenames
        {
            get { return filenames; }
        }

        public static Boolean IsTracing
        {
            get { return isTracing; }
        }

        public static Boolean IsDebugging
        {
            get { return isDebugging; }
        }

        internal static FileResolver FileResolver
        {
            set { fileResolver = value; }
        }

        public static void ParseArguments(String[] commandLine)
        {
            functionToAnalyze = getFunctionToAnalyze(commandLine);
            filenames = getFilenames(commandLine);
            isTracing = getIsTracing(commandLine);
            isDebugging = getIsDebugging(commandLine);
            if (isDebugging)
            {
                isTracing = true;
            }
        }

        private static String getFunctionToAnalyze(String[] arguments)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].StartsWith("--function")) continue;

                var indexOfEquals = arguments[i].IndexOf("=", StringComparison.Ordinal);

                if (indexOfEquals == -1)
                {
                    throw new ArgumentException("--function malformed, should be in the form of --function=name");
                }

                return arguments[i].Substring(indexOfEquals + 1);
            }

            return "_start";
        }

        private static ReadOnlyCollection<String> getFilenames(String[] arguments)
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

        private static Boolean getIsTracing(IEnumerable<string> arguments)
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

        private static Boolean getIsDebugging(IEnumerable<string> arguments)
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