// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using NUnit.Framework;

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

	public class X86emulator
	{
		private UInt32 instructionPointer;
		private UInt32 stackSize;
		private RegisterCollection registers;
		private Dictionary<UInt32, AbstractValue> dataSegment;
		
		public X86emulator()
		{
			dataSegment = new Dictionary<UInt32, AbstractValue>();
			registers = new RegisterCollection();
			stackSize = 1;
		}
		
		public X86emulator(RegisterCollection _registers)
		{
			dataSegment = new Dictionary<UInt32, AbstractValue>();
			registers = _registers;
		}

		public UInt32 InstructionPointer
		{
			get
			{
				return instructionPointer;
			}
		}
		
		public RegisterCollection Registers
		{
			get { return registers; }
		}

		public Dictionary<UInt32, AbstractValue> DataSegment
		{
			get { return dataSegment; }
		}
		
		public UInt32 StackSize
		{
			get
			{
				return stackSize;
			}
		}

		public AbstractValue TopOfStack
		{
			get
			{
				return registers[RegisterName.ESP].PointsTo[0];
			}
			
			private set 
			{
				registers[RegisterName.ESP].PointsTo[0] = value;
			}
		}

		public AbstractValue ReturnValue
		{
			get
			{
				return registers[RegisterName.EAX];
			}
			set
			{
				registers[RegisterName.EAX] = value;
			}
		}
				
		public void PushOntoStack(AbstractValue value)
		{
			TopOfStack = new AbstractValue(value);
			stackSize = 1;
		}

		public void Run(Byte[] _code)
		{
			if (_code.Length == 0)
				throw new ArgumentException("_code", "Empty array not allowed.");
			
			emulateOpcode(_code);
			instructionPointer += (UInt32)_code.Length;
			return;
		}

		private void emulateOpcode(Byte[] _code)
		{
			RegisterName ev, gv;
			AbstractValue value;
			Byte index;

			OpcodeEncoding encoding = OpcodeHelper.GetEncoding(_code);
			OperatorEffect op = OpcodeHelper.GetOperatorEffect(_code);

			if (OpcodeHelper.GetStackEffect(_code) == StackEffect.Push)
			{
				stackSize++;
				return;
			}

			switch (encoding)
			{
				
				case OpcodeEncoding.rAxIv:
				case OpcodeEncoding.rAxIz:
				{
					UInt32 immediate = BitMath.BytesToDword(_code, 1);
					value = registers[RegisterName.EAX];
					registers[RegisterName.EAX] = value.DoOperation(op, new AbstractValue(immediate));
					return;
				}

				case OpcodeEncoding.rAxOv:
				{
					registers[RegisterName.EAX] = dataSegment[BitMath.BytesToDword(_code, 1)];
					return;
				}

				case OpcodeEncoding.EvIz:
				{
					if (ModRM.HasSIB(_code))
					{
						if (_code[2] != 0x24)
							throw new NotImplementedException(String.Format("Only supports 0x24 at this time, got: 0x{0:x2}.", _code[2]));
						
						TopOfStack = new AbstractValue(BitMath.BytesToDword(_code, 3));
						return;
					}
					
					if (!ModRM.IsEffectiveAddressDereferenced(_code))
						throw new NotImplementedException("EvIz currently supports only dereferenced Ev.");
				
					index = 0;
					Byte dwordOffset = 2;
					if (ModRM.HasIndex(_code)) {
						index = ModRM.GetIndex(_code);
						dwordOffset++;
					}
					
					ev = ModRM.GetEv(_code);
					UInt32 immediate = BitMath.BytesToDword(_code, dwordOffset);
					
					registers[ev].PointsTo[index] = new AbstractValue(immediate);
					
					return;
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
						if (registers[ev] == null)
							throw new InvalidOperationException(String.Format("Trying to dereference null pointer in register {0}.", ev));						
						
							registers[ev].PointsTo[index] = registers[ev].PointsTo[index].DoOperation(op, value);
                            if (registers[ev].PointsTo[index].IsOOB)
                                throw new OutOfBoundsMemoryAccessException(instructionPointer, value.IsTainted);
					}
					else
					{
						registers[ev] = registers[ev].DoOperation(op, value);
                        if (registers[ev].IsOOB)
						{
							throw new OutOfBoundsMemoryAccessException(instructionPointer, value.IsTainted);							
						}
					}
					return;
				}

				case OpcodeEncoding.Jz:
					AbstractValue[] buffer = AbstractValue.GetNewBuffer(TopOfStack.Value); // hardcoded malloc emulation
					ReturnValue = new AbstractValue(buffer);
					return;

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
						value = dataSegment[offset];
						registers[gv] = registers[gv].DoOperation(op, value);
						return;
					}
					
					value = registers[ev];
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
							throw new OutOfBoundsMemoryAccessException(instructionPointer, value.IsTainted);
						}
					}
					
					registers[gv] = registers[gv].DoOperation(op, value);
					return;
				}

				case OpcodeEncoding.GvM:
				{
					if (!ModRM.IsEffectiveAddressDereferenced(_code))
					{
						throw new InvalidOperationException("GvM must be dereferenced");
					}
					
					gv = ModRM.GetGv(_code);
					ev = ModRM.GetEv(_code);
					
					if (Registers[ev].PointsTo == null)
					{
						throw new NotImplementedException("GvM currently only supports dereferenced ev");
					}
					
					index = 0;
					if (ModRM.HasIndex(_code))
					{
						index = ModRM.GetIndex(_code);
					}
					
					Registers[gv] = Registers[ev].DoOperation(OperatorEffect.Add, new AbstractValue(index));
                    if (Registers[gv].IsOOB)
					{
						throw new OutOfBoundsMemoryAccessException(instructionPointer, Registers[ev].IsTainted);
					}
					
					return;
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
					value = registers[ev];
					
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
							value.PointsTo[index] = value.PointsTo[index].DoOperation(op, registers[gv]);
							if (value.PointsTo[index].IsOOB)
								throw new OutOfBoundsMemoryAccessException(instructionPointer, registers[gv].IsTainted);
                        }

					}
					else
					{
						registers[ev] = value.DoOperation(op, registers[gv]);
					}
					return;							
				}

				case OpcodeEncoding.ObAL:
				{
					UInt32 offset;
					
					AbstractValue dwordValue = registers[RegisterName.EAX];
					AbstractValue byteValue = dwordValue.TruncateValueToByte();
					
					offset = BitMath.BytesToDword(_code, 1); // This is 1 for ObAL
					if (dataSegment.ContainsKey(offset)) 
						value = dataSegment[offset];
					else
						value = new AbstractValue();						
					
					dataSegment[offset] = value.DoOperation(op, byteValue);
					return;	
				}
				
				case OpcodeEncoding.None:
					return;

				default:
					throw new InvalidOpcodeException( _code);
			}
		}
	}
}
