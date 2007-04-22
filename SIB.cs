// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 3
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
			return code[Opcode.GetOpcodeLength(code) + 1];
		}
		
		public static RegisterName GetBaseRegister(Byte[] code)
		{
			if (!ModRM.HasSIB(code))
			{
				throw new InvalidOperationException("For ModRM that does not specify a SIB, usage of GetBaseRegister is invalid.");
			}

			Byte sib = getSIB(code);
			RegisterName register = (RegisterName)(sib & 7);
			
			return register;
		}
		
		public static RegisterName GetScaledRegister(Byte[] code)
		{
			if (!ModRM.HasSIB(code))
			{
				throw new InvalidOperationException("For ModRM that does not specify a SIB, usage of GetBaseRegister is invalid.");
			}

			RegisterName register = (RegisterName) getIndex(code);
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
				throw new InvalidOperationException("For ModRM that does not specify a SIB, usage of GetBaseRegister is invalid.");
			}

			Byte S = (Byte)((getSIB(code) >> 6) & 3);
			return (UInt32)Math.Pow(2, S);
		}
	}
}
