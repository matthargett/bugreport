using System;
using System.Collections.Generic;

namespace bugreport
{
	public struct OperationResult
	{
		public AbstractValue Value;
		public Boolean ZeroFlag;
		
		public OperationResult(AbstractValue value, Boolean zeroFlag)
		{
			this.Value = value;
			this.ZeroFlag = zeroFlag;
		}
		
		public override Boolean Equals(object obj)
		{
			OperationResult opResult = (OperationResult)obj;
			return (this.Value.Equals(opResult.Value)) && (this.ZeroFlag == opResult.ZeroFlag);
		}
		
		public override Int32 GetHashCode()
		{
			return this.Value.GetHashCode() ^ this.ZeroFlag.GetHashCode();
		}
		
		public static Boolean operator== (OperationResult a, OperationResult b)
		{
			return a.Equals(b);
		}
		
		public static Boolean operator!= (OperationResult a, OperationResult b)
		{
			return !a.Equals(b);
		}
	}
	
	public struct MachineState
	{
		private UInt32 instructionPointer;
		private RegisterCollection registers;
		private Dictionary<UInt32, AbstractValue> dataSegment;
		Boolean zeroFlag;

		public MachineState(RegisterCollection _registers)
		{
			dataSegment = new Dictionary<UInt32, AbstractValue>();
			registers = _registers;
			instructionPointer = 0x00;
			zeroFlag = false;
		}
		
		public MachineState(MachineState _copyMe)
		{
			this.instructionPointer = _copyMe.instructionPointer;
			this.registers = new RegisterCollection(_copyMe.registers);
			this.dataSegment = new Dictionary<UInt32, AbstractValue>(_copyMe.dataSegment);
			this.zeroFlag = _copyMe.zeroFlag;
		}
		
		public override Boolean Equals(object obj)
		{
			MachineState other = (MachineState)obj;
			
			if (!(this.instructionPointer == other.instructionPointer &&
				this.registers.Equals(other.registers) &&
				this.zeroFlag == other.zeroFlag))
			{
				return false;
			}
			
			foreach (UInt32 key in this.dataSegment.Keys)
			{
				if (!other.dataSegment.ContainsKey(key))
				{
					return false;
				}
				
				if (!this.dataSegment[key].Equals(other.dataSegment[key]))
				{
					return false;
				}
			}
			
			return true;
		}

		public override Int32 GetHashCode()
		{
			Int32 hashCode = this.instructionPointer.GetHashCode() ^
				this.registers.GetHashCode() ^
				this.zeroFlag.GetHashCode();
			
			foreach (UInt32 key in this.dataSegment.Keys)
			{
				hashCode ^= this.dataSegment[key].GetHashCode();
			}
			
			return hashCode;
		}
		
		public static Boolean operator== (MachineState a, MachineState b)
		{
			return a.Equals(b);
		}
		
		public static Boolean operator!= (MachineState a, MachineState b)
		{
			return !a.Equals(b);
		}
		
		public Boolean ZeroFlag
		{
			get { return zeroFlag; }
			private set { zeroFlag = value; }
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
				System.Diagnostics.Debug.Assert(registers[RegisterName.ESP] != null);
				System.Diagnostics.Debug.Assert(registers[RegisterName.ESP].PointsTo != null);
				return registers[RegisterName.ESP].PointsTo[0];
			}
			
