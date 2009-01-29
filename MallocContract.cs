// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
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