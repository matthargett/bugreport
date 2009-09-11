// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;

namespace bugreport
{
    public enum StackEffect
    {
        None,
        Push,
        Pop
    }

    public enum OperatorEffect
    {
        Unknown,
        Assignment,
        Add,
        Sub,
        None,
        And,
        Shr,
        Shl,
        Xor,
        Return,
        Leave,
        Cmp,
        Jnz,
        Call,
    }

    public enum OpcodeEncoding
    {
        None,
        EvGv,
        EvIb,
        EbGb,
        EvIz,
        EbIb,
        GvEv,
        GvEb,
        Iz,
        GvM,
        rAxIv,
        rAxIz,
        rAxOv,
        ObAL,
        rAX,
        rBX,
        rCX,
        rDX,
        rSI,
        rSP,
        rBP,
        Jz,
        Jb,
        Int3
    }

    public interface Opcode
    {
        OpcodeEncoding GetEncodingFor(Byte[] code);

        OperatorEffect GetOperatorEffectFor(Byte[] code);

        StackEffect GetStackEffectFor(Byte[] code);

        Boolean HasModRM(Byte[] code);

        Boolean HasOffset(Byte[] code);

        Boolean HasImmediate(Byte[] code);

        UInt32 GetImmediateFor(Byte[] code);

        Byte GetInstructionLengthFor(Byte[] code);

        Byte GetInstructionLengthFor(Byte[] code, UInt32 index);

        Byte GetOpcodeLengthFor(Byte[] code);

        RegisterName GetSourceRegisterFor(Byte[] code);

        RegisterName GetDestinationRegisterFor(Byte[] code);

        Boolean HasSourceRegister(Byte[] code);

        Boolean HasDestinationRegister(Byte[] code);

        Boolean TerminatesFunction(Byte[] code);

        UInt32 GetEffectiveAddress(Byte[] code, UInt32 instructionPointer);
    }
}