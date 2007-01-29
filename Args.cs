using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace bugreport
{
	public class Args
	{
		private String[] arguments;
		
		public Args(String[] arguments)
		{
			this.arguments = arguments;
		}
		
		public ReadOnlyCollection<String> Filenames
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
					
					return getFilesFromDirectory(path, fileName);
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
		
		public Boolean IsTracing
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

		protected virtual ReadOnlyCollection<String> getFilesFromDirectory(String path, String fileName)
		{
			return new ReadOnlyCollection<String>(Directory.GetFiles(path, fileName));
		}
	}
}