			set
			{
				System.Diagnostics.Debug.Assert(registers[RegisterName.ESP] != null);
				System.Diagnostics.Debug.Assert(registers[RegisterName.ESP].PointsTo != null);
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


		public MachineState DoOperation(OperatorEffect _operatorEffect, AbstractValue _offset)
		{
			if (_offset.IsPointer)
				throw new ArgumentException("_offset pointer not supported.");

			MachineState newState = new MachineState(this);
			switch(_operatorEffect)
			{
				case OperatorEffect.Jnz:
					{
						if (!newState.zeroFlag)
							newState.instructionPointer += _offset.Value;
						
						break;
					}
				default:
					throw new ArgumentException(String.Format("Unsupported OperatorEffect: {0}", _operatorEffect), "_operatorEffect");
			}
			return newState;
		}

		public MachineState DoOperation(UInt32 offset, OperatorEffect _operatorEffect, AbstractValue rhs)
		{
			MachineState newState = new MachineState(this);

			switch(_operatorEffect)
			{
				case OperatorEffect.Assignment:
					{
						newState.dataSegment[offset] = newState.DoOperation(newState.dataSegment[offset], _operatorEffect, rhs).Value;
						break;
					}
			}
			return newState;
		}

		public MachineState DoOperation(RegisterName lhs, Int32 index, OperatorEffect _operatorEffect, AbstractValue rhs)
		{
			MachineState newState = new MachineState(this);

			switch(_operatorEffect)
			{
				case OperatorEffect.Assignment:
				case OperatorEffect.Cmp:
				{
					OperationResult result = newState.DoOperation(Registers[lhs].PointsTo[index], _operatorEffect, rhs);
					newState.Registers[lhs].PointsTo[index] = result.Value;
					newState.ZeroFlag = result.ZeroFlag;
					break;
				}
			}
			
			return newState;
		}
		
		public MachineState DoOperation(RegisterName lhs, OperatorEffect _operatorEffect, AbstractValue rhs)
		{
			MachineState newState = new MachineState(this);
			
			if (Registers[lhs].IsPointer && _operatorEffect != OperatorEffect.Assignment)
			{
				AbstractBuffer newBuffer = Registers[lhs].PointsTo.DoOperation(_operatorEffect, rhs);
				newState.Registers[lhs] = new AbstractValue(newBuffer);
			}
			else
			{
				OperationResult result = newState.DoOperation(this.Registers[lhs], _operatorEffect, rhs);
				newState.Registers[lhs] = result.Value;
				newState.ZeroFlag = result.ZeroFlag;
			}

			return newState;
		}
		
		public MachineState DoOperation(RegisterName lhs, OperatorEffect _operatorEffect, RegisterName rhs)
		{
			MachineState newState = new MachineState(this);
			OperationResult result = newState.DoOperation(Registers[lhs], _operatorEffect, Registers[rhs]);
			newState.Registers[lhs] = result.Value;
			newState.ZeroFlag = result.ZeroFlag;
			return newState;
		}

		public OperationResult DoOperation(AbstractValue lhs, OperatorEffect _operatorEffect, AbstractValue rhs)
		{
			if (rhs.IsPointer && _operatorEffect != OperatorEffect.Assignment)
				throw new ArgumentException("rhs pointer only supported for OperatorEffect.Assignment.");
			
			OperationResult result = new OperationResult();

			switch(_operatorEffect)
			{
				case OperatorEffect.Assignment:
				{
					AbstractValue newValue = new AbstractValue(rhs);
					if (rhs.IsInitialized && rhs.IsOOB)
						newValue.IsOOB = true;
					
					result.Value = newValue;
					
					return result;
				}
					
				case OperatorEffect.Add:
				{
					if (lhs.IsPointer)
					{
						AbstractBuffer newBuffer = lhs.PointsTo.DoOperation(OperatorEffect.Add, rhs);
						result.Value = new AbstractValue(newBuffer);
						return result;
					}
					
					UInt32 sum = lhs.Value + rhs.Value;
					AbstractValue sumValue = new AbstractValue(sum);
					if (lhs.IsTainted || rhs.IsTainted)
					{
						sumValue = sumValue.AddTaint();
					}
					
					result.Value = sumValue;
					return result;
				}
					
				case OperatorEffect.Sub:
				{
					if (lhs.IsPointer)
					{
						AbstractBuffer newBuffer = lhs.PointsTo.DoOperation(OperatorEffect.Sub, rhs);
						result.Value = new AbstractValue(newBuffer);
						return result;
					}
					UInt32 total = lhs.Value - rhs.Value;
					AbstractValue totalValue = new AbstractValue(total);
					
					if (lhs.IsTainted || rhs.IsTainted)
					{
						totalValue = totalValue.AddTaint();
					}
					
					result.Value = totalValue;
					
					return result;
				}
					
				case OperatorEffect.And:
				{
					if (lhs.IsPointer)
					{
						AbstractBuffer newBuffer = lhs.PointsTo.DoOperation(OperatorEffect.And, rhs);
						result.Value = new AbstractValue(newBuffer);
						return result;
					}
					
					UInt32 total = lhs.Value & rhs.Value;
					AbstractValue totalValue = new AbstractValue(total);

					if (lhs.IsTainted || rhs.IsTainted)
					{
						totalValue = totalValue.AddTaint();
					}

					result.Value = totalValue;
					return result;
				}
					
				case OperatorEffect.Xor:
				{
					if (lhs.IsPointer)
					{
						AbstractBuffer newBuffer = lhs.PointsTo.DoOperation(OperatorEffect.Xor, rhs);
						result.Value = new AbstractValue(newBuffer);
						return result;
					}
					
					UInt32 total = lhs.Value ^ rhs.Value;
					AbstractValue totalValue = new AbstractValue(total);

					if (lhs.IsTainted || rhs.IsTainted)
					{
						totalValue = totalValue.AddTaint();
					}

					result.Value = totalValue;
					return result;
				}
					
				case OperatorEffect.Shr:
				{
					UInt32 total = lhs.Value >> (Byte)rhs.Value;
					AbstractValue totalValue = new AbstractValue(total);

					if (lhs.IsTainted || rhs.IsTainted)
					{
						totalValue = totalValue.AddTaint();
					}

					result.Value = totalValue;
					return result;
				}
					
				case OperatorEffect.Shl:
				{
					UInt32 total = lhs.Value << (Byte)rhs.Value;
					AbstractValue totalValue = new AbstractValue(total);

					if (lhs.IsTainted || rhs.IsTainted)
					{
						totalValue = totalValue.AddTaint();
					}

					result.Value = totalValue;
					return result;
				}
					
				case OperatorEffect.Cmp:
				{
					if ((lhs.Value - rhs.Value) == 0)
						result.ZeroFlag = true;
					else
						result.ZeroFlag = false;
					
					result.Value = lhs;
					return result;
				}
					
				default:
				{
					throw new ArgumentException(String.Format("Unsupported OperatorEffect: {0}", _operatorEffect), "_operatorEffect");
				}
			}
		}
	}
}
