// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert,
// Cullen Bryan, Mike Seery
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using NUnit.Framework;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace bugreport
{
	[TestFixture]
	public class OptionsTest
	{
		internal class FakeFileResolver : FileResolver
		{
			String expectedPath, expectedFileName;
			Int32 numberOfFilesFound;
			
			public FakeFileResolver(String expectedPath, String expectedFileName, Int32 numberOfFilesFound)
			{
				this.expectedPath = expectedPath;
				this.expectedFileName = expectedFileName;
				this.numberOfFilesFound = numberOfFilesFound;
			}
			
			public override ReadOnlyCollection<String> GetFilesFromDirectory(String path, String fileName)
			{
				if (path == expectedPath && fileName == expectedFileName)
				{
					return new ReadOnlyCollection<String>(new String[numberOfFilesFound]);
				}
				
				throw new ArgumentException(
					String.Format("expectations not met: path = {0} , fileName = {1}", path, fileName)
				);
			}
		}
		
		FakeFileResolver fileResolver;
		String[] commandLine;
		
		[Test]
		public void WildcardInFileName()
		{
			fileResolver = new FakeFileResolver(
				@"c:\cygwin\home\matt\bugreport\tests\simple\heap",
				@"simple-malloc-via-immediate*.dump",
				12
			);
			Options.FileResolver = fileResolver;
			commandLine = new String[] {
				@"c:\cygwin\home\matt\bugreport\tests\simple\heap\simple-malloc-via-immediate*.dump"
			};
			Options.ParseArguments(commandLine);
			Assert.AreEqual(12, Options.Filenames.Count);

			commandLine = new String[] {"simple-malloc-via-immediate*.dump"};

			String oldDirectory = Directory.GetCurrentDirectory();
			try
			{
				Directory.SetCurrentDirectory(Environment.SystemDirectory);
				fileResolver = new FakeFileResolver(
					Environment.SystemDirectory,
					@"simple-malloc-via-immediate*.dump",
					12
				);
				Options.FileResolver = fileResolver;
				Options.ParseArguments(commandLine);
				Assert.AreEqual(12, Options.Filenames.Count);
			}
			
			finally
			{
				Directory.SetCurrentDirectory(oldDirectory);
			}
		}
		
		[Test]
		public void TraceOption()
		{
			commandLine = new String[] {
				"--trace", 
				@"c:\cygwin\home\matt\bugreport\tests\simple\heap\simple-malloc-via-immediate*.dump"
			};
			Options.ParseArguments(commandLine);
			Assert.IsTrue(Options.IsTracing);

			commandLine = new String[] {
				@"c:\cygwin\home\matt\bugreport\tests\simple\heap\simple-malloc-via-immediate*.dump",
				"--trace"
			};
			Options.ParseArguments(commandLine);
			Assert.IsTrue(Options.IsTracing);

			commandLine = new String[] {
				@"c:\cygwin\home\matt\bugreport\tests\simple\heap\simple-malloc-via-immediate*.dump"
			};
			Options.ParseArguments(commandLine);
			Assert.IsFalse(Options.IsTracing);
		}
		
		[Test]
		public void MultipleFilenames()
		{
			commandLine = new String[] {
				@"c:\cygwin\home\matt\bugreport\tests\simple\heap\simple-malloc-via-immediate_gcc403-02-g.dump", 
				@"c:\cygwin\home\matt\bugreport\tests\simple\heap\simple-malloc-via-immediate2_gcc403-02-g.dump"
			};
			Options.ParseArguments(commandLine);
			Assert.AreEqual(2, Options.Filenames.Count);
		}
		
		[Test]
		public void FunctionName()
		{
			commandLine = new String[] {
				"--function=nomain", 
				@"c:\cygwin\home\matt\bugreport\tests\simple\heap\simple-malloc-via-immediate_gcc403-02-g.dump", 
				@"c:\cygwin\home\matt\bugreport\tests\simple\heap\simple-malloc-via-immediate2_gcc403-02-g.dump"
			};
			Options.ParseArguments(commandLine);
			Assert.AreEqual("nomain", Options.FunctionToAnalyze);
			Assert.AreEqual(2, Options.Filenames.Count);

			
			commandLine = new String[] {
				@"c:\cygwin\home\matt\bugreport\tests\simple\heap\simple-malloc-via-immediate_gcc403-02-g.dump"
			};
			Options.ParseArguments(commandLine);
			Assert.AreEqual("main", Options.FunctionToAnalyze);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void FunctionNameWithoutEquals()
		{
			commandLine = new String[] {
				"--function", 
				@"c:\cygwin\home\matt\bugreport\tests\simple\heap\simple-malloc-via-immediate2_gcc403-02-g.dump"
			};
			Options.ParseArguments(commandLine);
			Assert.AreEqual("nomain", Options.FunctionToAnalyze);
		}
		
		[Test]
		public void FileResolver()
		{
			FileResolver realFileResolver = new FileResolver();
			ReadOnlyCollection<String> files = realFileResolver.GetFilesFromDirectory(Environment.CurrentDirectory, "*");
			Assert.IsNotNull(files);
		}
	}
}
