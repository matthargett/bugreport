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
            const int DWORD_SIZE = 4;

            if (data.Length < index + DWORD_SIZE)
            {
                throw new ArgumentException(
                    String.Format("Not enough bytes for DWORD: need {0}, got {1}", index + DWORD_SIZE, data.Length),
                    "data"
                    );
            }

            for (var i = 0; i < DWORD_SIZE; ++i)
            {
                result |= (UInt32)data[i + index] << (8*i);
            }

            return result;
        }
    }
}