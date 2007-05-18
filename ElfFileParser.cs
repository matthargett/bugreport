// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert,
// Cullen Bryan, Mike Seery
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Globalization;


namespace bugreport
{

public sealed class ElfFileParser : IParsable, IDisposable
{
    private Stream stream;
    private Byte[] textData;

    public ElfFileParser(Stream stream)
    {
        this.stream = stream;
        textData = new Byte [stream.Length];
    }
    
    public void Dispose()
    {
        if (null != stream)
        {
            stream.Dispose();
        }
    }   

    public ReadOnlyCollection<ReportItem> ExpectedReportItems
    {
        get
        {
            return new ReadOnlyCollection<ReportItem>(new List<ReportItem>());
        }
    }

    public Byte[] GetBytes()
    {
        stream.Seek(0x2e0, SeekOrigin.Begin);
        stream.Read(textData, 0, (Int32)(stream.Length - stream.Position));
        return textData;
    }
    
    public UInt32 BaseAddress
    {
        get { return 0x080482e0; }
    }

    public UInt32 EntryPointAddress
    {
        get { return 0x080482e0; }
    }
}
}
