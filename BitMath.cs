// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;

namespace bugreport
{
    public static class BitMath
    {
        public static UInt32 BytesToDword(Byte[] data, Byte index)
        {
            UInt32 result = 0;

            if (data.Length < index + 4)
            {
                throw new ArgumentException(
                    String.Format("Not enough bytes for DWORD: need {0}, got {1}", index + 4, data.Length), "data");
            }

            for (Byte i = 0; i < 4; ++i)
            {
                result |= (UInt32) data[i + index] << (8 * i);
            }

            return result;
        }
    }
}