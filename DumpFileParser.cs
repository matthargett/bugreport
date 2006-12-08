// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
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
		
		Byte [] GetNextInstructionBytes();
		
		String CurrentLine
		{	
			get;
		}
	}
	
	public class DumpFileParser : IParsable
	{
		private Stream stream;
		private StreamReader reader;
		private Boolean inMain;
		private String currentLine;

		public DumpFileParser(Stream _stream)
		{
			stream = _stream;
			stream.Position = 0;
			reader = new StreamReader(stream);
			inMain = false;
		}
		
		public String CurrentLine
		{
			get 
			{ 
				if (currentLine == null)
				{
					throw new InvalidOperationException("No previous line read");
				}
				
				return currentLine; 
			}
		}
			
		public Byte[] GetNextInstructionBytes()
		{
			Byte[] hexBytes = null;
			
			while (null == hexBytes && !reader.EndOfStream)
			{
				currentLine = reader.ReadLine();
				if (isInMain(currentLine))
					hexBytes = getHexFromString(currentLine);
				else
					hexBytes = null;
			} 
			
			return hexBytes;
		}
		
		public Boolean EndOfFunction
		{
			get
			{
				return reader.EndOfStream;
			}
		}
		
		private Boolean isInMain(String _line)
		{
			if (_line.Length > 0 && _line[0] >= '0' && _line[0] <= '7')
			{
				if (_line.Contains("<main>:"))
					inMain = true;
				else 
					inMain = false;
			}
			
			return inMain;	
		}
		
		private String getHexWithSpaces(String line)
		{
			Int32 colonIndex = line.IndexOf(':');
			
			if (colonIndex == -1)
				return null;
			else if (colonIndex != 8)
				return null;
			
			String afterColonToEnd = line.Substring(colonIndex+1).Trim();
		    Int32 doubleSpaceIndex = afterColonToEnd.IndexOf("  ");
		    Int32 spaceTabIndex = afterColonToEnd.IndexOf(" \t");
		    Int32 endOfHexIndex;
		    
			if ( doubleSpaceIndex >= 0 && spaceTabIndex >= 0)
				endOfHexIndex = doubleSpaceIndex < spaceTabIndex ? doubleSpaceIndex : spaceTabIndex;
			else
				endOfHexIndex = doubleSpaceIndex > spaceTabIndex ? doubleSpaceIndex : spaceTabIndex;
				
			if (endOfHexIndex == -1)
				return null;
				
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

	}
	
}
