// Copyright (c) 2006-2007 Luis Miras, Doug Coker, Todd Nagengast,
// Anthony Lineberry, Dan Moniz, Bryan Siepert, Mike Seery, Cullen Bryan
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;
using System.Text;

namespace bugreport
{
	public static class OpcodeFormatter
	{
		public static String GetInstructionName(Byte[] code)
		{
			String instructionName = String.Empty;
			
			if (Opcode.GetStackEffect(code) != StackEffect.None)
			{
				instructionName += Opcode.GetStackEffect(code).ToString().ToLower();
			}
			else
			{
				OperatorEffect effect = Opcode.GetOperatorEffect(code);
				if (OperatorEffect.Assignment == effect)
				{
					instructionName += "mov";
				}
				else if (OperatorEffect.None == effect)
				{
					instructionName +="nop";
				}
				else
				{
					instructionName += effect.ToString().ToLower();
				}
			}

			return instructionName;
		}
		
		private static UInt32 getOperandCount(Byte[] code)
		{
			UInt32 count = 0;
			
			if (Opcode.HasDestinationRegister(code)) 
			{
				count++;
			}
			
			if (Opcode.HasSourceRegister(code) ||
				Opcode.HasImmediate(code) ||
				Opcode.HasModRM(code)) 
			{
				count++;
			}
			
			return count;
		}
		
		private static String getSingleOperand(Byte[] code)
		{
			if (Opcode.HasSourceRegister(code) && !Opcode.HasDestinationRegister(code))
			{
				return Opcode.GetSourceRegister(code).ToString().ToLower();
			}
			
			if (!Opcode.HasSourceRegister(code) && Opcode.HasDestinationRegister(code))
			{
				return Opcode.GetDestinationRegister(code).ToString().ToLower();
			}

			if (Opcode.HasImmediate(code))
			{
				return String.Format("0x{0:x8}", Opcode.GetImmediate(code));
			}
			
			throw new ArgumentException("single operand requested when there is more than one");
		}
		
		private static String encaseInSquareBrackets(String toBeEncased)
		{
			return "[" + toBeEncased + "]";
		}
		
		private static String getSourceOperand(Byte[] code)
		{
			if (Opcode.HasImmediate(code))
			{
				return String.Format("0x{0:x}", Opcode.GetImmediate(code));
			}
			else if (Opcode.HasSourceRegister(code))
			{
				return Opcode.GetSourceRegister(code).ToString().ToLower();
			}
			
			throw new ArgumentException(
				"don't know how to render this code's source operand: " + formatCode(code)
			);
		}
		
		private static String formatCode(Byte[] code)
		{
			StringBuilder formattedCode = new StringBuilder();
			foreach (Byte codeByte in code)
			{
				formattedCode.Append(String.Format("{0:x2} ", codeByte));
			}
			
			return formattedCode.ToString();
		}

		private static String getDestinationOperand(Byte[] code)
		{
			String destinationOperand = Opcode.GetDestinationRegister(code).ToString().ToLower();
			
			if (ModRM.HasSIB(code))
			{
				String baseRegister = SIB.GetBaseRegister(code).ToString().ToLower();
				String scaled = SIB.GetScaledRegister(code).ToString().ToLower();
				UInt32 scaler = SIB.GetScaler(code);
				
				if (SIB.GetScaledRegister(code) == RegisterName.None)
				{
					destinationOperand = baseRegister;
				}
				else if (scaler == 1)
				{
					destinationOperand = baseRegister + "+" + scaled;	
				}
				else
				{
					destinationOperand = baseRegister + "+" + scaled + "*" + scaler;
				}
				
				destinationOperand = encaseInSquareBrackets(destinationOperand);
			}
			else if (Opcode.HasModRM(code))
			{
				if (ModRM.HasIndex(code))
				{
					destinationOperand += "+" + ModRM.GetIndex(code);
				}

				Boolean evDereferenced = ModRM.IsEffectiveAddressDereferenced(code);
				if (evDereferenced)
				{
					destinationOperand = encaseInSquareBrackets(destinationOperand);
				}
			}
			
			return destinationOperand;
		}
		
		public static String GetOperands(Byte[] code)
		{
			UInt32 operandCount = getOperandCount(code);
			
			switch(operandCount)
			{
				case 0:
				{
					return String.Empty;
				}
			
				case 1:
				{
					return getSingleOperand(code);
				}
			
				case 2:
				{
					String destinationOperand = getDestinationOperand(code);
					String sourceOperand = getSourceOperand(code);
						
					return destinationOperand + ", " + sourceOperand;
				}
					
				default:
				{
					throw new InvalidOperationException("don't know how to display " + operandCount + " operands");
				}
			}
		}
	
		public static String GetEncoding(Byte[] code)
		{
			String encoding = String.Empty;
				
			if (OpcodeEncoding.None != Opcode.GetEncoding(code))
			{
				encoding += "(" + Opcode.GetEncoding(code) + ")";
			}
			
			return encoding;
		}
	}
}

