// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;

namespace bugreport
{
    /// <summary>
    /// Based on table at http://sandpile.org/ia32/opc_1.htm
    /// </summary>
    public sealed class X86Opcode : Opcode
    {
        public OpcodeEncoding GetEncodingFor(Byte[] code)
        {
            switch (code[0])
            {
                case 0xc9:
                case 0xc3:
                case 0x90:
                case 0xf4:
                    return OpcodeEncoding.None;
                case 0x5e:
                case 0x56:
                    return OpcodeEncoding.rSI;
                case 0x05:
                    return OpcodeEncoding.rAxIz;
                case 0x0f:
                    return OpcodeEncoding.GvEb;
                case 0x50:
                case 0x58:
                    return OpcodeEncoding.rAX;
                case 0x53:
                case 0x5b:
                    return OpcodeEncoding.rBX;
                case 0x51:
                case 0x59:
                    return OpcodeEncoding.rCX;
                case 0x52:
                case 0x5a:
                    return OpcodeEncoding.rDX;
                case 0x5d:
                case 0x55:
                    return OpcodeEncoding.rBP;
                case 0x54:
                case 0x5c:
                    return OpcodeEncoding.rSP;
                case 0x75:
                    return OpcodeEncoding.Jb;
                case 0x83:
                case 0xc1:
                    return OpcodeEncoding.EvIb;
                case 0x88:
                    return OpcodeEncoding.EbGb;
                case 0x29:
                case 0x31:
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
                case 0x68:
                    return OpcodeEncoding.Iz;
                case 0xcc:
                    return OpcodeEncoding.Int3;
                default:
                    throw new InvalidOpcodeException(code);
            }
        }

