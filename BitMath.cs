﻿// Copyright (c) 2006 Luis Miras
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using NUnit.Framework;
using System;

namespace bugreport
{
	public static class BitMath
	{
		public static UInt32 BytesToDword(Byte[] bytes, Byte index)
		{
				UInt32 result = 0;
				
				if (bytes.Length < index + 4)
					throw new ArgumentException("bytes", String.Format("Not enough bytes for DWORD: need {0}, got {1}", index + 4, bytes.Length));
				for (Byte i = 0; i < 4; ++i)
				{
					result |= (UInt32) (bytes[i + index]) << (8*i);
				}
				
				return result;
		}
	}
}
