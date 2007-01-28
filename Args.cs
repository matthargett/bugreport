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
		
		public ICollection<String> Filenames
		{
			get
			{
				String fileArgument = arguments[arguments.Length - 1];
				String[] fileNames;
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
					
					
					fileNames = getFilesFromDirectory(path, fileName);
				}
				else
				{
					fileNames = new String[] { fileArgument };
				}
	
				return new List<String>(fileNames);
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

		protected virtual String[] getFilesFromDirectory(String path, String fileName)
		{
			return Directory.GetFiles(path, fileName);
		}
	}
}
