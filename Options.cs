// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace bugreport
{
    internal class FileResolver
    {
        public virtual ReadOnlyCollection<String> GetFilesFromDirectory(String path, String fileName)
        {
            return new ReadOnlyCollection<String>(Directory.GetFiles(path, fileName));
        }
    }

    public static class Options
    {
        private static FileResolver fileResolver = new FileResolver();

        private static String functionToAnalyze;
        private static ReadOnlyCollection<String> filenames;
        private static Boolean isTracing;
        private static Boolean isDebugging;

        internal static FileResolver FileResolver
        {
            set
            {
                fileResolver = value;
            }
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
            for (Int32 i=0; i < arguments.Length; i++)
            {
                if (arguments[i].StartsWith("--function"))
                {
                    Int32 indexOfEquals = arguments[i].IndexOf("=", StringComparison.Ordinal);

                    if (indexOfEquals == -1)
                    {
                        throw new ArgumentException("--function malformed, should be in the form of --function=name");
                    }
                    return arguments[i].Substring(indexOfEquals + 1);
                }
            }

            return "_start";
        }

        public static String FunctionToAnalyze
        {
            get
            {
                return functionToAnalyze;
            }
        }

        private static ReadOnlyCollection<String> getFilenames(String[] arguments)
        {
            String fileArgument = arguments[arguments.Length - 1];

            if (fileArgument.Contains("*"))
            {
                String path;

                if (fileArgument.Contains(Path.DirectorySeparatorChar.ToString()) || fileArgument.Contains(Path.AltDirectorySeparatorChar.ToString()))
                {
                    Int32 directorySeparatorIndex = fileArgument.LastIndexOf(Path.DirectorySeparatorChar);
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

                String fileName = Path.GetFileName(fileArgument);

                return fileResolver.GetFilesFromDirectory(path, fileName);
            }
            else
            {
                List<String> fileNames = new List<String>();
                foreach (String argument in arguments)
                {
                    if (!argument.StartsWith("--"))
                    {
                        fileNames.Add(argument);
                    }
                }

                return fileNames.AsReadOnly();
            }
        }

        public static ReadOnlyCollection<String> Filenames
        {
            get
            {
                return filenames;
            }
        }

        private static Boolean getIsTracing(IEnumerable<string> arguments)
        {
            foreach (String argument in arguments)
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
            foreach (String argument in arguments)
            {
                if (argument == "--debug")
                {
                    return true;
                }
            }

            return false;
        }
        
        public static Boolean IsTracing
        {
            get
            {
                return isTracing;
            }
        }

        public static Boolean IsDebugging
        {
            get
            {
                return isDebugging;
            }
        }
    }
}
