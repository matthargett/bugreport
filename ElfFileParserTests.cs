
// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert,
// Cullen Bryan, Mike Seery
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;
using System.IO;
using NUnit.Framework;

namespace bugreport.ElfFileParserTests
{
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
public abstract class ElfFileParserFixture
{
    protected StreamWriter writer;
    protected ElfFileParser parser;
    protected MemoryStream stream;

    public virtual void SetUp()
    {
        stream = new MemoryStream();
        writer = new StreamWriter(stream);
    }

    [TearDown]
    public void TearDown()
    {
        if (parser != null)
        {
            parser.Dispose();
            parser = null;
        }
    }
}

[TestFixture]
public class WellFormatted:ElfFileParserFixture
{
    
    Byte [] textData;
    
    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        Byte[] elfHeader = new Byte[] {0x7f, 0x45, 0x4c, 0x46};
        textData = new Byte[] {0x90};
        stream.Write(elfHeader,0, elfHeader.Length);
        stream.Seek(0x2e0, SeekOrigin.Begin);
        stream.Write(textData, 0, textData.Length);
        parser = new ElfFileParser(stream);
    }

    [Test]
    public void GetBytes()
    {
        Assert.AreEqual(textData[0], parser.GetBytes()[0]);
    }
    
    
}

}
