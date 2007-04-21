// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert,
// Cullen Bryan, Mike Seery
// Licensed under GPLv3 draft 3
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
			String destinationRegister = String.Empty;
			
			if (OpcodeHelper.HasSourceRegister(code) && !OpcodeHelper.HasDestinationRegister(code))
			{
				return OpcodeHelper.GetSourceRegister(code).ToString().ToLower();
			}
			
			if (OpcodeHelper.HasDestinationRegister(code))
			{
				destinationRegister = OpcodeHelper.GetDestinationRegister(code).ToString().ToLower();
			}
			
			if (OpcodeHelper.HasModRM(code))
			{
				if (!ModRM.HasSIB(code))
				{
					Boolean evDereferenced = ModRM.IsEffectiveAddressDereferenced(code);
					if (evDereferenced)
					{
						operands += "[";
					}

					operands += destinationRegister;

					if (ModRM.HasIndex(code))
					{
						operands += "+" + ModRM.GetIndex(code);
					}
	
					if (evDereferenced)
					{
						operands += "]";
					}
				}
				else
				{
					operands += "[" + SIB.GetBaseRegister(code).ToString().ToLower();
					String scaled = SIB.GetScaledRegister(code).ToString().ToLower() +
						"*" + SIB.GetScaler(code);
					if (!(SIB.GetScaler(code) == 1 && SIB.GetScaledRegister(code) == RegisterName.None))
					{
						operands += "+" + scaled;
					}
					
					operands += "]";
				}
			}
			else
			{
				operands += destinationRegister;
			}

			if (OpcodeHelper.HasImmediate(code))
			{
				if (OpcodeHelper.HasDestinationRegister(code))
				{
					operands += ", ";
				}
				
				operands += String.Format("0x{0:x}", OpcodeHelper.GetImmediate(code));
			}
			else if (OpcodeHelper.HasSourceRegister(code) && OpcodeHelper.HasDestinationRegister(code))
			{
				operands += String.Format(", {0}", OpcodeHelper.GetSourceRegister(code).ToString().ToLower());
			}
				
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
