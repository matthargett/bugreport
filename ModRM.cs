// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;

namespace bugreport
{
    public static class ModRM
    {
        private static readonly Opcode opcode = new X86Opcode();

        private static Byte GetRMFor(Byte modrm)
        {
            return (Byte)(modrm & 7);
        }

        public static RegisterName GetEvFor(Byte[] code)
        {
            if (HasSIB(code))
            {
                throw new InvalidOperationException("For ModRM that specifies SIB byte, usage of GetEv is invalid.");
            }

            var modRM = GetModRMFor(code);
            return (RegisterName)(modRM & 7);
        }

        internal static RegisterName GetGvFor(Byte[] code)
        {
            var modRM = GetModRMFor(code);
            return (RegisterName)((modRM >> 3) & 7);
        }

        internal static Byte GetOpcodeGroupIndexFor(Byte[] code)
        {
            var modRM = GetModRMFor(code);
            return (Byte)((modRM >> 3) & 7);
        }

        public static Boolean HasIndex(Byte[] code)
        {
            var modRM = GetModRMFor(code);
            var mod = GetModFor(modRM);

            return mod == 1 || mod == 2;
        }

        public static Byte GetIndexFor(Byte[] code)
        {
            if (!HasIndex(code))
            {
                throw new InvalidOperationException(
                    "For ModRM that does not specify an index, usage of GetIndexFor is invalid."
                    );
            }

            UInt32 modRMIndex = opcode.GetOpcodeLengthFor(code);
            var modRM = GetModRMFor(code);
            var mod = GetModFor(modRM);

            switch (mod)
            {
                case 1:
                {
                    return code[modRMIndex + 1];
                }

                default:
                {
                    throw new NotImplementedException(String.Format("Unsupported Mod: 0x{0:x2}", mod));
                }
            }
        }

        public static Boolean IsEffectiveAddressDereferenced(Byte[] code)
        {
            var modRM = GetModRMFor(code);

            return GetModFor(modRM) != 3;
        }

        public static Boolean HasOffset(Byte[] code)
        {
            if (HasSIB(code))
            {
                return false;
            }

            var modRM = GetModRMFor(code);
            return GetRMFor(modRM) == 5 && GetModFor(modRM) == 0;
        }

        public static UInt32 GetOffsetFor(Byte[] code)
        {
            var offsetBeginsAt = opcode.GetOpcodeLengthFor(code);
            offsetBeginsAt++; // for modRM byte

            return BitMath.BytesToDword(code, offsetBeginsAt);
        }

        public static Boolean HasSIB(Byte[] code)
        {
            var modRM = GetModRMFor(code);

            return GetRMFor(modRM) == 4 && IsEffectiveAddressDereferenced(code);
        }

        public static Boolean IsEvDword(Byte[] code)
        {
            var modRM = GetModRMFor(code);
            if ((GetModFor(modRM) == 0) &&
                (GetRMFor(modRM) == 5))
            {
                return true;
            }

            return false;
        }

        private static Byte GetModRMFor(Byte[] code)
        {
            Int32 modRMIndex = opcode.GetOpcodeLengthFor(code);

            if (modRMIndex > code.Length - 1)
            {
                throw new InvalidOperationException("No ModRM present: " + code[0]);
            }

            return code[modRMIndex];
        }

        private static Byte GetModFor(Byte modrm)
        {
            return (Byte)((modrm >> 6) & 3);
        }
    }
}