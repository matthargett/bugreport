// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;

namespace bugreport
{

	public static class SIB
	{
		private static Byte getIndex(Byte[] code)
		{
			return (Byte)((getSIB(code) >> 3) & 7);
		}
		
		private static Byte getSIB(Byte[] code)
		{
			return code[OpcodeHelper.GetOpcodeLength(code) + 1];
		}
		
		public static RegisterName GetBaseRegister(Byte[] code)
		{
			if (!ModRM.HasSIB(code))
			{
				throw new InvalidOperationException("For ModRM that does not specify a SIB, usage of GetBaseRegister is invalid.");
			}

			if (getIndex(code) != 0x4)
			{
				throw new NotImplementedException("GetBaseRegister only supports scaler of none.");
			}
			
			Byte sib = getSIB(code);
			RegisterName register = (RegisterName)(sib & 7);
			
			if (RegisterName.ESP != register)
			{
				// TODO: this check can be removed once tests are in place
				throw new NotImplementedException("SIB currently only supports ESP, this register was attempted: " + register);
			}
			return register;
		}
	}
}
