// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	[TestFixture]
	public class X86emulatorTests
	{
		MachineState state;
		Byte[] code;
		
		[SetUp]
		public void SetUp()
		{
			state = new MachineState(new RegisterCollection());
			AbstractBuffer buffer = new AbstractBuffer(new AbstractValue[0x200]);
			AbstractValue pointer = new AbstractValue(buffer);
			state.Registers[RegisterName.ESP] = pointer;
			state.Registers[RegisterName.EBP] = state.Registers[RegisterName.ESP];
			
		}
		
		[Test]
		public void InitialRegisters()
		{
			AbstractValue value = new AbstractValue(1);
			RegisterCollection registers = new RegisterCollection();
			registers[RegisterName.EAX] = value;
			state = new MachineState(registers);
			state = X86emulator.Run(state, new byte[] { 0x90});
			Assert.AreEqual(value, state.Registers[RegisterName.EAX]);
		}
		
		[Test]
		public void MovFromGlobalIntoEax()
		{
			code = new Byte[] {0xa1, 0xe4, 0x84, 0x04, 0x08};
			AbstractValue value = new AbstractValue(31337);
			state.DataSegment[0x080484e4] = value;
			state = X86emulator.Run(state, code);
			
			Assert.AreEqual(value, state.Registers[RegisterName.EAX]);
		}
		
		[Test]
		public void PushEbpThenPopEbp()
		{
			
			Byte [] pushCode = new Byte[] {0x55};
			Byte [] popCode = new Byte[] {0x5d};
			state = X86emulator.Run(state, pushCode);

			Assert.AreEqual(0x1, state.InstructionPointer);
			state.Registers[RegisterName.EBP] = null;
			state = X86emulator.Run(state, popCode);
			Assert.AreEqual(0x2, state.InstructionPointer);
			Assert.AreEqual(state.Registers[RegisterName.ESP], state.Registers[RegisterName.EBP]);
			
		}

		[Test]
		public void PushEbxThenPopEbx()
		{
			
			Byte [] pushCode = new Byte[] {0x53};
			Byte [] popCode = new Byte[] {0x5b};
			state.Registers[RegisterName.EBX] = new AbstractValue(0x31337);
			state = X86emulator.Run(state, pushCode);

			Assert.AreEqual(0x1, state.InstructionPointer);
			state.Registers[RegisterName.EBX] = null;
			state = X86emulator.Run(state, popCode);
			Assert.AreEqual(0x2, state.InstructionPointer);
			Assert.AreEqual(0x31337, state.Registers[RegisterName.EBX].Value);
		}
		
		[Test]
		public void PushEbx()
		{
			code = new Byte[] {0x53};
			state = X86emulator.Run(state, code);

			Assert.AreEqual(0x1, state.InstructionPointer);
			Assert.AreEqual(state.TopOfStack, state.Registers[RegisterName.EBX]);
		}

		[Test]
		public void DefaultValues()
		{
			Assert.AreEqual(0x0, state.InstructionPointer);
		}

		[Test]
		public void MovEbpEsp()
		{ // mov    ebp,esp
			code = new Byte[] { 0x89, 0xe5} ;
			UInt32 value = 0x31337;
			AbstractValue abstractValue = new AbstractValue(value);
			state.Registers[RegisterName.ESP] = abstractValue;
			state = X86emulator.Run(state, code);

			Assert.AreEqual(0x2, state.InstructionPointer);
			Assert.AreEqual(value, state.Registers[RegisterName.ESP].Value);
			Assert.AreEqual(value, state.Registers[RegisterName.EBP].Value);
		}
		
		[Test]
		public void MovEaxEaxFourByteValue()
		{ //  mov    eax,DWORD PTR [eax]
			UInt32 value = 0x31337;
			AbstractValue[] buffer = new AbstractValue[] {new AbstractValue(value)};
			AbstractValue pointer = new AbstractValue(buffer);
			state.Registers[RegisterName.EAX] = pointer;

			code = new Byte[] {0x8b, 0x00} ;
			state = X86emulator.Run(state, code);

			Assert.AreEqual(0x2, state.InstructionPointer);
			Assert.AreEqual(value, state.Registers[RegisterName.EAX].Value);
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DerefRegisterWithNull()
		{
			//  mov    eax,DWORD PTR [eax]
			code = new Byte[] {0x8b, 0x00};
			state.Registers[RegisterName.EAX] = null;
			state = X86emulator.Run(state, code);
		}
		
		[Test]
		public void MovEaxEaxPointerPointerValue()
		{ //  mov    eax,DWORD PTR [eax]
			UInt32 value = 0x31337;
			AbstractValue argv = new AbstractValue(value);
			AbstractValue[] argvBuffer = new AbstractValue[] {argv};
			AbstractValue argvPointer = new AbstractValue(argvBuffer);
			state.Registers[RegisterName.EAX] = argvPointer;

			code = new Byte[] {0x8b, 0x00} ;
			state = X86emulator.Run(state, code);

			Assert.AreEqual(0x2, state.InstructionPointer);
			Assert.AreEqual(value, state.Registers[RegisterName.EAX].Value);
		}
		
		[Test]
		public void MovEaxEbpPlusTwelve()
		{
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(16);
			buffer[12] = new AbstractValue(1);
			
			AbstractValue pointer = new AbstractValue(buffer);
			state.Registers[RegisterName.EBP] = pointer;

			code = new Byte[] {0x8b, 0x45, 0xc} ;
			state = X86emulator.Run(state, code);

			Assert.AreEqual(0x3, state.InstructionPointer);
			Assert.AreEqual(1, state.ReturnValue.Value);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void EmptyCodeArray()
		{
			code = new Byte[] {};
			state = X86emulator.Run(state, code);
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOpcodeException))]
		public void InvalidOpcode()
		{
			state = X86emulator.Run(state, new Byte[] {0xfe});
		}
		

		[Test]
		public void Sub()
		{	
			code = new Byte[] {0x83, 0xec, 0x08};
			state.Registers[RegisterName.ESP] = new AbstractValue(0x0a);
			state = X86emulator.Run(state, code);
			Assert.AreEqual(0x3, state.InstructionPointer);
			Assert.AreEqual(0x02, state.Registers[RegisterName.ESP].Value);
		}
		
		[Test]
		public void CmpEax0()
		{ // cmp eax, 0
			code = new Byte[] {0x83, 0xf8, 0x0};
			state.Registers[RegisterName.EAX] = new AbstractValue(1);
			state = X86emulator.Run(state, code);
			Assert.AreEqual(0x3, state.InstructionPointer);
			Assert.IsFalse(state.ZeroFlag);

			state.Registers[RegisterName.EAX] = new AbstractValue(0);
			state = X86emulator.Run(state, code);
			Assert.AreEqual(0x6, state.InstructionPointer);
			Assert.IsTrue(state.ZeroFlag);
		}
		
		[Test]
		public void CmpEbpPlusEight0()
		{ // cmp    DWORD PTR [ebp+8],0x0
			code = new Byte[] {0x83, 0x7d, 0x08, 0x0};
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(16);
			buffer[8] = new AbstractValue(1);
			
			AbstractValue pointer = new AbstractValue(buffer);
			state.Registers[RegisterName.EBP] = pointer;

			state = X86emulator.Run(state, code);
			Assert.AreEqual(0x4, state.InstructionPointer);
			Assert.IsFalse(state.ZeroFlag);

			buffer[8] = new AbstractValue(0);
			state = X86emulator.Run(state, code);
			Assert.AreEqual(0x8, state.InstructionPointer);
			Assert.IsTrue(state.ZeroFlag);
		}

		[Test]
		public void MovPtrEsp0x10()
		{
			code = new Byte[] {0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};
			state = X86emulator.Run(state, code);
			Assert.AreEqual(0x7, state.InstructionPointer);
			Assert.AreEqual(0x10, state.TopOfStack.Value);
		}

		[Test]
		public void MovEax0x0()
		{
			code = new Byte[] {0xb8, 0x00, 0x00, 0x00, 0x00};
			state = X86emulator.Run(state, code);
			Assert.AreEqual(code.Length, state.InstructionPointer);
			Assert.AreEqual(0x0, state.Registers[RegisterName.EAX].Value);
		}

		[Test]
		public void AddEax0x1()
		{
			code = new Byte[] {0x05, 0x01, 0x00, 0x00, 0x00};
			state.Registers[RegisterName.EAX] = new AbstractValue(1);
			state = X86emulator.Run(state, code);
			Assert.AreEqual(code.Length, state.InstructionPointer);
			Assert.AreEqual(0x2, state.Registers[RegisterName.EAX].Value);
		}

		[Test]
		public void MovEax0xXXXXXXXX()
		{
			code = new Byte[] {0xb8, 0x37, 0x13, 0x03, 0x00};
			state = X86emulator.Run(state, code);
			Assert.AreEqual(code.Length, state.InstructionPointer);
			Assert.AreEqual(0x00031337, state.ReturnValue.Value);
		}

		[Test]
		public void MovEsp0xXXXXXXXX()
		{
			code = new Byte[] {0xc7, 0x04, 0x24, 0x37, 0x13, 0x03, 0x00};
			state = X86emulator.Run(state, code);
			Assert.AreEqual(0x7, state.InstructionPointer);
			Assert.AreEqual(0x00031337, state.TopOfStack.Value);
		}
		
		[Test]
		public void ReturnValueIsAliasedToEax()
		{
			AbstractValue value = new AbstractValue(0x31337);
			state.ReturnValue = value;
			Assert.AreEqual(value, state.Registers[RegisterName.EAX]);
		}

		[Test]
		public void Call()
		{
			//TODO: need to reconcile with esp/ebp handling
			state.PushOntoStack(new AbstractValue(16));
			code = new Byte[] {0xe8, 0x14, 0xff, 0xff, 0xff};
			state = X86emulator.Run(state, code);
			Assert.AreEqual(0x5, state.InstructionPointer);
			Assert.AreEqual(16, state.ReturnValue.PointsTo.Length);
		}
		
		[Test]
		public void MovIntoAssignedEaxOutOfBounds()
		{
			Byte value = 0x01, index = 0x10;
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(index);
			AbstractValue pointer = new AbstractValue(buffer);
			state.ReturnValue = pointer;
			code = new Byte[] {0xc6, 0x40, index, value};
			state = X86emulator.Run(state, new Byte[] { 0x90 });
			try
			{
			    state = X86emulator.Run(state, code);
			}
			catch (OutOfBoundsMemoryAccessException e)
			{
			    Assert.AreEqual(1, e.InstructionPointer);
			}
		}

		[Test]
		public void MovIntoAssignedEaxInBounds()
		{
			Byte value = 0x01, index = 0xf;
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(16);
			AbstractValue pointer = new AbstractValue(buffer);
			state.ReturnValue = pointer;
			code = new Byte[] {0xc6, 0x40, index, value};
			state = X86emulator.Run(state, code);
			Assert.AreEqual(value, state.ReturnValue.PointsTo[index].Value);
		}	
		
		[Test]
		public void GetNewBufferReturnsUnallocatedValues() {
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(16);
			for (int i = 0; i < 16; i++) {
				Assert.IsFalse(buffer[i].IsInitialized);
			}
		}
		
		[Test]
		public void MovBytePtrEaxInBounds()
		{ //	mov    BYTE PTR [eax], value
			Byte value = 0x01;
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(16);
			AbstractValue pointer = new AbstractValue(buffer);
			state.ReturnValue = pointer;
			code = new Byte[] {0xc6, 0x00, value};
			state = X86emulator.Run(state, code);
			Assert.AreEqual(value, state.ReturnValue.PointsTo[0].Value);
		}
		
		[Test]
		public void MovBytePtrEaxPlus16FromBl()
		{ // mov    BYTE PTR [eax+16],bl
			Byte offset = 0x10;
			state.Registers[RegisterName.EBX] = new AbstractValue(0x1);
			AbstractValue[] buffer = AbstractValue.GetNewBuffer((uint)offset+1);
			AbstractValue pointer = new AbstractValue(buffer);
			state.ReturnValue = pointer;

			code = new Byte[] {0x88, 0x58, offset};
			state = X86emulator.Run(state, code);

			Assert.AreEqual(0x1, state.ReturnValue.PointsTo[offset].Value);
		}

		[Test]
		public void MovDwordPtrEaxPlus16FromEbx()
		{ // mov    BYTE PTR [eax+16],bl
			Byte offset = 0x10;
			state.Registers[RegisterName.EBX] = new AbstractValue(0x1);
			AbstractValue[] buffer = AbstractValue.GetNewBuffer((uint)offset+1);
			AbstractValue pointer = new AbstractValue(buffer);
			state.Registers[RegisterName.EAX] = pointer;

			code = new Byte[] {0x89, 0x58, offset};
			state = X86emulator.Run(state, code);

			Assert.AreEqual(0x1, state.Registers[RegisterName.EAX].PointsTo[offset].Value);
		}

		[Test]
		public void MovIntoOffsetFromAl()
		{ // mov    ds:0x80495e0,al
			UInt32 value = 1;
			UInt32 offset = 0x80495e0;
			code = new Byte[] {0xa2, 0xe0, 0x95, 0x04, 0x08};
			state.Registers[RegisterName.EAX] = new AbstractValue(value);
			state = X86emulator.Run(state, code);
			Assert.AreEqual(value, state.DataSegment[offset].Value);
		}

		[Test]
		public void MovIntoEdxFromOffset()
		{ // movzx  edx,BYTE PTR ds:0x80495e0
			UInt32 value = 1;
			AbstractValue abstractValue = new AbstractValue(value);
			state.DataSegment[0x80495e0] = abstractValue;
			code = new Byte[] {0x0f, 0xb6, 0x15, 0xe0, 0x95, 0x04, 0x08};
			state = X86emulator.Run(state, code);
			Assert.AreEqual(value, state.Registers[RegisterName.EDX].Value);
		}

		[Test]
		public void AddImmediateToEAX()
		{ // add    eax,0xf
			Byte immediate = 0x0f;
			code = new Byte[] {0x83, 0xc0, immediate};
			UInt32 value = 1;
			state.Registers[RegisterName.EAX] = new AbstractValue(value);
			state = X86emulator.Run(state, code);
			Assert.AreEqual(value + immediate, state.Registers[RegisterName.EAX].Value);	
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void AddImmediateNullPointerDeref()
		{ // add    [eax], 0x00
			code = new Byte[] {0x83, 0x00, 0x00};
			state.Registers[RegisterName.EAX] = null;
			state = X86emulator.Run(state, code);
		}

		[Test]
		public void MovEax0x10()
		{ // mov    DWORD PTR [eax],0x10
		
			AbstractValue[] value = AbstractValue.GetNewBuffer(1);
			state.Registers[RegisterName.EAX] = new AbstractValue(value);
			code = new Byte[] {0xc7, 0x00, 0x10, 0x00, 0x00, 0x00};
			state = X86emulator.Run(state, code);

			AbstractValue sixteen = state.Registers[RegisterName.EAX].PointsTo[0];
			Assert.AreEqual(0x10, sixteen.Value);
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void SubNullPointerDeref()
		{
			// sub eax, [eax]
			code = new Byte[] {0x29, 0x00};
			state.Registers[RegisterName.EAX] = null;
			state = X86emulator.Run(state, code);
		}

		[Test]
		public void MovEbpMinus8()
		{ // mov    DWORD PTR [ebp-8],0xf
		
			AbstractValue[] value = AbstractValue.GetNewBuffer(0x100);
			state.Registers[RegisterName.EBP] = new AbstractValue(value);
			code = new Byte[] {0xc7, 0x45, 0xf8, 0x0f, 0x00, 0x00, 0x00};
			state = X86emulator.Run(state, code);

			AbstractValue fifteen = state.Registers[RegisterName.EBP].PointsTo[0xf8];
			Assert.AreEqual(0xf, fifteen.Value);
		}
		
		[Test]
		public void MovPtrEspEax()
		{ // mov [esp]. eax
			Byte[] code = new Byte[] {0x89, 0x04, 0x24};
			state.Registers[RegisterName.EAX] = new AbstractValue(0x10);
			AbstractValue[] values = new AbstractValue[] {new AbstractValue(1)};
			AbstractBuffer buffer = new AbstractBuffer(values);
			state.Registers[RegisterName.ESP] = new AbstractValue(buffer);
			
			state = X86emulator.Run(state, code);
			
			AbstractBuffer espBuffer = state.Registers[RegisterName.ESP].PointsTo;
			Assert.AreEqual(0x10, espBuffer[0].Value);
		}

		[Test]
		public void LeaEdxFromEaxPlus16()
		{ //  lea    edx,[eax+index]
			Byte index = 0x1;
			Byte[] code = new Byte[] {0x8d, 0x50, index};

			AbstractValue zero = new AbstractValue(0);
			AbstractValue one = new AbstractValue(1);
			AbstractValue[] values = new AbstractValue [] {zero, one};
			AbstractBuffer buffer = new AbstractBuffer(values);
			state.Registers[RegisterName.EAX] = new AbstractValue(buffer);

			state = X86emulator.Run(state, code);
			AbstractValue edx = state.Registers[RegisterName.EDX];
			Assert.IsNotNull(edx);
			Assert.AreEqual(one, edx.PointsTo[0]);
		}
		
		[Test]
		public void JnzPlus6()
		{
			// cmp eax, 0
			Byte [] cmpCode = new Byte[] {0x83, 0xf8, 0x0};
			state.Registers[RegisterName.EAX] = new AbstractValue(0);
			state = X86emulator.Run(state, cmpCode);

			Byte offset = 0x06;
			Byte [] jnzCode = new Byte[] {0x75, offset};
			UInt32 oldEIP = state.InstructionPointer;
			state = X86emulator.Run(state, jnzCode);

			Assert.AreEqual(jnzCode.Length, state.InstructionPointer - oldEIP);

			state.Registers[RegisterName.EAX] = new AbstractValue(1);
			state = X86emulator.Run(state, cmpCode);
			
			oldEIP = state.InstructionPointer;
			state = X86emulator.Run(state, jnzCode);
			Assert.AreEqual(jnzCode.Length + offset, state.InstructionPointer - oldEIP);	
		}
	
	}
}
