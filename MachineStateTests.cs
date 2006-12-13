
using NUnit.Framework;
using System;

namespace bugreport
{
	[TestFixture]
	public class MachineStateTests
	{
		MachineState state;
		AbstractValue one = new AbstractValue(1), two = new AbstractValue(2).AddTaint();
		
		
		[SetUp]
		public void SetUp()
		{
			state = new MachineState(new RegisterCollection());
		}
		
		[Ignore("TODO")]
		[Test]
		public void Equal()
		{
			MachineState a = new MachineState(new RegisterCollection());
			MachineState b = new MachineState(new RegisterCollection());
			Assert.AreEqual(a, b);
		}
	
		[Test]
		[Ignore("TODO")]
		public void NotEqual()
		{
		}
		
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
		
		[Test]
		public void DoOperationForAssignment()
		{
			eax = one;
			ebx = two;
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.Assignment, RegisterName.EBX);
			
			Assert.AreEqual(eax, ebx);
			Assert.AreNotSame(eax, ebx);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DoOperationForUnknown()
		{
			state.DoOperation(RegisterName.EAX, OperatorEffect.Unknown, RegisterName.EBX);
		}

		[Test]
		public void DoOperationForAdd()
		{
			eax = one;
			ebx = two;
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.Add, RegisterName.EBX);
			
			Assert.AreEqual(3, eax.Value);
			Assert.IsTrue(eax.IsTainted);
		}
		
		[Test]
		public void DoOperationForSub()
		{
			eax = one;
			ebx = two;
			state = state.DoOperation(RegisterName.EBX, OperatorEffect.Sub, RegisterName.EAX);
			
			Assert.AreEqual(1, ebx.Value);
			Assert.IsTrue(ebx.IsTainted);
		}

		[Test]
		public void DoOperationForAnd()
		{
			eax = new AbstractValue(0x350).AddTaint();
			ebx = new AbstractValue(0xff);
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.And, RegisterName.EBX);
			
			Assert.AreEqual(0x50, eax.Value);
			Assert.IsTrue(eax.IsTainted);
		}

		[Test]
		public void DoOperationForShr()
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
		public void DoOperationForPointerAnd()
		{
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(0x10);
			buffer[4] = one;
			
			eax = new AbstractValue(buffer);
			
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.Add, new AbstractValue(0x4));
			Assert.AreEqual(one, eax.PointsTo[0]);
			
			AbstractValue andValue = new AbstractValue(0xfffffff0);
			state = state.DoOperation(RegisterName.EAX, OperatorEffect.And, andValue);
			Assert.AreEqual(one, eax.PointsTo[4]);		
		}
	}
}
