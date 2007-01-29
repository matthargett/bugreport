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
		private static String[] arguments;
		private static FileResolver fileResolver = new FileResolver();
		
		internal static FileResolver FileResolver
		{
			set
			{
				fileResolver = value;
			}
		}
		
		public static void ParseArguments(String[] commandLine)
		{
			arguments = commandLine;
		}
		
		public static String FunctionToAnalyze
		{
			get
			{
				for (Int32 i=0; i < arguments.Length - 1; i++)
				{
					if (arguments[i] == "--function")
					{
						return arguments[i + 1];
					}
				}
				
				return "main";
			}
		}
		
		public static ReadOnlyCollection<String> Filenames
		{
			get
			{
				String fileArgument = arguments[arguments.Length - 1];
				if (fileArgument.Contains("*"))
				{
					String path;
					
					if (fileArgument.Contains(Path.DirectorySeparatorChar.ToString()))
					{
						path = fileArgument.Substring(0, fileArgument.LastIndexOf(Path.DirectorySeparatorChar));
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
		}	
		
		public static Boolean IsTracing
		{
			get
			{
				if (arguments[0].Equals("--trace"))
				{
					return true;
				}
	
				return false;
			}
		}
	}
}
