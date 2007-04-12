// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace bugreport
{
	public class InvalidOpcodeException : Exception
	{
		public InvalidOpcodeException(params Byte[] code) : base(FormatOpcodes(code))
		{
		}
		
		public static String FormatOpcodes(params Byte[] code)
		{
			StringBuilder message = new StringBuilder("Invalid opcode: ");
			
			
			foreach (Byte opcode in code)
			{
				message.Append(String.Format(" 0x{0:x2}", opcode));
			}
		
			return message.ToString();
		}
	}

	public static class X86emulator
	{
		public static MachineState Run(Collection<ReportItem> reportItemCollector, MachineState machineState, Byte[] code)
		{
			if (code.Length == 0)
				throw new ArgumentException("code", "Empty array not allowed.");
			
			MachineState afterState = emulateOpcode(reportItemCollector, machineState, code);
			afterState.InstructionPointer += (UInt32)code.Length;
			return afterState;
		}

		private static MachineState emulateOpcode(Collection<ReportItem> reportItems, MachineState machineState, Byte[] code)
		{
			MachineState state = machineState;
			RegisterName sourceRegister, destinationRegister;
			AbstractValue sourceValue;
			Int32 index;

			OpcodeEncoding encoding = OpcodeHelper.GetEncoding(code);
			OperatorEffect op = OpcodeHelper.GetOperatorEffect(code);
			
			if (op == OperatorEffect.Unknown)
				throw new InvalidOpcodeException(code);
	
			switch (encoding)
			{				
				case OpcodeEncoding.rAxIv:
				case OpcodeEncoding.rAxIz:
				{
					destinationRegister = OpcodeHelper.GetDestinationRegister(code);
					UInt32 immediate = OpcodeHelper.GetImmediate(code);
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
					switch (OpcodeHelper.GetStackEffect(code))
					{
						case StackEffect.Pop:
						{	
							destinationRegister = OpcodeHelper.GetDestinationRegister(code);							
							state.Registers[destinationRegister] = state.Registers[RegisterName.ESP].PointsTo[0];
							state = state.DoOperation(RegisterName.ESP, OperatorEffect.Sub, new AbstractValue(1));							
							break;
						}
						case StackEffect.Push:
						{
							state = state.DoOperation(RegisterName.ESP, OperatorEffect.Add, new AbstractValue(1));
							
							if (OpcodeHelper.HasSourceRegister(code))
							{
								sourceRegister = OpcodeHelper.GetSourceRegister(code);
								sourceValue = state.Registers[sourceRegister];
							}
							else if (OpcodeHelper.HasImmediate(code))
							{
								sourceValue = new AbstractValue(OpcodeHelper.GetImmediate(code));
							}
							else
							{
								throw new InvalidOperationException("tried to push something that wasn't a register or an immediate");
							}
							
							state.Registers[RegisterName.ESP].PointsTo[0] = sourceValue;

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
					destinationRegister = OpcodeHelper.GetDestinationRegister(code);
					index = 0;
					
					if (ModRM.HasIndex(code))
					{
						index = ModRM.GetIndex(code);
					}

					if (OpcodeHelper.HasImmediate(code))
					{
						sourceValue = new AbstractValue(OpcodeHelper.GetImmediate(code));
					}
					else
					{
						sourceRegister = ModRM.GetGv(code);
						sourceValue = state.Registers[sourceRegister];
					}
					
					if (ModRM.HasOffset(code))
					{
						UInt32 offset = ModRM.GetOffset(code);
						state.DataSegment[offset] = sourceValue;
						return state;
					}
					
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
					sourceValue =  state.Registers[sourceRegister];
					
					if (ModRM.HasOffset(code))
					{
						UInt32 offset = ModRM.GetOffset(code);
						sourceValue = state.DataSegment[offset];
					}
					else if (ModRM.IsEffectiveAddressDereferenced(code))
					{
						if (!sourceValue.IsPointer)
							throw new InvalidOperationException(String.Format("Trying to dereference null pointer in register {0}.", sourceRegister));

						index = 0;
						
						if (ModRM.HasIndex(code))
							index = ModRM.GetIndex(code);
						
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
					
					destinationRegister = ModRM.GetGv(code);
					AbstractValue baseRegisterValue;
					index = 0;
				
					// TODO: handle the [dword] special case
					if (ModRM.HasSIB(code))
					{
						UInt32 scaledRegisterValue = state.Registers[SIB.GetScaledRegister(code)].Value;
						UInt32 scaler = SIB.GetScaler(code);
						baseRegisterValue = state.Registers[SIB.GetBaseRegister(code)];
						index = (Int32)(scaledRegisterValue * scaler);
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
							new AbstractValue((UInt32)index)
						)
					);
					
					state = state.DoOperation(destinationRegister, OperatorEffect.Assignment, sourceValue);
					if (state.Registers[destinationRegister].IsOOB)
					{
						ReportItem reportItem = new ReportItem(state.InstructionPointer, sourceValue.IsTainted);
						reportItems.Add(reportItem);
					}
					
					return state;
				}
					
				case OpcodeEncoding.ObAL:
				{
					UInt32 offset;
					
					AbstractValue dwordValue = state.Registers[RegisterName.EAX];
					AbstractValue byteValue = dwordValue.TruncateValueToByte();
					
					offset = BitMath.BytesToDword(code, 1); // This is 1 for ObAL

					if (!state.DataSegment.ContainsKey(offset))
						state.DataSegment[offset] = new AbstractValue();
					
					state = state.DoOperation(offset, op, byteValue);
					return state;	
				}

				case OpcodeEncoding.Jz:
				{
					AbstractValue[] buffer = AbstractValue.GetNewBuffer(state.TopOfStack.Value); // hardcoded malloc emulation
					state.ReturnValue = new AbstractValue(buffer);
					return state;
				}

				case OpcodeEncoding.Jb:
				{
					UInt32 offset;
					offset = (UInt32) code[1];
					
					state = state.DoOperation(op, new AbstractValue(offset));
					
					return state;
				}
					
				case OpcodeEncoding.None:
				{
					return state;
				}

				default:
				{
					throw new InvalidOpcodeException( code);
				}
			}
		}
	}
}
