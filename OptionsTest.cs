// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Collections.ObjectModel;
using System.IO;
using NUnit.Framework;

namespace bugreport
{
    [TestFixture]
    public class OptionsTest
    {
        private FakeFileResolver fileResolver;
        private String[] commandLine;

        private sealed class FakeFileResolver : FileResolver
        {
            private readonly String expectedFileName;
            private readonly String expectedPath;
            private readonly Int32 numberOfFilesFound;

            public FakeFileResolver(String expectedPath, String expectedFileName, Int32 numberOfFilesFound)
            {
                this.expectedPath = expectedPath;
                this.expectedFileName = expectedFileName;
                this.numberOfFilesFound = numberOfFilesFound;
            }

            [CoverageExclude]
            public override ReadOnlyCollection<String> GetFilesFromDirectory(String path, String fileName)
            {
                if (path == expectedPath &&
                    fileName == expectedFileName)
                {
                    return new ReadOnlyCollection<String>(new String[numberOfFilesFound]);
                }

                throw new ArgumentException(
                    String.Format(
                        "expectations not met: path = {0} , expected = {1} ; fileName = {2} , expected = {3}",
                        path,
                        expectedPath,
                        fileName,
                        expectedFileName
                        )
                    );
            }
        }

        [Test]
        public void DebugOption()
        {
            fileResolver = new FakeFileResolver(
                @"/cygwin/home/steve/bugreport/tests/simple/heap",
                @"simple-malloc-via-immediate*.dump",
                12
                );
            Options.FileResolver = fileResolver;
            commandLine = new[]
                          {
                              "--debug",
                              @"/cygwin/home/steve/bugreport/tests/simple/heap/simple-malloc-via-immediate*.dump"
                          };
            Options.ParseArgumentsFrom(commandLine);
            Assert.IsTrue(Options.IsDebugging);

            commandLine = new[]
                          {
                              @"/cygwin/home/steve/bugreport/tests/simple/heap/simple-malloc-via-immediate*.dump",
                              "--debug"
                          };
            Options.ParseArgumentsFrom(commandLine);
            Assert.IsTrue(Options.IsDebugging);

            commandLine = new[]
                          {
                              @"/cygwin/home/steve/bugreport/tests/simple/heap/simple-malloc-via-immediate*.dump"
                          };
            Options.ParseArgumentsFrom(commandLine);
            Assert.IsFalse(Options.IsDebugging);
        }

        [Test]
        public void FileResolver()
        {
            var realFileResolver = new FileResolver();
            var files = realFileResolver.GetFilesFromDirectory(Environment.CurrentDirectory, "*");
            Assert.IsNotNull(files);
        }

        [Test]
        public void FunctionName()
        {
            commandLine = new[]
                          {
                              "--function=nomain",
                              @"/cygwin/home/steve/bugreport/tests/simple/heap/simple-malloc-via-immediate_gcc403-02-g.dump"
                              ,
                              @"/cygwin/home/steve/bugreport/tests/simple/heap/simple-malloc-via-immediate2_gcc403-02-g.dump"
                          };
            Options.ParseArgumentsFrom(commandLine);
            Assert.AreEqual("nomain", Options.FunctionToAnalyze);
            Assert.AreEqual(2, Options.Filenames.Count);

            commandLine = new[]
                          {
                              @"/cygwin/home/steve/bugreport/tests/simple/heap/simple-malloc-via-immediate_gcc403-02-g.dump"
                          };
            Options.ParseArgumentsFrom(commandLine);
            Assert.AreEqual("_start", Options.FunctionToAnalyze);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void FunctionNameWithoutEquals()
        {
            commandLine = new[]
                          {
                              "--function",
                              "alternatefuntionname",
                              @"/cygwin/home/steve/bugreport/tests/simple/heap/simple-malloc-via-immediate2_gcc403-02-g.dump"
                          };
            Options.ParseArgumentsFrom(commandLine);
        }

        [Test]
        public void MultipleFilenames()
        {
            commandLine = new[]
                          {
                              @"/cygwin/home/steve/bugreport/tests/simple/heap/simple-malloc-via-immediate_gcc403-02-g.dump"
                              ,
                              @"/cygwin/home/steve/bugreport/tests/simple/heap/simple-malloc-via-immediate2_gcc403-02-g.dump"
                          };
            Options.ParseArgumentsFrom(commandLine);
            Assert.AreEqual(2, Options.Filenames.Count);
        }

        [Test]
        public void TraceOption()
        {
            fileResolver = new FakeFileResolver(
                @"/cygwin/home/steve/bugreport/tests/simple/heap",
                @"simple-malloc-via-immediate*.dump",
                12
                );
            Options.FileResolver = fileResolver;
            commandLine = new[]
                          {
                              "--trace",
                              @"/cygwin/home/steve/bugreport/tests/simple/heap/simple-malloc-via-immediate*.dump"
                          };
            Options.ParseArgumentsFrom(commandLine);
            Assert.IsTrue(Options.IsTracing);

            commandLine = new[]
                          {
                              @"/cygwin/home/steve/bugreport/tests/simple/heap/simple-malloc-via-immediate*.dump",
                              "--trace"
                          };
            Options.ParseArgumentsFrom(commandLine);
            Assert.IsTrue(Options.IsTracing);

            commandLine = new[]
                          {
                              @"/cygwin/home/steve/bugreport/tests/simple/heap/simple-malloc-via-immediate*.dump"
                          };
            Options.ParseArgumentsFrom(commandLine);
            Assert.IsFalse(Options.IsTracing);
        }

        [Test]
        public void WildcardInFileName()
        {
            fileResolver = new FakeFileResolver(
                @"/cygwin/home/steve/bugreport/tests/simple/heap",
                @"simple-malloc-via-immediate*.dump",
                12
                );
            Options.FileResolver = fileResolver;
            commandLine = new[]
                          {
                              @"/cygwin/home/steve/bugreport/tests/simple/heap/simple-malloc-via-immediate*.dump"
                          };
            Options.ParseArgumentsFrom(commandLine);
            Assert.AreEqual(12, Options.Filenames.Count);

            commandLine = new[] {"simple-malloc-via-immediate*.dump"};

            var oldDirectory = Directory.GetCurrentDirectory();
            try
            {
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                Directory.SetCurrentDirectory(desktopPath);
                fileResolver = new FakeFileResolver(
                    desktopPath,
                    @"simple-malloc-via-immediate*.dump",
                    12
                    );
                Options.FileResolver = fileResolver;
                Options.ParseArgumentsFrom(commandLine);
                Assert.AreEqual(12, Options.Filenames.Count);
            }
            finally
            {
                Directory.SetCurrentDirectory(oldDirectory);
            }
        }
    }
}