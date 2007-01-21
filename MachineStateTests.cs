
using NUnit.Framework;
using System;

namespace bugreport
{
	[TestFixture]
	public class MachineStateTests
	{
		MachineState state;
		AbstractValue one = new AbstractValue(1), two = new AbstractValue(2).AddTaint();

		private AbstractValue eax
		{
			get { return state.Registers[RegisterName.EAX]; }
			set { state.Registers[RegisterName.EAX] = value; }
		}
		
		private AbstractValue ebx 
		{
			get { return state.Registers[RegisterName.EBX]; }
			set { state.Registers[RegisterName.EBX] = value; }
		}
		
		[SetUp]
		public void SetUp()
		{
			state = new MachineState(new RegisterCollection());
		}
		
		[Test]
		public void Copy()
		{
			state.Registers[RegisterName.ESP] = new AbstractValue(new AbstractBuffer(AbstractValue.GetNewBuffer(10)));
			MachineState newState = new MachineState(state);
			Assert.AreNotSame(newState, state);
			Assert.AreNotSame(newState.Registers, state.Registers);
			Assert.AreNotSame(newState.DataSegment, state.DataSegment);
			Assert.AreNotSame(newState.ReturnValue, state.ReturnValue);
			Assert.AreNotSame(newState.TopOfStack, state.TopOfStack);
		}
		
		[Test]
		public void Assignment()
		{
			eax = one;
			ebx = two;
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.Assignment, RegisterName.EBX);
			
			Assert.AreEqual(eax, ebx);
			Assert.AreNotSame(eax, ebx);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Unknown()
		{
			state.DoOperation(RegisterName.EAX, OperatorEffect.Unknown, RegisterName.EBX);
		}

		[Test]
		public void Add()
		{
			eax = one;
			ebx = two;
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.Add, RegisterName.EBX);
			
			Assert.AreEqual(3, eax.Value);
			Assert.IsTrue(eax.IsTainted);
		}
		
		[Test]
		public void Sub()
		{
			eax = one;
			ebx = two;
			state = state.DoOperation(RegisterName.EBX, OperatorEffect.Sub, RegisterName.EAX);
			
			Assert.AreEqual(1, ebx.Value);
			Assert.IsTrue(ebx.IsTainted);
		}

		[Test]
		public void And()
		{
			eax = new AbstractValue(0x350).AddTaint();
			ebx = new AbstractValue(0xff);
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.And, RegisterName.EBX);
			
			Assert.AreEqual(0x50, eax.Value);
			Assert.IsTrue(eax.IsTainted);
		}

		[Test]
		public void Shr()
		{
			eax = new AbstractValue(0x8).AddTaint();
			ebx = new AbstractValue(0x3);
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.Shr, RegisterName.EBX);
			
			Assert.AreEqual(0x1, eax.Value);
			Assert.IsTrue(eax.IsTainted);
		}

		[Test]
		public void DoOperationForShl()
		{
			eax = one.AddTaint();
			ebx = new AbstractValue(0x3);
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.Shl, RegisterName.EBX);
			
			Assert.AreEqual(0x8, eax.Value);
			Assert.IsTrue(eax.IsTainted);
		}

		[Test]
		public void PointerAdd()
		{
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(0x10);	
			buffer[4] = one;
			eax = new AbstractValue(buffer);
			
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.Add, new AbstractValue(0x4));
			Assert.AreEqual(one, eax.PointsTo[0]);
		}
		
		[Test]
		public void PointerSub()
		{
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(0x10);	
			buffer[0] = one;
			eax = new AbstractValue(buffer);
			ebx = new AbstractValue(0x4);
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.Add, RegisterName.EBX);
			Assert.IsNotNull(eax.PointsTo);
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.Sub, RegisterName.EBX);
			Assert.AreEqual(one, eax.PointsTo[0]);
		}
		
		[Test]
		public void PointerAnd()
		{
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(0x10);
			buffer[4] = one;
			
			eax = new AbstractValue(buffer);
			
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.Add, new AbstractValue(0x4));
			Assert.AreEqual(one, eax.PointsTo[0]);
			
			AbstractValue andValue = new AbstractValue(0xfffffff0);
			MachineState newState = state.DoOperation(RegisterName.EAX, OperatorEffect.And, andValue);
			Assert.AreNotSame(newState, state);
			Assert.AreNotEqual(newState, state);
			
			state = newState;
			Assert.AreEqual(one, eax.PointsTo[4]);		
		}
		
		[Test]
		public void Jnz()
		{
			Byte offset = 6;
			eax = two;
			ebx = one;
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.Cmp, RegisterName.EAX);
			state = state.DoOperation(OperatorEffect.Jnz, new AbstractValue(offset));
			Assert.AreEqual(0, state.InstructionPointer);

			state = state.DoOperation(RegisterName.EAX, OperatorEffect.Cmp, RegisterName.EBX);
			state = state.DoOperation(OperatorEffect.Jnz, new AbstractValue(offset));		
			Assert.AreEqual(offset, state.InstructionPointer);
		}
	}
}
