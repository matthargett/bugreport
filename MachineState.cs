using System;
using System.Collections.Generic;

namespace bugreport
{
	public struct MachineState : IEquatable<MachineState>
	{
		private UInt32 instructionPointer;
		private RegisterCollection registers;
		private Dictionary<UInt32, AbstractValue> dataSegment;

		public MachineState(RegisterCollection _registers)
		{
			dataSegment = new Dictionary<UInt32, AbstractValue>();
			registers = _registers;
			instructionPointer = 0x00;
		}
		
		public MachineState(MachineState _copyMe)
		{
			this.instructionPointer = _copyMe.instructionPointer;
			this.registers = new RegisterCollection(_copyMe.registers);
			this.dataSegment = new Dictionary<UInt32, AbstractValue>(_copyMe.dataSegment);
		}
		public Boolean ZeroFlag
		{
			get { return false; }
		}

		public UInt32 InstructionPointer
		{
			get
			{
				return instructionPointer;
			}
			set 
			{
				instructionPointer = value;
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

		public AbstractValue TopOfStack
		{
			get
			{
				return registers[RegisterName.ESP].PointsTo[0];
			}
			
			set 
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
		}

		public MachineState DoOperation(RegisterName lhs, OperatorEffect _operatorEffect, AbstractValue rhs)
		{
			if (rhs.IsPointer)
				throw new ArgumentException("rhs pointer not supported.");

			MachineState newState = new MachineState(this);

			if (Registers[lhs].IsPointer)
			{
				AbstractBuffer newBuffer = Registers[lhs].PointsTo.DoOperation(_operatorEffect, rhs);
				newState.Registers[lhs] = new AbstractValue(newBuffer);
			}
			else
				throw new NotImplementedException("Operating on a register that does not contain a pointer is not supported");

			return newState;
		}
		
		public MachineState DoOperation(RegisterName lhs, OperatorEffect _operatorEffect, RegisterName rhs)
		{
			MachineState newState = new MachineState(this);
			
			switch(_operatorEffect)
			{
				case OperatorEffect.Assignment:
				{
					newState.Registers[lhs] = new AbstractValue(Registers[rhs]);
					break;
				}
				case OperatorEffect.Add:
				{
					newState.Registers[lhs] = new AbstractValue(Registers[lhs].Value + Registers[rhs].Value);
					newState.Registers[lhs] = newState.Registers[lhs].AddTaintIf(Registers[lhs].IsTainted || Registers[rhs].IsTainted);
					break;
				}
				case OperatorEffect.Sub:
				{
					newState.Registers[lhs] = new AbstractValue(Registers[lhs].Value - Registers[rhs].Value);
					newState.Registers[lhs] = newState.Registers[lhs].AddTaintIf(Registers[lhs].IsTainted || Registers[rhs].IsTainted);
					break;
				}
				case OperatorEffect.And:
				{
					newState.Registers[lhs] = new AbstractValue(Registers[lhs].Value & Registers[rhs].Value);
					newState.Registers[lhs] = newState.Registers[lhs].AddTaintIf(Registers[lhs].IsTainted || Registers[rhs].IsTainted);
					break;
				}
				case OperatorEffect.Shr:
				{
					newState.Registers[lhs] = new AbstractValue(Registers[lhs].Value >> (Byte)(Registers[rhs].Value));
					newState.Registers[lhs] = newState.Registers[lhs].AddTaintIf(Registers[lhs].IsTainted || Registers[rhs].IsTainted);
					break;
				}
				case OperatorEffect.Shl:
				{
					newState.Registers[lhs] = new AbstractValue(Registers[lhs].Value << (Byte)(Registers[rhs].Value));
					newState.Registers[lhs] = newState.Registers[lhs].AddTaintIf(Registers[lhs].IsTainted || Registers[rhs].IsTainted);
					break;
				}
				
				default:
					throw new ArgumentException(String.Format("Unsupported OperatorEffect: {0}", _operatorEffect), "_operatorEffect");
					
			}
			return newState;
		}
		public AbstractValue DoOperation(AbstractValue lhs, OperatorEffect _operatorEffect, AbstractValue rhs)
		{
			switch(_operatorEffect)
			{
                case OperatorEffect.Assignment:
                {
                    AbstractValue newAbstractValue = new AbstractValue(rhs);
                    if (rhs.IsInitialized && rhs.IsOOB)
                        newAbstractValue.IsOOB = true;
                    
                    return newAbstractValue;
                }
					
				case OperatorEffect.Add:
				{
					if (rhs.IsPointer)
						throw new ArgumentException("rhs pointer not supported.");
					
					if (lhs.IsPointer)
					{
						AbstractBuffer newBuffer = lhs.PointsTo.DoOperation(OperatorEffect.Add, rhs);
						return new AbstractValue(newBuffer);
					}
					
					UInt32 sum = lhs.Value + rhs.Value;
					AbstractValue result = new AbstractValue(sum);
					if (lhs.IsTainted || rhs.IsTainted)
					{
						result = result.AddTaint();
					}
					
					return result;
				}
					
				case OperatorEffect.Sub:
				{
					if (rhs.IsPointer)
						throw new ArgumentException("rhs pointer not supported.");
					
					if (lhs.IsPointer)
					{
						AbstractBuffer newBuffer = lhs.PointsTo.DoOperation(OperatorEffect.Sub, rhs);
						return new AbstractValue(newBuffer);
					}
					UInt32 total = lhs.Value - rhs.Value;
					AbstractValue result = new AbstractValue(total);
					
					if (lhs.IsTainted || rhs.IsTainted)
					{
						result = result.AddTaint();
					}
					
					return result;
				}
				
				case OperatorEffect.And:
				{
					if (rhs.IsPointer)
						throw new ArgumentException("rhs pointer not supported.");
					
					if (lhs.IsPointer)
					{
						AbstractBuffer newBuffer = lhs.PointsTo.DoOperation(OperatorEffect.And, rhs);
						return new AbstractValue(newBuffer);
					}
					
					UInt32 total = lhs.Value & rhs.Value;
					AbstractValue result = new AbstractValue(total);

					if (lhs.IsTainted || rhs.IsTainted)
					{
						result = result.AddTaint();
					}

					return result;
				}
				
				case OperatorEffect.Shr:
				{
					UInt32 total = lhs.Value >> (Byte)rhs.Value;
					AbstractValue result = new AbstractValue(total);

					if (lhs.IsTainted || rhs.IsTainted)
					{
						result = result.AddTaint();
					}

					return result;
				}
					
				case OperatorEffect.Shl:
				{
					UInt32 total = lhs.Value << (Byte)rhs.Value;
					AbstractValue result = new AbstractValue(total);

					if (lhs.IsTainted || rhs.IsTainted)
					{
						result = result.AddTaint();
					}

					return result;
				}
					
				default:
					throw new ArgumentException(String.Format("Unsupported OperatorEffect: {0}", _operatorEffect), "_operatorEffect");
			}
		}
				
		#region Equals and GetHashCode implementation
		// The code in this region is useful if you want to use this structure in collections.
		// If you don't need it, you can just remove the region and the ": IEquatable<MachineState>" declaration.
		
		public override bool Equals(object obj)
		{
			if (obj is MachineState)
				return Equals((MachineState)obj); // use Equals method below
			else
				return false;
		}
		
		public bool Equals(MachineState other)
		{
			// add comparisions for all members here
			return this.dataSegment.Equals(other.dataSegment) &&
				this.instructionPointer == other.instructionPointer &&
				this.registers.Equals(other.registers);
		}
		
		public override int GetHashCode()
		{
			// combine the hash codes of all members here (e.g. with XOR operator ^)
			throw new NotImplementedException();
		}
		
		public static bool operator ==(MachineState lhs, MachineState rhs)
		{
			return lhs.Equals(rhs);
		}
		
		public static bool operator !=(MachineState lhs, MachineState rhs)
		{
			return !(lhs.Equals(rhs)); // use operator == and negate result
		}
		#endregion
	}
}
