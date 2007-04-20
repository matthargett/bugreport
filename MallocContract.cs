// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert, Cullen Bryan 
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;

namespace bugreport
{
	/// <summary>
	/// Description of MallocContract.
	/// </summary>
	public class MallocContract
	{
		public MallocContract()
		{
		}		
		
		public Boolean IsSatisfiedBy(MachineState state, Byte[] code)
		{
			UInt32 offset = OpcodeHelper.GetImmediate(code);
			UInt32 effectiveAddress;
			
			unchecked{
				effectiveAddress = state.InstructionPointer + offset + (UInt32)code.Length;
			}
					
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
		
		public MachineState Execute(MachineState state, Byte[] code)
		{			
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(state.TopOfStack.Value); 
			state.ReturnValue = new AbstractValue(buffer);			
			return state;
		}		
	}
}
