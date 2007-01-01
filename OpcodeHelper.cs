// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;

namespace bugreport
{
	public enum StackEffect {None, Push, Pop};
	
	public enum OperatorEffect {Unknown, Assignment, Add, Sub, And, Shr, Shl, Cmp, Jnz, None, Return, Leave, Jump};

	public enum OpcodeEncoding {None, EvGv, GvEv, rAxIv, rAxIz, rAxOv, EvIz, EbIb, Jz, rBP, rBX, GvEb, EbGb, ObAL, EvIb, GvM, Jb};
	
	/// <summary>
	/// Based on table at http://sandpile.org/ia32/opc_1.htm
	/// </summary>
	public static class OpcodeHelper
	{ 
		public static OpcodeEncoding GetEncoding(Byte[] _code)
		{
			switch (_code[0])
			{
				case 0xc9:
				case 0xc3:
				case 0x90:
					return OpcodeEncoding.None;
				case 0x05:
					return OpcodeEncoding.rAxIz;
				case 0x0f:
					return OpcodeEncoding.GvEb;
				case 0x53:
				case 0x5b:
					return OpcodeEncoding.rBX;
				case 0x5d:
				case 0x55:
					return OpcodeEncoding.rBP;
				case 0x75:
					return OpcodeEncoding.Jb;
				case 0x83:
				case 0xc1:
					return OpcodeEncoding.EvIb;
				case 0x88:
					return OpcodeEncoding.EbGb;
				case 0x29:
				case 0x89:
					return OpcodeEncoding.EvGv;
				case 0x8b:
					return OpcodeEncoding.GvEv;
				case 0x8d:
					return OpcodeEncoding.GvM;
				case 0xa2:
					return OpcodeEncoding.ObAL;
				case 0xb8:
					return OpcodeEncoding.rAxIv;
				case 0xa1:
					return OpcodeEncoding.rAxOv;
				case 0xc6:
					return OpcodeEncoding.EbIb;
				case 0xc7:
					return OpcodeEncoding.EvIz;
				case 0xe8:
					return OpcodeEncoding.Jz;
				default:
					throw new InvalidOpcodeException(_code);
			}			
		}

		public static Boolean HasModRM(Byte[] _code)
		{
			switch (GetEncoding(_code))
			{
				case OpcodeEncoding.GvEb:
				case OpcodeEncoding.GvEv:
				case OpcodeEncoding.EvGv:
				case OpcodeEncoding.EvIb:
				case OpcodeEncoding.EbIb:
				case OpcodeEncoding.EbGb:
				case OpcodeEncoding.EvIz:
					return true;
				default:
					return false;
			}
		}

		public static Boolean HasImmediate(Byte[] _code)
		{
			switch (GetEncoding(_code))
			{
				case OpcodeEncoding.EvIb:
				case OpcodeEncoding.EbIb:
				case OpcodeEncoding.Jb:
				case OpcodeEncoding.EvIz:
				case OpcodeEncoding.rAxIz:
				case OpcodeEncoding.rAxIv:
				case OpcodeEncoding.Jz:
					return true;
				default:
					return false;
			}
		}

		public static UInt32 GetImmediate(Byte[] _code)
		{
			if (!HasImmediate(_code))
				throw new InvalidOperationException("Can't get immediate from an opcode that doesn't have one");

			Byte valueIndex = 1;
			if (HasModRM(_code))
			{
				valueIndex++;

				if (ModRM.HasIndex(_code))
				{
					valueIndex++;
				}

				if (ModRM.HasSIB(_code))
				{
					valueIndex++;
				}
			}

			switch (GetEncoding(_code))
			{
				case OpcodeEncoding.EvIb:
				case OpcodeEncoding.EbIb:
				case OpcodeEncoding.Jb:
				{
					
					return _code[valueIndex];
				}
				case OpcodeEncoding.EvIz:
				case OpcodeEncoding.rAxIz:
				case OpcodeEncoding.rAxIv:
				case OpcodeEncoding.Jz:
					return BitMath.BytesToDword(_code, valueIndex);

				default:
					throw new InvalidOpcodeException("Don't know how to get the immediate for this opcode: ", _code);
			}
		}
		
		public static StackEffect GetStackEffect(Byte[] _code)
		{
		
			switch(_code[0])
			{
				case 0x53:
				case 0x55:
					return StackEffect.Push;
				case 0x5b:
				case 0x5d:
					return StackEffect.Pop;

				default:
					return StackEffect.None;
			}
		}
		
		public static OperatorEffect GetOperatorEffect(Byte[] _code)
		{

			switch(_code[0])    	
			{
				case 0x05:
					return OperatorEffect.Add;
					
				case 0x29:
					return OperatorEffect.Sub;
					
				case 0x75:
					return OperatorEffect.Jnz;
				
				case 0x83:
				{
					Byte rm = ModRM.GetOpcodeGroupIndex(_code);
					
					switch (rm) 
					{
						case 0:
							return OperatorEffect.Add;
						case 4:
							return OperatorEffect.And;
						case 5:
							return OperatorEffect.Sub;
						case 7:
							return OperatorEffect.Cmp;
						default:
							return OperatorEffect.Unknown;
					}
				}
				
				case 0xc9:
					return OperatorEffect.Leave;
	
				case 0xc3:
					return OperatorEffect.Return;
	
				case 0x90:
					return OperatorEffect.None;
	
				case 0xc1:
				{
					Byte rm = ModRM.GetOpcodeGroupIndex(_code);
					
					switch (rm) 
					{
						case 4:
							return OperatorEffect.Shl;
						case 5:
							return OperatorEffect.Shr;
						default:
							return OperatorEffect.Unknown;
					}
				}

				case 0xe8:
					return OperatorEffect.Jump;
				
				default:
					return OperatorEffect.Assignment;
			}
		}
		
		public static Byte GetOpcodeLength(Byte[] _code)
		{
			Byte opcodeLength = 1;
			if (_code[0] == 0x0f)
				opcodeLength++;

			return opcodeLength;
		}
	}
}
