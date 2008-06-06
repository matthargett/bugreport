// Copyright (c) 2006-2008 Luis Miras, Doug Coker, Todd Nagengast,
// Anthony Lineberry, Dan Moniz, Bryan Siepert, Mike Seery, Cullen Bryan
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;

namespace bugreport
{
public class MallocContract : Contract
{
    public override Boolean IsSatisfiedBy(MachineState state, Byte[] code)
    {
        UInt32 effectiveAddress = opcode.GetEffectiveAddress(code, state.InstructionPointer);

        const UInt32 MALLOC_IMPORT_FUNCTION_ADDR = 0x80482a8;
        if (effectiveAddress == MALLOC_IMPORT_FUNCTION_ADDR)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override MachineState Execute(MachineState state)
    {
        AbstractValue[] buffer = AbstractValue.GetNewBuffer(state.TopOfStack.Value);
        state.ReturnValue = new AbstractValue(buffer);
        
        return state;
    }
}
}
