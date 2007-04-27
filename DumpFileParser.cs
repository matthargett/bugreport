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
		Boolean EndOfFunction
		{
			get;
		}
		
		ReadOnlyCollection<ReportItem> ExpectedReportItems
		{
			get;
		}		
		
		UInt32 BaseAddress
		{
			get;
		}
		
		Byte [] GetNextInstructionBytes();
		Byte [] GetBytes();
	}
	
	public sealed class DumpFileParser : IParsable, IDisposable
	{
		private Stream stream;
		private StreamReader reader;
		private Boolean inMain;
		private String currentLine;
		private List<Byte[]> opCodeList;
		private Int32 index = -1;
		private List<ReportItem> expectedReportItems;
		private UInt32 baseAddress;
		private String functionNameToParse = "main";

		public DumpFileParser(Stream _stream, String functionNameToParse)
		{
			this.functionNameToParse = functionNameToParse;
			stream = _stream;
			stream.Position = 0;
			reader = new StreamReader(stream);
			expectedReportItems = new List<ReportItem>();
			opCodeList = parse();
		}
		
		public void Dispose()
		{
			if(null != reader)
			{
				reader.Dispose();
			}
		}
		
		public Byte[] GetNextInstructionBytes()
		{
			if (EndOfFunction)
			{
				return null;
			}
			
			return opCodeList[++index];
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
				for(int i=0;i<bytes.Length;i++)
				{
					allBytes[i+allByteCount] = bytes[i];
				}
				allByteCount+=bytes.Length;
			}
			
			return allBytes;
		}
		
		public Boolean EndOfFunction
		{
			get
			{
				return (index >= opCodeList.Count-1);
			}
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
			get
			{
				return baseAddress;
			}
		}
		
		private void updateMainInfo(String line)
		{
			if (line.Length > 0 && line[0] >= '0' && line[0] <= '7')
			{
				if (line.Contains("<" + functionNameToParse +">:"))
				{
					String address = line.Substring(0,8);
					baseAddress = UInt32.Parse(address, System.Globalization.NumberStyles.HexNumber);
					inMain = true;
				}
				else
				{
					inMain = false;
				}
			}
		}
		
		private String getHexWithSpaces(String line)
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
			Int32 doubleSpaceIndex = afterColonToEnd.IndexOf("  ");
			Int32 spaceTabIndex = afterColonToEnd.IndexOf(" \t");
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

		private Byte[] getByteArrayFromHexString(String hex)
		{
			String[] hexStrings = hex.Split(new Char[] {' '});
			
			Byte[] hexBytes = new Byte[hexStrings.Length];
			
			for (Int32 i = 0; i < hexStrings.Length; ++i)
			{
				hexBytes[i] = Byte.Parse(hexStrings[i], NumberStyles.HexNumber);
			}

			return hexBytes;
		}
		
		private Byte[] getHexFromString(String line)
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
			Byte[] opCode;
			List<Byte[]> opCodeList = new List<Byte[]>();

			while (!reader.EndOfStream)
			{
				currentLine = reader.ReadLine();
				if (hasAnnotation(currentLine))
				{
					ReportItem item = getAnnotation(currentLine);
					expectedReportItems.Add(item);
				}
				
				updateMainInfo(currentLine);

				if (inMain)
				{
					opCode = getHexFromString(currentLine);
					if (opCode != null)
					{
						opCodeList.Add(getHexFromString(currentLine));
					}
				}
			}
			return opCodeList;
		}

		private Boolean hasAnnotation(String line)
		{
			return line.Contains("//<OutOfBoundsMemoryAccess ");
		}

		private ReportItem getAnnotation(String line)
		{
			Int32 locationIndex = line.IndexOf("=") + 1;
			UInt32 location = UInt32.Parse(line.Substring(locationIndex + "/>".Length, 8), System.Globalization.NumberStyles.HexNumber);
			Int32 exploitableIndex = line.IndexOf("=", locationIndex + 1) + 1;
			Boolean exploitable = Boolean.Parse(line.Substring(exploitableIndex, (line.Length - exploitableIndex)-"/>".Length));
			return new ReportItem(location, exploitable);
		}
	}
}

