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
    static Opcode opcode = new X86Opcode();

    public static String GetInstructionName(Byte[] code)
    {
        String instructionName = String.Empty;

        if (opcode.GetStackEffect(code) != StackEffect.None)
        {
            instructionName += opcode.GetStackEffect(code).ToString().ToLower();
        }
        else
        {
            OperatorEffect effect = opcode.GetOperatorEffect(code);
            if (OperatorEffect.Assignment == effect)
            {
                if (code[0] == 0x8d)
                {
                    instructionName += "lea";
                }
                else
                {
                    instructionName += "mov";
                }
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

        if (opcode.HasDestinationRegister(code))
        {
            count++;
        }

        if (opcode.HasSourceRegister(code) ||
                opcode.HasImmediate(code) ||
                opcode.HasModRM(code))
        {
            count++;
        }

        return count;
    }

    private static String getSingleOperand(Byte[] code)
    {
        if (opcode.HasSourceRegister(code) && !opcode.HasDestinationRegister(code))
        {
            return opcode.GetSourceRegister(code).ToString().ToLower();
        }

        if (!opcode.HasSourceRegister(code) && opcode.HasDestinationRegister(code))
        {
            return opcode.GetDestinationRegister(code).ToString().ToLower();
        }

        if (opcode.HasImmediate(code))
        {
            return String.Format("0x{0:x8}", opcode.GetImmediate(code));
        }

        throw new ArgumentException("single operand requested when there is more than one");
    }

    private static String encaseInSquareBrackets(String toBeEncased)
    {
        return "[" + toBeEncased + "]";
    }

    private static String getSourceOperand(Byte[] code)
    {
        if (opcode.HasImmediate(code))
        {
            return String.Format("0x{0:x}", opcode.GetImmediate(code));
        }
        
        if (opcode.HasOffset(code) && opcode.HasModRM(code))
        {
            String offset = String.Format("0x{0:x}", ModRM.GetOffset(code));
            return encaseInSquareBrackets(offset);
        }

        String sourceOperand;
        Boolean sourceIsEffectiveAddress = opcode.GetEncoding(code).ToString().EndsWith("Ev") ||
                        opcode.GetEncoding(code).ToString().EndsWith("Eb") ||
                        opcode.GetEncoding(code).ToString().EndsWith("M");

        if (sourceIsEffectiveAddress && ModRM.HasSIB(code))
        {
            String baseRegister = SIB.GetBaseRegister(code).ToString().ToLower();
            String scaled = SIB.GetScaledRegister(code).ToString().ToLower();
            UInt32 scaler = SIB.GetScaler(code);

            if (SIB.GetScaledRegister(code) == RegisterName.None)
            {
                sourceOperand = baseRegister;
            }
            else if (scaler == 1)
            {
                sourceOperand = baseRegister + "+" + scaled;
            }
            else
            {
                sourceOperand = baseRegister + "+" + scaled + "*" + scaler;
            }

            sourceOperand =  encaseInSquareBrackets(sourceOperand);
            return sourceOperand;
        }

        if (opcode.HasSourceRegister(code))
        {
            sourceOperand = opcode.GetSourceRegister(code).ToString().ToLower();

            if (opcode.HasModRM(code) && sourceIsEffectiveAddress)
            {
                if (ModRM.HasIndex(code))
                {
                    sourceOperand += "+" + ModRM.GetIndex(code);
                }
    
                Boolean evDereferenced = ModRM.IsEffectiveAddressDereferenced(code);
                if (evDereferenced)
                {
                    sourceOperand = encaseInSquareBrackets(sourceOperand);
                }                
            }
            
            return sourceOperand;

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
        String destinationOperand = opcode.GetDestinationRegister(code).ToString().ToLower();
        Boolean destinationIsEffectiveAddress = opcode.GetEncoding(code).ToString().StartsWith("Ev") ||
                                                opcode.GetEncoding(code).ToString().StartsWith("Eb");

        if (destinationIsEffectiveAddress && ModRM.HasSIB(code))
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
        else if (opcode.HasModRM(code) && destinationIsEffectiveAddress)
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

        switch (operandCount)
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

        if (OpcodeEncoding.None != opcode.GetEncoding(code))
        {
            encoding += "(" + opcode.GetEncoding(code) + ")";
        }

        return encoding;
    }
}
}

