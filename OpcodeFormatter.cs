// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;

namespace bugreport
{
	public static class OpcodeFormatter
	{
		public static String GetInstructionName(Byte[] code)
		{
			String instructionName = String.Empty;
			
			if (OpcodeHelper.GetStackEffect(code) != StackEffect.None)
			{
				instructionName += OpcodeHelper.GetStackEffect(code).ToString().ToLower();
			}
			else
			{
				OperatorEffect effect = OpcodeHelper.GetOperatorEffect(code);
				if (OperatorEffect.Assignment == effect)
					instructionName += "mov";
				else if (OperatorEffect.None == effect)
					instructionName +="nop";
				else
					instructionName += effect.ToString().ToLower();
			}

			return instructionName;
		}

		public static String GetOperands(Byte[] code)
		{
			String operands = String.Empty;
						
			if (OpcodeHelper.IsRegisterOnlyOperand(code))
			{
				String encoding = OpcodeHelper.GetEncoding(code).ToString().ToLower();
				operands += "e" + encoding[1] + encoding[2];
			}
			
			if (OpcodeHelper.HasModRM(code) )
			{
				if (!ModRM.HasSIB(code))
				{
					Boolean evDereferenced = ModRM.IsEffectiveAddressDereferenced(code);
					if (evDereferenced)
						operands += "[";
		
					operands += ModRM.GetEv(code).ToString().ToLower();

					if (ModRM.HasIndex(code))
					{
						operands += "+" + ModRM.GetIndex(code);
					}
	
					if (evDereferenced)
						operands += "]";
				}
				else
				{
					operands += "[" + SIB.GetBaseRegister(code).ToString().ToLower() + "]";
				}
	
				operands += ", ";
			}
	
			if (OpcodeHelper.HasImmediate(code))
				operands += String.Format("0x{0:x}", OpcodeHelper.GetImmediate(code));
				
			return operands;
		}
	
		public static String GetEncoding(Byte[] code)
		{
			String encoding = String.Empty;
				
			if (OpcodeEncoding.None != OpcodeHelper.GetEncoding(code))
			{
				encoding += "(" + OpcodeHelper.GetEncoding(code) + ")";
			}
			
			return encoding;
		}
	}
}
