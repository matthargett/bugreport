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
			throw new NotImplementedException();
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
