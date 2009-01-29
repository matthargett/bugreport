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
        Jump,
        Cmp,
        Jnz,
        Call,
    }

    public enum OpcodeEncoding
    {
        Unknown,
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
        OpcodeEncoding GetEncoding(Byte[] code);
        
        OperatorEffect GetOperatorEffect(Byte[] code);
        
        StackEffect GetStackEffect(Byte[] code);

        Boolean HasModRM(Byte[] code);
        
        Boolean HasOffset(Byte[] code);

        Boolean HasImmediate(Byte[] code);
        
        UInt32 GetImmediate(Byte[] code);

        Byte GetInstructionLength(Byte[] code);
        
        Byte GetInstructionLength(Byte[] code, UInt32 index);
        
        Byte GetOpcodeLength(Byte[] code);

        RegisterName GetSourceRegister(Byte[] code);
        
        RegisterName GetDestinationRegister(Byte[] code);
        
        Boolean HasSourceRegister(Byte[] code);
        
        Boolean HasDestinationRegister(Byte[] code);

        Boolean TerminatesFunction(Byte[] code);
        
        UInt32 GetEffectiveAddress(Byte[] code, UInt32 instructionPointer);
    }
}