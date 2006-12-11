// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;

namespace bugreport
{
    public class OutOfBoundsMemoryAccessException : ApplicationException
    {
		private readonly UInt32 instructionPointer;
		private readonly Boolean isTainted;
        
		public OutOfBoundsMemoryAccessException (UInt32 _instructionPointer, Boolean _isTainted)
		{
			instructionPointer = _instructionPointer;
			isTainted = _isTainted;
		}

		public UInt32 InstructionPointer
		{
			get
			{
				return instructionPointer;
			}
		}

		public Boolean IsTainted
		{
		    get
		    {
		        return isTainted;
		    }
		}		
    }

	public class InvalidOpcodeException : ApplicationException
	{
		
		public InvalidOpcodeException(params Byte[] _code) : base(FormatOpcodes(_code))
		{
		}
		
		public static String FormatOpcodes(params Byte[] _code)
		{
			String message = "Invalid opcode: ";
			
			
			foreach (Byte opcode in _code)
			{
				message += String.Format(" 0x{0:x2}", opcode);
			}
		
			return message;
		}
	}

	public static class X86emulator
	{
		public static MachineState Run(MachineState _machineState, Byte[] _code)
		{
			if (_code.Length == 0)
				throw new ArgumentException("_code", "Empty array not allowed.");
			
			MachineState afterState = emulateOpcode(_machineState, _code);
			afterState.InstructionPointer += (UInt32)_code.Length;
			return afterState;
		}

