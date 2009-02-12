// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;

namespace bugreport
{
    public static class SIB
    {
        private static readonly Opcode opcode = new X86Opcode();

        public static RegisterName GetScaledRegister(Byte[] code)
        {
            if (!ModRM.HasSIB(code))
            {
                throw new InvalidOperationException(
                    "For ModRM that does not specify a SIB, usage of GetBaseRegister is invalid.");
            }

            var register = (RegisterName) getIndex(code);
            if (register == RegisterName.ESP)
            {
                register = RegisterName.None;
            }

            return register;
        }

        public static UInt32 GetScaler(Byte[] code)
        {
            if (!ModRM.HasSIB(code))
            {
                throw new InvalidOperationException(
                    "For ModRM that does not specify a SIB, usage of GetBaseRegister is invalid.");
            }

            var s = (Byte) ((getSIB(code) >> 6) & 3);
            return (UInt32) Math.Pow(2, s);
        }

        public static RegisterName GetBaseRegister(Byte[] code)
        {
            if (!ModRM.HasSIB(code))
            {
                throw new InvalidOperationException(
                    "For ModRM that does not specify a SIB, usage of GetBaseRegister is invalid.");
            }

            var sib = getSIB(code);
            var register = (RegisterName) (sib & 7);

            return register;
        }

        private static Byte getIndex(Byte[] code)
        {
            return (Byte) ((getSIB(code) >> 3) & 7);
        }

        private static Byte getSIB(Byte[] code)
        {
            return code[opcode.GetOpcodeLength(code) + 1];
        }
    }
}