        public Boolean HasModRM(Byte[] code)
        {
            switch (GetEncodingFor(code))
            {
                case OpcodeEncoding.GvEb:
                case OpcodeEncoding.GvEv:
                case OpcodeEncoding.GvM:
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

        public Boolean HasImmediate(Byte[] code)
        {
            switch (GetEncodingFor(code))
            {
                case OpcodeEncoding.EvIb:
                case OpcodeEncoding.EbIb:
                case OpcodeEncoding.Jb:
                case OpcodeEncoding.EvIz:
                case OpcodeEncoding.rAxIz:
                case OpcodeEncoding.rAxIv:
                case OpcodeEncoding.Jz:
                case OpcodeEncoding.Iz:
                    return true;
                default:
                    return false;
            }
        }

        public UInt32 GetImmediateFor(Byte[] code)
        {
            if (!HasImmediate(code))
            {
                throw new InvalidOperationException("Can't get immediate from an opcode that doesn't have one");
            }

            Byte valueIndex = 1;
            if (HasModRM(code))
            {
                valueIndex++;

                if (ModRM.HasIndex(code))
                {
                    valueIndex++;
                }

                if (ModRM.HasSIB(code))
                {
                    valueIndex++;
                }
            }

            switch (GetEncodingFor(code))
            {
                case OpcodeEncoding.EvIb:
                case OpcodeEncoding.EbIb:
                case OpcodeEncoding.Jb:
                {
                    return code[valueIndex];
                }

                case OpcodeEncoding.EvIz:
                case OpcodeEncoding.rAxIz:
                case OpcodeEncoding.rAxIv:
                case OpcodeEncoding.Jz:
                case OpcodeEncoding.Iz:
                {
                    return BitMath.BytesToDword(code, valueIndex);
                }

                default:
                {
                    throw new NotImplementedException("Don't know how to get the immediate for this opcode: " + code[0]);
                }
            }
        }

        public StackEffect GetStackEffectFor(Byte[] code)
        {
            switch (code[0])
            {
                case 0x50:
                case 0x51:
                case 0x52:
                case 0x53:
                case 0x54:
                case 0x55:
                case 0x56:
                case 0x68:
                    return StackEffect.Push;
                case 0x58:
                case 0x59:
                case 0x5a:
                case 0x5b:
                case 0x5c:
                case 0x5d:
                case 0x5e:
                    return StackEffect.Pop;

                default:
                    return StackEffect.None;
            }
        }

        public Boolean HasSourceRegister(Byte[] code)
        {
            return GetSourceRegisterFor(code) != RegisterName.None;
        }

        public Boolean HasDestinationRegister(Byte[] code)
        {
            return GetDestinationRegisterFor(code) != RegisterName.None;
        }

        public RegisterName GetSourceRegisterFor(Byte[] code)
        {
            var opcodeEncoding = GetEncodingFor(code);

            if (GetStackEffectFor(code) ==
                StackEffect.Push)
            {
                switch (opcodeEncoding)
                {
                    case OpcodeEncoding.rBP:
                    {
                        return RegisterName.EBP;
                    }

                    case OpcodeEncoding.rSI:
                    {
                        return RegisterName.ESI;
                    }

                    case OpcodeEncoding.rSP:
                    {
                        return RegisterName.ESP;
                    }

                    case OpcodeEncoding.rAX:
                    {
                        return RegisterName.EAX;
                    }

                    case OpcodeEncoding.rBX:
                    {
                        return RegisterName.EBX;
                    }

                    case OpcodeEncoding.rCX:
                    {
                        return RegisterName.ECX;
                    }

                    case OpcodeEncoding.rDX:
                    {
                        return RegisterName.EDX;
                    }
                }
            }

            switch (opcodeEncoding)
            {
                case OpcodeEncoding.EbGb:
                case OpcodeEncoding.EvGv:
                {
                    return ModRM.GetGvFor(code);
                }

                case OpcodeEncoding.GvEb:
                case OpcodeEncoding.GvEv:
                case OpcodeEncoding.GvM:
                {
                    if (ModRM.HasSIB(code))
                    {
                        // MOV EAX,DWORD PTR DS:[EDX+EAX*4]
                        // Since SIB does not have a single source register
                        // it currently returns RegisterName.None
                        return RegisterName.None;
                    }

                    if (ModRM.IsEvDword(code))
                    {
                        return RegisterName.None;
                    }

                    return ModRM.GetEvFor(code);
                }

                default:
                {
                    return RegisterName.None;
                }
            }
        }

        public RegisterName GetDestinationRegisterFor(Byte[] code)
        {
            var opcodeEncoding = GetEncodingFor(code);

            if (GetStackEffectFor(code) ==
                StackEffect.Pop)
            {
                switch (opcodeEncoding)
                {
                    case OpcodeEncoding.rBP:
                    {
                        return RegisterName.EBP;
                    }

                    case OpcodeEncoding.rSI:
                    {
                        return RegisterName.ESI;
                    }

                    case OpcodeEncoding.rSP:
                    {
                        return RegisterName.ESP;
                    }

                    case OpcodeEncoding.rAX:
                    {
                        return RegisterName.EAX;
                    }

                    case OpcodeEncoding.rBX:
                    {
                        return RegisterName.EBX;
                    }

                    case OpcodeEncoding.rCX:
                    {
                        return RegisterName.ECX;
                    }

                    case OpcodeEncoding.rDX:
                    {
                        return RegisterName.EDX;
                    }
                }
            }

            switch (opcodeEncoding)
            {
                case OpcodeEncoding.GvM:
                case OpcodeEncoding.GvEv:
                case OpcodeEncoding.GvEb:
                {
                    return ModRM.GetGvFor(code);
                }

                case OpcodeEncoding.rAxIv:
                case OpcodeEncoding.rAxIz:
                {
                    return RegisterName.EAX;
                }

                case OpcodeEncoding.EbGb:
                case OpcodeEncoding.EbIb:
                case OpcodeEncoding.EvGv:
                case OpcodeEncoding.EvIb:
                case OpcodeEncoding.EvIz:
                {
                    if (ModRM.HasSIB(code))
                    {
                        return SIB.GetBaseRegister(code);
                    }

                    return ModRM.GetEvFor(code);
                }

                default:
                {
                    return RegisterName.None;
                }
            }
        }

        public OperatorEffect GetOperatorEffectFor(Byte[] code)
        {
            switch (code[0])
            {
                case 0x05:
                    return OperatorEffect.Add;

                case 0x29:
                    return OperatorEffect.Sub;

                case 0x31:
                    return OperatorEffect.Xor;

                case 0x75:
                    return OperatorEffect.Jnz;

                case 0x83:
                {
                    var rm = ModRM.GetOpcodeGroupIndexFor(code);

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
                    var rm = ModRM.GetOpcodeGroupIndexFor(code);

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
                    return OperatorEffect.Call;

                default:
                    return OperatorEffect.Assignment;
            }
        }

        public Byte GetOpcodeLengthFor(Byte[] code)
        {
            Byte opcodeLength = 1;
            if (code[0] == 0x0f)
            {
                opcodeLength++;
            }

            return opcodeLength;
        }

        public Byte GetInstructionLengthFor(Byte[] code)
        {
            var instructionLength = GetOpcodeLengthFor(code);

            if (HasModRM(code))
            {
                instructionLength++;

                if (ModRM.HasSIB(code))
                {
                    instructionLength++;
                }

                if (ModRM.HasIndex(code))
                {
                    instructionLength += 1;
                }
            }

            if (HasOffset(code))
            {
                instructionLength += 4;
            }

            if (HasImmediate(code))
            {
                switch (GetEncodingFor(code))
                {
                    case OpcodeEncoding.EbIb:
                    case OpcodeEncoding.EvIb:
                    case OpcodeEncoding.Jb:
                        instructionLength += 1;
                        break;
                    default:
                        instructionLength += 4;
                        break;
                }
            }

            return instructionLength;
        }

        public Byte GetInstructionLengthFor(Byte[] code, UInt32 index)
        {
            var shortCode = new Byte[15];

            var minLength = (Int32) Math.Min(15, code.Length - index);
            Array.ConstrainedCopy(code, (Int32) index, shortCode, 0, minLength);

            return GetInstructionLengthFor(shortCode);
        }

        public Boolean HasOffset(Byte[] code)
        {
            var encoding = GetEncodingFor(code);

            if (encoding == OpcodeEncoding.ObAL ||
                encoding == OpcodeEncoding.rAxOv)
            {
                return true;
            }

            if (HasModRM(code))
            {
                if (ModRM.HasOffset(code))
                {
                    return true;
                }
            }

            return false;
        }

        public Boolean TerminatesFunction(Byte[] code)
        {
            return code[0] == 0xf4 || code[0] == 0xc3;
        }

        public UInt32 GetEffectiveAddress(Byte[] code, UInt32 instructionPointer)
        {
            var offset = GetImmediateFor(code);

            unchecked
            {
                // FIXME: find a way to do this without an unchecked operation
                return instructionPointer + offset + (UInt32) code.Length;
            }
        }
    }
}