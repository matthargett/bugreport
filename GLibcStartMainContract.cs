// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;

namespace bugreport
{
    public class GLibcStartMainContract : Contract
    {
        public override Boolean IsSatisfiedBy(MachineState state, Byte[] code)
        {
            UInt32 effectiveAddress = opcode.GetEffectiveAddress(code, state.InstructionPointer);
            
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

        public override MachineState Execute(MachineState state)
        {
            state.InstructionPointer = state.TopOfStack.Value; 
            return state;
        }
    }
}
