// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Text;

namespace bugreport
{
    public enum RegisterName
    {
        EAX,
        ECX,
        EDX,
        EBX,
        ESP,
        EBP,
        ESI,
        EDI,
        Unknown,
        None
    }

    public class RegisterCollection
    {
        private readonly AbstractValue[] registers;

        public RegisterCollection()
        {
            registers = new AbstractValue[8];
            for (UInt32 i = 0; i < registers.Length; ++i)
            {
                registers[i] = new AbstractValue();
            }
        }

        public RegisterCollection(RegisterCollection _copyMe)
        {
            registers = new AbstractValue[8];
            for (UInt32 i = 0; i < registers.Length; ++i)
            {
                registers[i] = new AbstractValue(_copyMe.registers[i]);
            }
        }

        public AbstractValue this[RegisterName index]
        {
            get { return registers[(Int32) index]; }
            set { registers[(Int32) index] = value; }
        }

        public override Boolean Equals(object obj)
        {
            var other = obj as RegisterCollection;

            if (null == other)
            {
                return false;
            }

            for (Int32 i = 0; i < registers.Length; i++)
            {
                if (!registers[i].Equals(other.registers[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override Int32 GetHashCode()
        {
            Int32 hashCode = 0;

            for (Int32 i = 0; i < registers.Length; i++)
            {
                hashCode ^= registers[i].GetHashCode();
            }

            return hashCode;
        }

        public override String ToString()
        {
            var result = new StringBuilder(String.Empty);
            for (UInt32 i = 0; i < registers.Length; ++i)
            {
                AbstractValue value = registers[i];
                result.Append(Enum.GetName(typeof(RegisterName), i) + "=" + value + "\t");
                if (value.ToString().Length < 8)
                {
                    result.Append("\t");
                }

                if ((i + 1) % 4 == 0)
                {
                    result.Append(Environment.NewLine);
                }
            }
            
            return result.ToString();
        }
    }
}