// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace bugreport
{
    public interface IParsable
    {
        ReadOnlyCollection<ReportItem> ExpectedReportItems { get; }

        UInt32 BaseAddress { get; }

        UInt32 EntryPointAddress { get; }

        Byte[] GetBytes();
    }

    public sealed class DumpFileParser : IParsable, IDisposable
    {
        private readonly List<ReportItem> expectedReportItems;
        private readonly String functionNameToParse = "main";
        private readonly List<Byte[]> opcodeList;
        private readonly StreamReader reader;
        private readonly Stream stream;
        private UInt32 baseAddress;
        private UInt32 entryPointAddress;
        private Boolean inTextSection;

        public DumpFileParser(Stream stream, String functionNameToParse)
        {
            this.functionNameToParse = functionNameToParse;
            this.stream = stream;
            this.stream.Position = 0;
            reader = new StreamReader(this.stream);
            expectedReportItems = new List<ReportItem>();
            opcodeList = parse();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (null != reader)
            {
                reader.Dispose();
            }
        }

        #endregion

        #region IParsable Members

        public ReadOnlyCollection<ReportItem> ExpectedReportItems
        {
            get { return expectedReportItems.AsReadOnly(); }
        }

        public UInt32 BaseAddress
        {
            get { return baseAddress; }
        }

        public UInt32 EntryPointAddress
        {
            get { return entryPointAddress; }
        }

        public Byte[] GetBytes()
        {
            if (opcodeList.Count == 0)
            {
                return null;
            }

            var total = 0;
            foreach (var bytes in opcodeList)
            {
                total += bytes.Length;
            }

            var allByteCount = 0;
            var allBytes = new Byte[total];
            foreach (var bytes in opcodeList)
            {
                for (var i = 0; i < bytes.Length; i++)
                {
                    allBytes[i + allByteCount] = bytes[i];
                }

                allByteCount += bytes.Length;
            }

            return allBytes;
        }

        #endregion

        public static Byte[] getByteArrayFromHexString(String hex)
        {
            var hexStrings = hex.Split(new[] {' '});

            var hexBytes = new Byte[hexStrings.Length];

            for (var i = 0; i < hexStrings.Length; ++i)
            {
                hexBytes[i] = Byte.Parse(hexStrings[i], NumberStyles.HexNumber);
            }

            return hexBytes;
        }

        private static UInt32 getAddressForLine(String line)
        {
            var address = line.Substring(0, 8);
            return UInt32.Parse(address, NumberStyles.HexNumber);
        }

        private static String getHexWithSpaces(String line)
        {
            var colonIndex = line.IndexOf(':');

            if (colonIndex == -1)
            {
                return null;
            }

            if (colonIndex != 8)
            {
                return null;
            }

            var afterColonToEnd = line.Substring(colonIndex + 1).Trim();
            var doubleSpaceIndex = afterColonToEnd.IndexOf("  ", StringComparison.Ordinal);
            var spaceTabIndex = afterColonToEnd.IndexOf(" \t", StringComparison.Ordinal);
            Int32 endOfHexIndex;

            if (doubleSpaceIndex >= 0 && spaceTabIndex >= 0)
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

            var hexString = afterColonToEnd.Substring(0, endOfHexIndex).Trim();

            return hexString;
        }

        private static Byte[] getHexFromString(String line)
        {
            if (line.Trim().Length == 0)
            {
                return null;
            }

            var hex = getHexWithSpaces(line);

            if (null == hex)
            {
                return null;
            }

            var hexBytes = getByteArrayFromHexString(hex);

            return hexBytes;
        }

        private static Boolean hasAnnotation(String line)
        {
            return line.Contains("//<OutOfBoundsMemoryAccess ");
        }

        private static ReportItem getAnnotation(String line)
        {
            var locationIndex = line.IndexOf("=", StringComparison.Ordinal) + 1;
            var location = UInt32.Parse(line.Substring(locationIndex + "/>".Length, 8), NumberStyles.HexNumber);
            var exploitableIndex = line.IndexOf("=", locationIndex + 1, StringComparison.Ordinal) + 1;
            var exploitable =
                Boolean.Parse(line.Substring(exploitableIndex, (line.Length - exploitableIndex) - "/>".Length));
            return new ReportItem(location, exploitable);
        }

        private void updateMainInfo(String line)
        {
            if (String.IsNullOrEmpty(line) || line[0] < '0' || line[0] > '7') return;

            if (line.Contains("<_start>:"))
            {
                baseAddress = getAddressForLine(line);
                inTextSection = true;
            }

            if (line.Contains("<" + functionNameToParse + ">:"))
            {
                entryPointAddress = getAddressForLine(line);
            }
        }

        private List<Byte[]> parse()
        {
            var opcodes = new List<Byte[]>();

            while (!reader.EndOfStream)
            {
                var currentLine = reader.ReadLine();
                if (hasAnnotation(currentLine))
                {
                    var item = getAnnotation(currentLine);
                    expectedReportItems.Add(item);
                }

                updateMainInfo(currentLine);

                if (inTextSection)
                {
                    var opcode = getHexFromString(currentLine);
                    if (opcode != null)
                    {
                        opcodes.Add(getHexFromString(currentLine));
                    }
                }
            }

            return opcodes;
        }
    }
}