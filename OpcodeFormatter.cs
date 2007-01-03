// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;

namespace bugreport
{
	public class OpcodeFormatter
	{
		public static String getInstructionName(Byte[] code)
		{
			String InstructionName = String.Empty;
			
			if (OpcodeHelper.GetStackEffect(code) != StackEffect.None)
			{
				InstructionName += OpcodeHelper.GetStackEffect(code).ToString().ToLower();
			}
			else
			{
				OperatorEffect effect = OpcodeHelper.GetOperatorEffect(code);
				if (OperatorEffect.Assignment == effect)
					InstructionName += "mov";
				else if (OperatorEffect.None == effect)
					InstructionName +="nop";
				else
					InstructionName += effect.ToString().ToLower();
			}

			InstructionName += "\t";
			
			return InstructionName;
		}

		public static String getOperands(Byte[] code)
		{
			String Operands = String.Empty;
			
			if (OpcodeHelper.HasModRM(code) )
			{
				if (!ModRM.HasSIB(code))
				{
					Boolean evDereferenced = ModRM.IsEffectiveAddressDereferenced(code);
					if (evDereferenced)
						Operands += "[";
		
					Operands += ModRM.GetEv(code).ToString().ToLower();
	
					if (evDereferenced)
						Operands += "]";
				}
				else
				{
					Operands += "[" + SIB.GetBaseRegister(code).ToString().ToLower() + "]";
				}
	
				Operands += ", ";
			}
	
			if (OpcodeHelper.HasImmediate(code))
				Operands += String.Format("0x{0:x}", OpcodeHelper.GetImmediate(code));
				
			return Operands;
		}
	
		public static String getEncoding(Byte[] code)
		{
			String Encoding = String.Empty;
				
			Encoding += "\t";
			if (OpcodeEncoding.None != OpcodeHelper.GetEncoding(code))
				Encoding += "\t(" + OpcodeHelper.GetEncoding(code) + ")";
			else
				Encoding += "\n";
			
			return Encoding;
			}
	
			
		}
}
