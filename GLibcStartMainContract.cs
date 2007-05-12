// Copyright (c) 2006-2007 Luis Miras, Doug Coker, Todd Nagengast,
// Anthony Lineberry, Dan Moniz, Bryan Siepert, Mike Seery, Cullen Bryan
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;

namespace bugreport
{
public class GLibcStartMainContract : Contract
{
    Opcode opcode = new X86Opcode();

    public Boolean IsSatisfiedBy(MachineState state, Byte[] code)
    {
        UInt32 offset = opcode.GetImmediate(code);
        UInt32 effectiveAddress;

        unchecked
        {
            //FIXME: find a way to do this without an unchecked operation
            effectiveAddress = state.InstructionPointer + offset + (UInt32)code.Length;
        }

        const UInt32 GLIBC_START_MAIN_IMPORT_FUNCTION_ADDR = 0x80482b8;
        if (effectiveAddress == GLIBC_START_MAIN_IMPORT_FUNCTION_ADDR)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public MachineState Execute(MachineState state)
    {
        state.InstructionPointer = state.TopOfStack.Value; 
        return state;
    }
}
}