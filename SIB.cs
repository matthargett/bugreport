// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;

namespace bugreport
{

	public static class SIB
	{
		private static Byte getIndex(Byte[] _code)
		{
			return (Byte)((getSIB(_code) >> 3) & 7);
		}
		
		private static Byte getSIB(Byte[] _code)
		{
			return _code[OpcodeHelper.GetOpcodeLength(_code) + 1];
		}
		
		public static RegisterName GetBaseRegister(Byte[] _code)
		{
			if (!ModRM.HasSIB(_code))
			    throw new InvalidOperationException("For ModRM that does not specify a SIB, usage of GetBaseRegister is invalid.");

			if (getIndex(_code) != 0x4)
				throw new NotImplementedException("GetBaseRegister only supports scaler of none.");
			Byte sib = getSIB(_code);
			return (RegisterName)(sib & 7);
		}
	}
}
