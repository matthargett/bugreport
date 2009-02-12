// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace bugreport
{
    public static class OpcodeFormatter
    {
        private static readonly Opcode opcode = new X86Opcode();

        public static String GetInstructionName(Byte[] code)
        {
            var instructionName = String.Empty;

            var stackEffect = opcode.GetStackEffect(code);

            if (stackEffect != StackEffect.None)
            {
                instructionName += stackEffect.ToString().ToLower();
            }
            else
            {
                var effect = opcode.GetOperatorEffect(code);
                switch (effect)
                {
                    case OperatorEffect.Assignment:
                        if (code[0] == 0x8d)
                        {
                            instructionName += "lea";
                        }
                        else
                        {
                            instructionName += "mov";
                        }
                        break;
                    case OperatorEffect.None:
                        instructionName += "nop";
                        break;
                    default:
                        instructionName += effect.ToString().ToLower();
                        break;
                }
            }

            return instructionName;
        }

        public static String GetOperands(Byte[] code, UInt32 instructionPointer)
        {
            var operandCount = getOperandCount(code);

            switch (operandCount)
            {
                case 0:
                {
                    return String.Empty;
                }

                case 1:
                {
                    return getSingleOperand(code, instructionPointer);
                }

                case 2:
                {
                    var destinationOperand = getDestinationOperand(code);
                    var sourceOperand = getSourceOperand(code);

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
            var encoding = String.Empty;

            if (OpcodeEncoding.None != opcode.GetEncoding(code))
            {
                encoding += "(" + opcode.GetEncoding(code) + ")";
            }

            return encoding;
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

        private static String getSingleOperand(Byte[] code, UInt32 instructionPointer)
        {
            if (opcode.HasSourceRegister(code) && !opcode.HasDestinationRegister(code))
            {
                return opcode.GetSourceRegister(code).ToString().ToLower();
            }

            if (!opcode.HasSourceRegister(code) && opcode.HasDestinationRegister(code))
            {
                return opcode.GetDestinationRegister(code).ToString().ToLower();
            }

            if (opcode.GetOperatorEffect(code) == OperatorEffect.Call)
            {
                return String.Format("0x{0:x8}", opcode.GetEffectiveAddress(code, instructionPointer));
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
                var offset = String.Format("0x{0:x}", ModRM.GetOffset(code));
                return encaseInSquareBrackets(offset);
            }

            if (SourceIsEffectiveAddress(code) && ModRM.HasSIB(code))
            {
                return encaseInSquareBrackets(getSIB(code));
            }

            if (opcode.HasSourceRegister(code))
            {
                return getSourceRegister(code);
            }

            throw new ArgumentException(
                "don't know how to format this code's source operand: " + formatCode(code)
                );
        }

        private static string getSourceRegister(byte[] code)
        {
            var sourceOperand = opcode.GetSourceRegister(code).ToString().ToLower();

            if (SourceIsEffectiveAddress(code) && opcode.HasModRM(code))
            {
                if (ModRM.HasIndex(code))
                {
                    sourceOperand += "+" + ModRM.GetIndex(code);
                }

                var effectiveAddressIsDereferenced = ModRM.IsEffectiveAddressDereferenced(code);
                if (effectiveAddressIsDereferenced)
                {
                    sourceOperand = encaseInSquareBrackets(sourceOperand);
                }
            }
            return sourceOperand;
        }

        private static string getSIB(byte[] code)
        {
            string sourceOperand;
            var baseRegister = SIB.GetBaseRegister(code).ToString().ToLower();
            var scaled = SIB.GetScaledRegister(code).ToString().ToLower();
            var scaler = SIB.GetScaler(code);

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
            return sourceOperand;
        }

        private static bool SourceIsEffectiveAddress(byte[] code)
        {
            return opcode.GetEncoding(code).ToString().EndsWith("Ev", StringComparison.Ordinal) ||
                   opcode.GetEncoding(code).ToString().EndsWith("Eb", StringComparison.Ordinal) ||
                   opcode.GetEncoding(code).ToString().EndsWith("M", StringComparison.Ordinal);
        }

        private static String formatCode(IEnumerable<byte> code)
        {
            var formattedCode = new StringBuilder();
            foreach (var codeByte in code)
            {
                formattedCode.Append(String.Format("{0:x2} ", codeByte));
            }

            return formattedCode.ToString();
        }

        private static String getDestinationOperand(Byte[] code)
        {
            var destinationOperand = opcode.GetDestinationRegister(code).ToString().ToLower();
            var destinationIsEffectiveAddress =
                opcode.GetEncoding(code).ToString().StartsWith("Ev", StringComparison.Ordinal) ||
                opcode.GetEncoding(code).ToString().StartsWith("Eb", StringComparison.Ordinal);

            if (destinationIsEffectiveAddress && ModRM.HasSIB(code))
            {
                var baseRegister = SIB.GetBaseRegister(code).ToString().ToLower();
                var scaled = SIB.GetScaledRegister(code).ToString().ToLower();
                var scaler = SIB.GetScaler(code);

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

                var effectiveAddressIsDereferenced = ModRM.IsEffectiveAddressDereferenced(code);
                if (effectiveAddressIsDereferenced)
                {
                    destinationOperand = encaseInSquareBrackets(destinationOperand);
                }
            }

            return destinationOperand;
        }
    }
}