		private static MachineState emulateOpcode(MachineState _machineState, Byte[] _code)
		{
			MachineState machineState = _machineState;
			RegisterName ev, gv;
			AbstractValue value;
			Byte index;

			OpcodeEncoding encoding = OpcodeHelper.GetEncoding(_code);
			OperatorEffect op = OpcodeHelper.GetOperatorEffect(_code);
			
			if (op == OperatorEffect.Unknown)
				throw new InvalidOpcodeException(_code);
	
			switch (encoding)
			{
				
				case OpcodeEncoding.rAxIv:
				case OpcodeEncoding.rAxIz:
				{
					UInt32 immediate = BitMath.BytesToDword(_code, 1);
					value = machineState.Registers[RegisterName.EAX];
					machineState.Registers[RegisterName.EAX] = machineState.DoOperation(value, op, new AbstractValue(immediate));
					return machineState;
				}

				case OpcodeEncoding.rAxOv:
				{
					machineState.Registers[RegisterName.EAX] = machineState.DataSegment[BitMath.BytesToDword(_code, 1)];
					return machineState;
				}

				case OpcodeEncoding.rBP:
				{
						switch (OpcodeHelper.GetStackEffect(_code))
						{
								case StackEffect.Pop:
									machineState.Registers[RegisterName.EBP] = machineState.TopOfStack;
									break;
								case StackEffect.Push:
									machineState.TopOfStack = machineState.Registers[RegisterName.EBP];
									break;
								default:
									throw new NotImplementedException("rBP only supports push and pop");
						}
						return machineState;
				}

				case OpcodeEncoding.rBX:
				{
						switch (OpcodeHelper.GetStackEffect(_code))
						{
								case StackEffect.Pop:
									machineState.Registers[RegisterName.EBX] = machineState.TopOfStack;
									break;
								case StackEffect.Push:
									machineState.TopOfStack = machineState.Registers[RegisterName.EBX];
									break;
								default:
									throw new NotImplementedException("rBX only supports push and pop");
						}
						return machineState;
				}
				case OpcodeEncoding.EvIz:
				{
					if (ModRM.HasSIB(_code))
					{
						if (_code[2] != 0x24)
							throw new NotImplementedException(String.Format("Only supports 0x24 at this time, got: 0x{0:x2}.", _code[2]));
						
						machineState.TopOfStack = new AbstractValue(BitMath.BytesToDword(_code, 3));
						return machineState;
					}
					
					if (!ModRM.IsEffectiveAddressDereferenced(_code))
						throw new NotImplementedException("EvIz currently supports only dereferenced Ev.");
				
					index = 0;
					Byte dwordOffset = 2;
					if (ModRM.HasIndex(_code)) 
					{
						index = ModRM.GetIndex(_code);
						dwordOffset++;
					}
					
					ev = ModRM.GetEv(_code);
					UInt32 immediate = BitMath.BytesToDword(_code, dwordOffset);
					
					machineState.Registers[ev].PointsTo[index] = new AbstractValue(immediate);
					
					return machineState;
				}
					
				case OpcodeEncoding.EvIb:				
				case OpcodeEncoding.EbIb:
				{
					ev = ModRM.GetEv(_code);
					Int32 valueIndex = 2;
					index = 0;
					
					if (ModRM.HasIndex(_code))
					{
						valueIndex++;
						index = ModRM.GetIndex(_code);
					}

					value = new AbstractValue(_code[valueIndex]);
					
					if (ModRM.IsEffectiveAddressDereferenced(_code))
					{
						if (machineState.Registers[ev] == null)
							throw new InvalidOperationException(String.Format("Trying to dereference null pointer in register {0}.", ev));						
						
							machineState.Registers[ev].PointsTo[index] = machineState.DoOperation(machineState.Registers[ev].PointsTo[index], op, value);
                            if (machineState.Registers[ev].PointsTo[index].IsOOB)
                                throw new OutOfBoundsMemoryAccessException(machineState.InstructionPointer, value.IsTainted);
					}
					else
					{
						machineState.Registers[ev] = machineState.DoOperation(machineState.Registers[ev], op, value);
                        if (machineState.Registers[ev].IsOOB)
						{
							throw new OutOfBoundsMemoryAccessException(machineState.InstructionPointer, value.IsTainted);							
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
					ev = ModRM.GetEv(_code);
					gv = ModRM.GetGv(_code);
					
					if (ModRM.HasOffset(_code))
					{
						Byte offsetBeginsAt = OpcodeHelper.GetOpcodeLength(_code);
						offsetBeginsAt++; // for modRM byte
						
						UInt32 offset = BitMath.BytesToDword(_code, offsetBeginsAt);
						value = machineState.DataSegment[offset];
						machineState.Registers[gv] = machineState.DoOperation(machineState.Registers[gv], op, value);
						return machineState;
					}
					
					value = machineState.Registers[ev];
					if (ModRM.IsEffectiveAddressDereferenced(_code))
					{
						if (value == null)
							throw new InvalidOperationException(String.Format("Trying to dereference null pointer in register {0}.", ev));

						index = 0;
						
						if (ModRM.HasIndex(_code))
							index = ModRM.GetIndex(_code);
						
						value = value.PointsTo[index];
                        if (value.IsOOB)
						{
							throw new OutOfBoundsMemoryAccessException(machineState.InstructionPointer, value.IsTainted);
						}
					}
					
					machineState.Registers[gv] = machineState.DoOperation(machineState.Registers[gv], op, value);
					return machineState;
				}

				case OpcodeEncoding.GvM:
				{
					if (!ModRM.IsEffectiveAddressDereferenced(_code))
					{
						throw new InvalidOperationException("GvM must be dereferenced");
					}
					
					gv = ModRM.GetGv(_code);
					ev = ModRM.GetEv(_code);
					
					if (machineState.Registers[ev].PointsTo == null)
					{
						throw new NotImplementedException("GvM currently only supports dereferenced ev");
					}
					
					index = 0;
					if (ModRM.HasIndex(_code))
					{
						index = ModRM.GetIndex(_code);
					}
					
					machineState.Registers[gv] = machineState.DoOperation(machineState.Registers[ev], OperatorEffect.Add, new AbstractValue(index));
                    if (machineState.Registers[gv].IsOOB)
					{
						throw new OutOfBoundsMemoryAccessException(machineState.InstructionPointer, machineState.Registers[ev].IsTainted);
					}
					
					return machineState;
				}
					
				case OpcodeEncoding.EvGv:
				case OpcodeEncoding.EbGb:
				{
					if (ModRM.HasSIB(_code))
					{
						ev = SIB.GetBaseRegister(_code);
					}
					else
						ev = ModRM.GetEv(_code);
					
					gv = ModRM.GetGv(_code);
					value = machineState.Registers[ev];
					
					if (ModRM.IsEffectiveAddressDereferenced(_code))
					{
						if (value == null)
							throw new InvalidOperationException(String.Format("Trying to dereference null pointer in register {0}.", ev));

						index = 0;
						
						if (ModRM.HasIndex(_code))
							index = ModRM.GetIndex(_code);
						
						if (ModRM.HasOffset(_code))
						{
							throw new InvalidOpcodeException(_code);
						}
						else
						{
							value.PointsTo[index] = machineState.DoOperation(value.PointsTo[index], op, machineState.Registers[gv]);
							if (value.PointsTo[index].IsOOB)
								throw new OutOfBoundsMemoryAccessException(machineState.InstructionPointer, machineState.Registers[gv].IsTainted);
                        }

					}
					else
					{
						machineState.Registers[ev] = machineState.DoOperation(value, op, machineState.Registers[gv]);
					}
					return machineState;							
				}

				case OpcodeEncoding.ObAL:
				{
					UInt32 offset;
					
					AbstractValue dwordValue = machineState.Registers[RegisterName.EAX];
					AbstractValue byteValue = dwordValue.TruncateValueToByte();
					
					offset = BitMath.BytesToDword(_code, 1); // This is 1 for ObAL
					if (machineState.DataSegment.ContainsKey(offset)) 
						value = machineState.DataSegment[offset];
					else
						value = new AbstractValue();						
					
					machineState.DataSegment[offset] = machineState.DoOperation(value, op, byteValue);
					return machineState;	
				}
				
				case OpcodeEncoding.None:
					return machineState;

				default:
					throw new InvalidOpcodeException( _code);
			}
		}
	}
}
