using NUnit.Framework;
using System;
using System.IO;

namespace bugreport
{
	[TestFixture]
	public class ArgsTest
	{
		internal class FakeArgs : Args
		{
			String expectedPath, expectedFileName;
			Int32 numberOfFilesFound;
			
			public FakeArgs(String[] arguments, String expectedPath, String expectedFileName, Int32 numberOfFilesFound) : base(arguments)
			{
				this.expectedPath = expectedPath;
				this.expectedFileName = expectedFileName;
				this.numberOfFilesFound = numberOfFilesFound;
			}
			
			protected override String[] getFilesFromDirectory(String path, String fileName)
			{
				if (path == expectedPath && fileName == expectedFileName)
				{
					return new String[numberOfFilesFound];
				}
				
				throw new ArgumentException(
					String.Format("expectations not met: path = {0} , fileName = {1}", path, fileName)
				);
			}
		}
		
		Args args;
		String[] commandLine;
		
		[Test]
		public void WildcardInFileName()
		{
			commandLine = new String[] {@"c:\cygwin\home\matt\bugreport\tests\simple\heap\simple-malloc-via-immediate*.dump"};
			args = new FakeArgs(
				commandLine,
				@"c:\cygwin\home\matt\bugreport\tests\simple\heap",
				@"simple-malloc-via-immediate*.dump",
				12
			);
			Assert.AreEqual(12, args.Filenames.Count);

			commandLine = new String[] {"simple-malloc-via-immediate*.dump"};

			String oldDirectory = Directory.GetCurrentDirectory();
			try
			{
				Directory.SetCurrentDirectory(Environment.SystemDirectory);
				args = new FakeArgs(
					commandLine,
					Environment.SystemDirectory,
					@"simple-malloc-via-immediate*.dump",
					12
				);
				Assert.AreEqual(12, args.Filenames.Count);
			}
			
			finally
			{
				Directory.SetCurrentDirectory(oldDirectory);
			}
		}
		
		[Test]
		public void TraceOption()
		{
			commandLine = new String[] {"--trace", @"c:\cygwin\home\matt\bugreport\tests\simple\heap\simple-malloc-via-immediate*.dump"};
			args = new Args(commandLine);
			Assert.IsTrue(args.IsTracing);

			commandLine = new String[] {@"c:\cygwin\home\matt\bugreport\tests\simple\heap\simple-malloc-via-immediate*.dump"};
			args = new Args(commandLine);
			Assert.IsFalse(args.IsTracing);
		}
	}
}
