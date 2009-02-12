// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace bugreport
{
    public static class X86Emulator
    {
        private static readonly Opcode opcode = new X86Opcode();

        public static MachineState Run(Collection<ReportItem> reportItemCollector,
                                       MachineState machineState,
                                       Byte[] code)
        {
            if (code.Length == 0)
            {
                throw new ArgumentException("Empty array not allowed.", "code");
            }

            var afterState = emulateOpcode(reportItemCollector, machineState, code);

            if (!branchTaken(machineState, afterState))
            {
                afterState.InstructionPointer += opcode.GetInstructionLength(code);
            }

            return afterState;
        }

        private static Boolean branchTaken(MachineState before, MachineState after)
        {
            return before.InstructionPointer != after.InstructionPointer;
        }

        private static MachineState emulateOpcode(ICollection<ReportItem> reportItems,
                                                  MachineState machineState,
                                                  Byte[] code)
        {
            var state = machineState;
            RegisterName sourceRegister, destinationRegister;
            AbstractValue sourceValue;
            Int32 index;

            var encoding = opcode.GetEncoding(code);
            var op = opcode.GetOperatorEffect(code);

            switch (encoding)
            {
                case OpcodeEncoding.rAxIv:
                case OpcodeEncoding.rAxIz:
                {
                    destinationRegister = opcode.GetDestinationRegister(code);
                    var immediate = opcode.GetImmediate(code);
                    state = state.DoOperation(destinationRegister, op, new AbstractValue(immediate));
                    return state;
                }

                case OpcodeEncoding.rAxOv:
                {
                    state.Registers[RegisterName.EAX] = state.DataSegment[BitMath.BytesToDword(code, 1)];
                    return state;
                }

                case OpcodeEncoding.rBP:
                case OpcodeEncoding.rSI:
                case OpcodeEncoding.rSP:
                case OpcodeEncoding.rAX:
                case OpcodeEncoding.rBX:
                case OpcodeEncoding.rCX:
                case OpcodeEncoding.rDX:
                case OpcodeEncoding.Iz:
                {
                    switch (opcode.GetStackEffect(code))
                    {
                        case StackEffect.Pop:
                        {
                            destinationRegister = opcode.GetDestinationRegister(code);
                            state.Registers[destinationRegister] = state.Registers[RegisterName.ESP].PointsTo[0];
                            state = state.DoOperation(RegisterName.ESP, OperatorEffect.Sub, new AbstractValue(1));
                            break;
                        }

                        case StackEffect.Push:
                        {
                            state = state.DoOperation(RegisterName.ESP, OperatorEffect.Add, new AbstractValue(1));

                            if (opcode.HasSourceRegister(code))
                            {
                                sourceRegister = opcode.GetSourceRegister(code);
                                sourceValue = state.Registers[sourceRegister];
                            }
                            else if (opcode.HasImmediate(code))
                            {
                                sourceValue = new AbstractValue(opcode.GetImmediate(code));
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    String.Format(
                                        "tried to push something that wasn't a register or an immediate @ 0x{0:x8}",
                                        state.InstructionPointer));
                            }

                            state.Registers[RegisterName.ESP].PointsTo[0] = sourceValue;

                            // TODO(matt_hargett): next step in correct stack emulation,, but breaks PushESPPopESP test
                            //                        state = state.PushOntoStack(sourceValue);
                            break;
                        }
                    }

                    return state;
                }

                case OpcodeEncoding.EvIz:
                case OpcodeEncoding.EvIb:
                case OpcodeEncoding.EbIb:
                case OpcodeEncoding.EvGv:
                case OpcodeEncoding.EbGb:
                {
                    destinationRegister = opcode.GetDestinationRegister(code);
                    index = 0;

                    if (ModRM.HasIndex(code))
                    {
                        index = ModRM.GetIndex(code);
                    }

                    if (opcode.HasImmediate(code))
                    {
                        sourceValue = new AbstractValue(opcode.GetImmediate(code));
                    }
                    else
                    {
                        sourceRegister = ModRM.GetGv(code);
                        sourceValue = state.Registers[sourceRegister];
                    }

                    ////if (ModRM.HasOffset(code))
                    ////{
                    ////    UInt32 offset = ModRM.GetOffset(code);
                    ////    state.DataSegment[offset] = sourceValue;
                    ////    return state;
                    ////}

                    if (ModRM.IsEffectiveAddressDereferenced(code))
                    {
                        if (!state.Registers[destinationRegister].IsPointer)
                        {
                            throw new InvalidOperationException(
                                "Trying to dereference non-pointer in register " + destinationRegister
                                );
                        }

                        state = state.DoOperation(destinationRegister, index, op, sourceValue);
                        if (state.Registers[destinationRegister].PointsTo[index].IsOOB)
                        {
                            reportItems.Add(new ReportItem(state.InstructionPointer, sourceValue.IsTainted));
                        }
                    }
                    else
                    {
                        state = state.DoOperation(destinationRegister, op, sourceValue);
                        if (state.Registers[destinationRegister].IsOOB)
                        {
                            reportItems.Add(new ReportItem(state.InstructionPointer, sourceValue.IsTainted));
                        }
                    }

                    return state;
                }

                case OpcodeEncoding.GvEv:
                case OpcodeEncoding.GvEb:
                {
                    sourceRegister = ModRM.GetEv(code);
                    destinationRegister = ModRM.GetGv(code);
                    sourceValue = state.Registers[sourceRegister];

                    if (ModRM.HasOffset(code))
                    {
                        var offset = ModRM.GetOffset(code);
                        sourceValue = state.DataSegment[offset];
                    }
                    else if (ModRM.IsEffectiveAddressDereferenced(code))
                    {
                        if (!sourceValue.IsPointer)
                        {
                            throw new InvalidOperationException(
                                String.Format("Trying to dereference null pointer in register {0}.", sourceRegister));
                        }

                        index = 0;

                        if (ModRM.HasIndex(code))
                        {
                            index = ModRM.GetIndex(code);
                        }

                        sourceValue = sourceValue.PointsTo[index];

                        if (sourceValue.IsOOB)
                        {
                            reportItems.Add(new ReportItem(state.InstructionPointer, sourceValue.IsTainted));
                        }
                    }

                    state = state.DoOperation(destinationRegister, op, sourceValue);
                    return state;
                }

                case OpcodeEncoding.GvM:
                {
                    // GvM, M may refer to [base register + offset]
                    if (!ModRM.IsEffectiveAddressDereferenced(code))
                    {
                        throw new InvalidOperationException("GvM must be dereferenced");
                    }

                    destinationRegister = opcode.GetDestinationRegister(code);
                    AbstractValue baseRegisterValue;
                    index = 0;

                    // TODO: handle the [dword] special case
                    if (ModRM.HasSIB(code))
                    {
                        var scaledRegisterValue = state.Registers[SIB.GetScaledRegister(code)].Value;
                        var scaler = SIB.GetScaler(code);
                        baseRegisterValue = state.Registers[SIB.GetBaseRegister(code)];
                        index = (Int32) (scaledRegisterValue * scaler);
                    }
                    else
                    {
                        sourceRegister = ModRM.GetEv(code);
                        if (ModRM.HasIndex(code))
                        {
                            index = ModRM.GetIndex(code);
                        }

                        baseRegisterValue = state.Registers[sourceRegister];
                    }

                    // TODO: review these casts of index for possible elimination
                    sourceValue = new AbstractValue(
                        baseRegisterValue.PointsTo.DoOperation(
                            OperatorEffect.Add,
                            new AbstractValue((UInt32) index)
                            )
                        );

                    state = state.DoOperation(destinationRegister, OperatorEffect.Assignment, sourceValue);
                    if (state.Registers[destinationRegister].IsOOB)
                    {
                        var reportItem = new ReportItem(state.InstructionPointer, sourceValue.IsTainted);
                        reportItems.Add(reportItem);
                    }

                    return state;
                }

                case OpcodeEncoding.ObAL:
                {
                    var dwordValue = state.Registers[RegisterName.EAX];
                    var byteValue = dwordValue.TruncateValueToByte();
                    var offset = BitMath.BytesToDword(code, 1);

                    if (!state.DataSegment.ContainsKey(offset))
                    {
                        state.DataSegment[offset] = new AbstractValue();
                    }

                    state = state.DoOperation(offset, op, byteValue);
                    return state;
                }

                case OpcodeEncoding.Jz:
                {
                    // TODO: should push EIP + code.Length onto stack
                    var contractSatisfied = false;
                    var mallocContract = new MallocContract();
                    var glibcStartMainContract = new GLibcStartMainContract();
                    var contracts = new List<Contract> {mallocContract, glibcStartMainContract};

                    foreach (var contract in contracts)
                    {
                        if (contract.IsSatisfiedBy(state, code))
                        {
                            contractSatisfied = true;
                            state = contract.Execute(state);
                        }
                    }

                    if (!contractSatisfied)
                    {
                        var returnAddress = state.InstructionPointer + (UInt32) code.Length;
                        state = state.PushOntoStack(new AbstractValue(returnAddress));
                        state.InstructionPointer = opcode.GetEffectiveAddress(code, state.InstructionPointer);
                    }

                    return state;
                }

                case OpcodeEncoding.Jb:
                {
                    var offset = code[1];

                    state = state.DoOperation(op, new AbstractValue(offset));
                    state.InstructionPointer += opcode.GetInstructionLength(code);

                    return state;
                }

                case OpcodeEncoding.None:
                {
                    return state;
                }

                default:
                {
                    throw new InvalidOpcodeException(code);
                }
            }
        }
    }
}