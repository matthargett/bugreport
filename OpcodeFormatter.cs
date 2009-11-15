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

            var stackEffect = opcode.GetStackEffectFor(code);

            if (stackEffect != StackEffect.None)
            {
                instructionName += stackEffect.ToString().ToLower();
            }
            else
            {
                var effect = opcode.GetOperatorEffectFor(code);
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
            var operandCount = GetOperandCountFor(code);

            switch (operandCount)
            {
                case 0:
                {
                    return String.Empty;
                }

                case 1:
                {
                    return GetSingleOperandFor(code, instructionPointer);
                }

                case 2:
                {
                    var destinationOperand = GetDestinationOperandFor(code);
                    var sourceOperand = GetSourceOperandFor(code);

                    return destinationOperand + ", " + sourceOperand;
                }

                default:
                {
                    throw new InvalidOperationException("don't know how to display " + operandCount + " operands");
                }
            }
        }

        public static String GetEncodingFor(Byte[] code)
        {
            var encoding = String.Empty;

            if (OpcodeEncoding.None != opcode.GetEncodingFor(code))
            {
                encoding += "(" + opcode.GetEncodingFor(code) + ")";
            }

            return encoding;
        }

        private static UInt32 GetOperandCountFor(Byte[] code)
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

        private static String GetSingleOperandFor(Byte[] code, UInt32 instructionPointer)
        {
            if (opcode.HasSourceRegister(code) && !opcode.HasDestinationRegister(code))
            {
                return opcode.GetSourceRegisterFor(code).ToString().ToLower();
            }

            if (!opcode.HasSourceRegister(code) && opcode.HasDestinationRegister(code))
            {
                return opcode.GetDestinationRegisterFor(code).ToString().ToLower();
            }

            if (opcode.GetOperatorEffectFor(code) == OperatorEffect.Call)
            {
                return String.Format("0x{0:x8}", opcode.GetEffectiveAddress(code, instructionPointer));
            }

            if (opcode.HasImmediate(code))
            {
                return String.Format("0x{0:x8}", opcode.GetImmediateFor(code));
            }

            throw new ArgumentException("single operand requested when there is more than one");
        }

        private static String Encase(String content)
        {
            return "[" + content + "]";
        }

        private static String GetSourceOperandFor(Byte[] code)
        {
            if (opcode.HasImmediate(code))
            {
                return String.Format("0x{0:x}", opcode.GetImmediateFor(code));
            }

            if (opcode.HasOffset(code) && opcode.HasModRM(code))
            {
                var offset = String.Format("0x{0:x}", ModRM.GetOffsetFor(code));
                return Encase(offset);
            }

            if (SourceIsEffectiveAddress(code) && ModRM.HasSIB(code))
            {
                return Encase(GetSIBFor(code));
            }

            if (opcode.HasSourceRegister(code))
            {
                return GetSourceRegisterFor(code);
            }

            throw new ArgumentException(
                "don't know how to format this code's source operand: " + GetBytesFor(code)
                );
        }

        private static string GetSourceRegisterFor(byte[] code)
        {
            var sourceOperand = opcode.GetSourceRegisterFor(code).ToString().ToLower();

            if (SourceIsEffectiveAddress(code) && opcode.HasModRM(code))
            {
                if (ModRM.HasIndex(code))
                {
                    sourceOperand += "+" + ModRM.GetIndexFor(code);
                }

                var effectiveAddressIsDereferenced = ModRM.IsEffectiveAddressDereferenced(code);
                if (effectiveAddressIsDereferenced)
                {
                    sourceOperand = Encase(sourceOperand);
                }
            }

            return sourceOperand;
        }

        private static string GetSIBFor(byte[] code)
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
            return opcode.GetEncodingFor(code).ToString().EndsWith("Ev", StringComparison.Ordinal) ||
                   opcode.GetEncodingFor(code).ToString().EndsWith("Eb", StringComparison.Ordinal) ||
                   opcode.GetEncodingFor(code).ToString().EndsWith("M", StringComparison.Ordinal);
        }

        private static String GetBytesFor(IEnumerable<byte> code)
        {
            var formattedCode = new StringBuilder();
            foreach (var codeByte in code)
            {
                formattedCode.Append(String.Format("{0:x2} ", codeByte));
            }

            return formattedCode.ToString();
        }

        private static String GetDestinationOperandFor(Byte[] code)
        {
            var destinationOperand = opcode.GetDestinationRegisterFor(code).ToString().ToLower();
            var destinationIsEffectiveAddress =
                opcode.GetEncodingFor(code).ToString().StartsWith("Ev", StringComparison.Ordinal) ||
                opcode.GetEncodingFor(code).ToString().StartsWith("Eb", StringComparison.Ordinal);

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

                destinationOperand = Encase(destinationOperand);
            }
            else if (opcode.HasModRM(code) && destinationIsEffectiveAddress)
            {
                if (ModRM.HasIndex(code))
                {
                    destinationOperand += "+" + ModRM.GetIndexFor(code);
                }

                var effectiveAddressIsDereferenced = ModRM.IsEffectiveAddressDereferenced(code);
                if (effectiveAddressIsDereferenced)
                {
                    destinationOperand = Encase(destinationOperand);
                }
            }

            return destinationOperand;
        }
    }
}