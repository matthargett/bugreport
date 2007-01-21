// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace bugreport
{
	public class InvalidOpcodeException : Exception
	{
		public InvalidOpcodeException(String message, params Byte[] code) : base(String.Concat(message, "  ", FormatOpcodes(code)))
		{
		}
		
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
		public static MachineState Run(ICollection<ReportItem> reportItemCollector, MachineState _machineState, Byte[] code)
		{
			if (code.Length == 0)
				throw new ArgumentException("code", "Empty array not allowed.");
			
			MachineState afterState = emulateOpcode(reportItemCollector, _machineState, code);
			afterState.InstructionPointer += (UInt32)code.Length;
			return afterState;
		}

		private static MachineState emulateOpcode(ICollection<ReportItem> reportItemCollector, MachineState _machineState, Byte[] code)
		{
			MachineState machineState = _machineState;
			RegisterName sourceRegister, destinationRegister;
			AbstractValue sourceValue;
			Byte index;

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
					machineState = machineState.DoOperation(destinationRegister, op, new AbstractValue(immediate));
					return machineState;
				}

				case OpcodeEncoding.rAxOv:
				{
					machineState.Registers[RegisterName.EAX] = machineState.DataSegment[BitMath.BytesToDword(code, 1)];
					return machineState;
				}

				case OpcodeEncoding.rBP:
				case OpcodeEncoding.rBX:
				{
					switch (OpcodeHelper.GetStackEffect(code))
					{
						case StackEffect.Pop:
						{	
							destinationRegister = OpcodeHelper.GetDestinationRegister(code);
							machineState.Registers[destinationRegister] = machineState.TopOfStack;
							break;
						}
						case StackEffect.Push:
						{
							sourceRegister = OpcodeHelper.GetDestinationRegister(code);
							machineState.TopOfStack = machineState.Registers[sourceRegister];
							break;
						}
						default:
							throw new NotImplementedException("rBX only supports push and pop");
					}

					return machineState;
				}
				case OpcodeEncoding.EvIz:
				{
					if (!ModRM.IsEffectiveAddressDereferenced(code))
					{
						// TODO: this can be removed when other cases are driven with tests
						throw new NotImplementedException("EvIz currently supports only dereferenced Ev.");
					}
				
					index = 0;
					if (ModRM.HasIndex(code)) 
					{
						index = ModRM.GetIndex(code);
					}
					
					destinationRegister = OpcodeHelper.GetDestinationRegister(code);
					sourceValue = new AbstractValue(OpcodeHelper.GetImmediate(code));
					
					machineState = machineState.DoOperation(destinationRegister, index, OperatorEffect.Assignment, sourceValue);
					
					return machineState;
				}
					
				case OpcodeEncoding.EvIb:				
				case OpcodeEncoding.EbIb:
				{
					destinationRegister = ModRM.GetEv(code);
					index = 0;
					
					if (ModRM.HasIndex(code))
					{
						index = ModRM.GetIndex(code);
					}

					sourceValue = new AbstractValue(OpcodeHelper.GetImmediate(code));
					
					if (ModRM.IsEffectiveAddressDereferenced(code))
					{
						if (machineState.Registers[destinationRegister] == null)
						{
							throw new InvalidOperationException(
								String.Format("Trying to dereference null pointer in register {0}.", destinationRegister)
							);
						}
						
						machineState = machineState.DoOperation(destinationRegister, index, op, sourceValue);
						if (machineState.Registers[destinationRegister].PointsTo[index].IsOOB)
						{
							reportItemCollector.Add(new ReportItem(machineState.InstructionPointer, sourceValue.IsTainted));
						}
					}
					else
					{
						machineState = machineState.DoOperation(destinationRegister, op, sourceValue);
						if (machineState.Registers[destinationRegister].IsOOB)
						{
							reportItemCollector.Add(new ReportItem(machineState.InstructionPointer, sourceValue.IsTainted));
						}
					}
					
					return machineState;
				}

				case OpcodeEncoding.Jz:
					AbstractValue[] buffer = AbstractValue.GetNewBuffer(machineState.TopOfStack.Value); // hardcoded malloc emulation
					machineState.ReturnValue = new AbstractValue(buffer);
					return machineState;

				case OpcodeEncoding.GvEv:
				case OpcodeEncoding.GvEb:
				{
					sourceRegister = ModRM.GetEv(code);
					destinationRegister = ModRM.GetGv(code);
					sourceValue =  machineState.Registers[sourceRegister];
					
					if (ModRM.HasOffset(code))
					{
						UInt32 offset = ModRM.GetOffset(code);
						sourceValue = machineState.DataSegment[offset];
					}
					else if (ModRM.IsEffectiveAddressDereferenced(code))
					{
						if (sourceValue == null)
							throw new InvalidOperationException(String.Format("Trying to dereference null pointer in register {0}.", sourceRegister));

						index = 0;
						
						if (ModRM.HasIndex(code))
							index = ModRM.GetIndex(code);
						
						sourceValue = sourceValue.PointsTo[index];
						if (sourceValue.IsOOB)
						{
							reportItemCollector.Add(new ReportItem(machineState.InstructionPointer, sourceValue.IsTainted));
						}
					}
					
					machineState = machineState.DoOperation(destinationRegister, op, sourceValue);
					return machineState;
				}

				case OpcodeEncoding.EvGv:
				case OpcodeEncoding.EbGb:
				{
					if (ModRM.HasSIB(code))
					{
						destinationRegister = SIB.GetBaseRegister(code);
					}
					else
						destinationRegister = ModRM.GetEv(code);
					
					sourceRegister = ModRM.GetGv(code);
					sourceValue = machineState.Registers[sourceRegister];
					if (ModRM.HasOffset(code))
					{
						UInt32 offset = ModRM.GetOffset(code);
						machineState.DataSegment[offset] = sourceValue;
					}
					else if (ModRM.IsEffectiveAddressDereferenced(code))
					{
						if (sourceValue == null)
							throw new InvalidOperationException(String.Format("Trying to dereference null pointer in register {0}.", destinationRegister));

						index = 0;
						
						if (ModRM.HasIndex(code))
							index = ModRM.GetIndex(code);
						
						machineState = machineState.DoOperation(destinationRegister, index, op, sourceValue);
						if (machineState.Registers[destinationRegister].PointsTo[index].IsOOB)
							reportItemCollector.Add(new ReportItem(machineState.InstructionPointer, sourceValue.IsTainted));
					}
					else
					{
						machineState = machineState.DoOperation(destinationRegister, op, sourceRegister);
					}
					return machineState;							
				}

				case OpcodeEncoding.GvM:
				{
					// GvM, M must refer to [base register + offset]
					if (!ModRM.IsEffectiveAddressDereferenced(code))
					{
						throw new InvalidOperationException("GvM must be dereferenced");
					}
					
					destinationRegister = ModRM.GetGv(code);
					// TODO: handle memory-only and SIB cases
					sourceRegister = ModRM.GetEv(code);
					
					if (machineState.Registers[sourceRegister].PointsTo == null)
					{
						throw new InvalidOperationException("Trying to dereference a null pointer in register " + sourceRegister);
					}
					
					index = 0;
					if (ModRM.HasIndex(code))
					{
						index = ModRM.GetIndex(code);
					}
					
					AbstractValue rhs = machineState.DoOperation(
						machineState.Registers[sourceRegister],
						OperatorEffect.Add,
						new AbstractValue(index)
					).Value;
					machineState = machineState.DoOperation(destinationRegister, OperatorEffect.Assignment, rhs);
					if (machineState.Registers[destinationRegister].IsOOB)
					{
						ReportItem reportItem = new ReportItem(machineState.InstructionPointer, machineState.Registers[sourceRegister].IsTainted);
						reportItemCollector.Add(reportItem);
					}
					
					return machineState;
				}
					
				case OpcodeEncoding.ObAL:
				{
					UInt32 offset;
					
					AbstractValue dwordValue = machineState.Registers[RegisterName.EAX];
					AbstractValue byteValue = dwordValue.TruncateValueToByte();
					
					offset = BitMath.BytesToDword(code, 1); // This is 1 for ObAL

					if (!machineState.DataSegment.ContainsKey(offset))
						machineState.DataSegment[offset] = new AbstractValue();
					
					machineState = machineState.DoOperation(offset, op, byteValue);
					return machineState;	
				}
				
				case OpcodeEncoding.Jb:
				{
					UInt32 offset;
					offset = (UInt32) code[1];
					
					machineState = machineState.DoOperation(op, new AbstractValue(offset));
					
					return machineState;
				}
					
				case OpcodeEncoding.None:
					return machineState;

				default:
					throw new InvalidOpcodeException( code);
			}
		}
	}
}
