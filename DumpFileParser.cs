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
public interface IParsable
{
    ReadOnlyCollection<ReportItem> ExpectedReportItems { get; }

    UInt32 BaseAddress { get; }
    UInt32 EntryPointAddress { get; }
    
    Byte [] GetBytes();
}

public sealed class DumpFileParser : IParsable, IDisposable
{
    private readonly Stream stream;
    private readonly StreamReader reader;
    private Boolean inTextSection;    
    private readonly List<Byte[]> opCodeList;
    private readonly List<ReportItem> expectedReportItems;
    private UInt32 entryPointAddress, baseAddress;
    private readonly String functionNameToParse = "main";

    public DumpFileParser(Stream stream, String functionNameToParse)
    {
        this.functionNameToParse = functionNameToParse;
        this.stream = stream;
        this.stream.Position = 0;
        reader = new StreamReader(this.stream);
        expectedReportItems = new List<ReportItem>();
        opCodeList = parse();
    }

    public void Dispose()
    {
        if (null != reader)
        {
            reader.Dispose();
        }
    }   

    public Byte[] GetBytes()
    {
        if (opCodeList.Count == 0)
        {
            return null;
        }

        Int32 total = 0;
        foreach (Byte[] bytes in opCodeList)
        {
            total += bytes.Length;
        }

        int allByteCount = 0;
        Byte[] allBytes = new Byte[total];
        foreach (Byte[] bytes in opCodeList)
        {
            for (int i=0;i<bytes.Length;i++)
            {
                allBytes[i+allByteCount] = bytes[i];
            }
            allByteCount+=bytes.Length;
        }

        return allBytes;
    }   

    public ReadOnlyCollection<ReportItem> ExpectedReportItems
    {
        get
        {
            return expectedReportItems.AsReadOnly();
        }
    }

    public UInt32 BaseAddress
    {
        get { return baseAddress; }
    }

    public UInt32 EntryPointAddress
    {
        get { return entryPointAddress; }
    }
    
    private static UInt32 getAddressForLine(String line)
    {
        String address = line.Substring(0,8);
        return UInt32.Parse(address, NumberStyles.HexNumber);
    }

    private void updateMainInfo(String line)
    {
        if (line.Length > 0 && line[0] >= '0' && line[0] <= '7')
        {
            if (line.Contains("<_start>:"))
            {
                baseAddress = getAddressForLine(line);
                inTextSection = true;
            }

            if (line.Contains("<" + functionNameToParse +">:"))
            {
                entryPointAddress = getAddressForLine(line);
            }
        }
    }

    private static String getHexWithSpaces(String line)
    {
        Int32 colonIndex = line.IndexOf(':');

        if (colonIndex == -1)
        {
            return null;
        }
        else if (colonIndex != 8)
        {
            return null;
        }

        String afterColonToEnd = line.Substring(colonIndex+1).Trim();
        Int32 doubleSpaceIndex = afterColonToEnd.IndexOf("  ", StringComparison.Ordinal);
        Int32 spaceTabIndex = afterColonToEnd.IndexOf(" \t", StringComparison.Ordinal);
        Int32 endOfHexIndex;

        if ( doubleSpaceIndex >= 0 && spaceTabIndex >= 0)
        {
            endOfHexIndex = doubleSpaceIndex < spaceTabIndex ? doubleSpaceIndex : spaceTabIndex;
        }
        else
        {
            endOfHexIndex = doubleSpaceIndex > spaceTabIndex ? doubleSpaceIndex : spaceTabIndex;
        }

        if (endOfHexIndex == -1)
        {
            return null;
        }

        String hexString = afterColonToEnd.Substring(0, endOfHexIndex).Trim();

        return hexString;
    }

    public static Byte[] getByteArrayFromHexString(String hex)
    {
        String[] hexStrings = hex.Split(new Char[] {' '});

        Byte[] hexBytes = new Byte[hexStrings.Length];

        for (Int32 i = 0; i < hexStrings.Length; ++i)
        {
            hexBytes[i] = Byte.Parse(hexStrings[i], NumberStyles.HexNumber);
        }

        return hexBytes;
    }

    private static Byte[] getHexFromString(String line)
    {

        if (line.Trim().Length == 0)
            return null;

        String hex = getHexWithSpaces(line);

        if (null == hex)
            return null;

        Byte[] hexBytes = getByteArrayFromHexString(hex);

        return hexBytes;
    }

    private List<Byte[]> parse()
    {
        List<Byte[]> opcodes = new List<Byte[]>();

        while (!reader.EndOfStream)
        {
            String currentLine = reader.ReadLine();
            if (hasAnnotation(currentLine))
            {
                ReportItem item = getAnnotation(currentLine);
                expectedReportItems.Add(item);
            }

            updateMainInfo(currentLine);

            if (inTextSection)
            {
                Byte[] opCode = getHexFromString(currentLine);
                if (opCode != null)
                {
                    opcodes.Add(getHexFromString(currentLine));
                }
            }
        }
        return opcodes;
    }

    private static Boolean hasAnnotation(String line)
    {
        return line.Contains("//<OutOfBoundsMemoryAccess ");
    }

    private static ReportItem getAnnotation(String line)
    {
        Int32 locationIndex = line.IndexOf("=", StringComparison.Ordinal) + 1;
        UInt32 location = UInt32.Parse(line.Substring(locationIndex + "/>".Length, 8), NumberStyles.HexNumber);
        Int32 exploitableIndex = line.IndexOf("=", locationIndex + 1, StringComparison.Ordinal) + 1;
        Boolean exploitable = Boolean.Parse(line.Substring(exploitableIndex, (line.Length - exploitableIndex)-"/>".Length));
        return new ReportItem(location, exploitable);
    }
}
}
