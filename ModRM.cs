// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;

namespace bugreport
{

	public static class ModRM
	{
		
		private static Byte getMod(Byte modrm)
		{
			return (Byte)((modrm >> 6) & 3);
		}
		
		public static Byte GetRM(Byte modrm)
		{
			return (Byte)(modrm & 7);
		}
		public static RegisterName GetEv(Byte[] code)
		{
			if (HasSIB(code))
			{
				throw new InvalidOperationException("For ModRM that specifies SIB byte, usage of GetEv is invalid.");
			}
			
			Byte modRM = getModRM(code);
			return (RegisterName)(modRM & 7);
		}

		public static RegisterName GetGv(Byte[] code)
		{
			Byte modRM = getModRM(code);
			return (RegisterName)((modRM >> 3) & 7);
		}
		
		public static Byte GetOpcodeGroupIndex(Byte[] _code)
		{
			Byte modRM = getModRM(_code);
			return (Byte)((modRM >> 3) & 7);
		}
		
		public static Boolean HasIndex(Byte[] _code)
		{
			Byte modRM = getModRM(_code);
			Byte mod = getMod(modRM);
			return (mod == 1 || mod == 2);
		}
		
		public static Byte GetIndex(Byte[] _code)
		{
			if (!HasIndex(_code))
				throw new InvalidOperationException("For ModRM that does not specify an index, usage of GetIndex is invalid.");

			UInt32 modRMIndex = OpcodeHelper.GetOpcodeLength(_code);
			Byte modRM = getModRM(_code);
			Byte mod = getMod(modRM);

			switch (mod)
			{
				case 1:
					{
						return _code[modRMIndex+1];
					}
				default:
					{
						throw new NotImplementedException(String.Format("Unsupported Mod: 0x{0:x2}", mod));
					}
			}
		}
		
		public static Boolean IsEffectiveAddressDereferenced(Byte[] _code)
		{
			Byte modRM = getModRM(_code);

			return !(getMod(modRM) == 3);
		}
		
		public static Boolean HasOffset(Byte[] _code)
		{
			if (HasSIB(_code))
			{
				return false;
			}

			Byte modRM = getModRM(_code);
			return (GetRM(modRM) == 5 && getMod(modRM) == 0);
		}
		
		public static UInt32 GetOffset(Byte[] code)
		{
			Byte offsetBeginsAt = OpcodeHelper.GetOpcodeLength(code);
			offsetBeginsAt++; // for modRM byte
			
			if (HasSIB(code))
			{
			    offsetBeginsAt++;
			}
			
			return BitMath.BytesToDword(code, offsetBeginsAt);
		}

		public static Boolean HasSIB(Byte[] _code)
		{
			Byte modRM = getModRM(_code);
			
			return ((GetRM(modRM) == 4) && (IsEffectiveAddressDereferenced(_code)));
		}

		public static Boolean IsEvDword(Byte[] _code)
		{
			Byte modRM = getModRM(_code);
			if (((modRM >> 6) == 0) && ((modRM & 7) == 5))
				return true;
			else 
				return false;
		}
		
		private static Byte getModRM(Byte[] _code)
		{
			Int32 modRMIndex = OpcodeHelper.GetOpcodeLength(_code);
			
			if (modRMIndex > _code.Length - 1)
				throw new InvalidOperationException("No ModRM present: " + _code[0].ToString());
			
			return _code[modRMIndex];
		}
	}
}
