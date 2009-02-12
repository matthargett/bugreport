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

        private static Byte GetRM(Byte modrm)
        {
            return (Byte) (modrm & 7);
        }

        public static RegisterName GetEv(Byte[] code)
        {
            if (HasSIB(code))
            {
                throw new InvalidOperationException("For ModRM that specifies SIB byte, usage of GetEv is invalid.");
            }

            var modRM = getModRM(code);
            return (RegisterName) (modRM & 7);
        }

        internal static RegisterName GetGv(Byte[] code)
        {
            var modRM = getModRM(code);
            return (RegisterName) ((modRM >> 3) & 7);
        }

        internal static Byte GetOpcodeGroupIndex(Byte[] _code)
        {
            var modRM = getModRM(_code);
            return (Byte) ((modRM >> 3) & 7);
        }

        public static Boolean HasIndex(Byte[] _code)
        {
            var modRM = getModRM(_code);
            var mod = getMod(modRM);

            return mod == 1 || mod == 2;
        }

        public static Byte GetIndex(Byte[] _code)
        {
            if (!HasIndex(_code))
            {
                throw new InvalidOperationException(
                    "For ModRM that does not specify an index, usage of GetIndex is invalid."
                    );
            }

            UInt32 modRMIndex = opcode.GetOpcodeLength(_code);
            var modRM = getModRM(_code);
            var mod = getMod(modRM);

            switch (mod)
            {
                case 1:
                {
                    return _code[modRMIndex + 1];
                }

                default:
                {
                    throw new NotImplementedException(String.Format("Unsupported Mod: 0x{0:x2}", mod));
                }
            }
        }

        public static Boolean IsEffectiveAddressDereferenced(Byte[] _code)
        {
            var modRM = getModRM(_code);

            return !(getMod(modRM) == 3);
        }

        public static Boolean HasOffset(Byte[] _code)
        {
            if (HasSIB(_code))
            {
                return false;
            }

            var modRM = getModRM(_code);
            return GetRM(modRM) == 5 && getMod(modRM) == 0;
        }

        public static UInt32 GetOffset(Byte[] code)
        {
            var offsetBeginsAt = opcode.GetOpcodeLength(code);
            offsetBeginsAt++; // for modRM byte

            return BitMath.BytesToDword(code, offsetBeginsAt);
        }

        public static Boolean HasSIB(Byte[] _code)
        {
            var modRM = getModRM(_code);

            return GetRM(modRM) == 4 && IsEffectiveAddressDereferenced(_code);
        }

        public static Boolean IsEvDword(Byte[] code)
        {
            var modRM = getModRM(code);
            if ((getMod(modRM) == 0) && (GetRM(modRM) == 5))
            {
                return true;
            }

            return false;
        }

        private static Byte getModRM(Byte[] _code)
        {
            Int32 modRMIndex = opcode.GetOpcodeLength(_code);

            if (modRMIndex > _code.Length - 1)
            {
                throw new InvalidOperationException("No ModRM present: " + _code[0]);
            }

            return _code[modRMIndex];
        }

        private static Byte getMod(Byte modrm)
        {
            return (Byte) ((modrm >> 6) & 3);
        }
    }
}