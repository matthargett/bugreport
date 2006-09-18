// Copyright (c) 2006 Luis Miras
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	[TestFixture]
	public class X86emulatorTests
	{
		Byte[] code;
		X86emulator x86emulator; 
		UInt32 oldStackSize;	
		
		[SetUp]
		public void SetUp()
		{
			x86emulator = new X86emulator();
			AbstractBuffer buffer = new AbstractBuffer(new AbstractValue[0x200]);
			AbstractValue pointer = new AbstractValue(buffer);
			x86emulator.Registers[RegisterName.ESP] = pointer;
			
			oldStackSize = x86emulator.StackSize;
			reportedInstructionPointer = 0xdead1337;
		}
		
		[Test]
		public void InitialRegisters()
		{
			AbstractValue value = new AbstractValue(1);
			RegisterCollection registers = new RegisterCollection();
			registers[RegisterName.EAX] = value;
			x86emulator = new X86emulator(registers);
			Assert.AreEqual(value, x86emulator.Registers[RegisterName.EAX]);
		}
		
		[Test]
		public void PushEbp()
		{
			code = new Byte[] {0x55};
			x86emulator.Run(code);

			Assert.AreEqual(0x1, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize + 1, x86emulator.StackSize);
		}
		
		[Test]
		public void PushEbx()
		{
			code = new Byte[] {0x53};
			x86emulator.Run(code);

			Assert.AreEqual(0x1, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize + 1, x86emulator.StackSize);
		}

		[Test]
		public void DefaultValues()
		{
			Assert.AreEqual(0x0, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize, x86emulator.StackSize);
		}

		[Test]
		public void MovEbpEsp()
		{ // mov    ebp,esp
			code = new Byte[] { 0x89, 0xe5} ;
			UInt32 value = 0x31337;
			AbstractValue abstractValue = new AbstractValue(value);
			x86emulator.Registers[RegisterName.ESP] = abstractValue;
			x86emulator.Run(code);

			Assert.AreEqual(0x2, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize, x86emulator.StackSize);
			Assert.AreEqual(value, x86emulator.Registers[RegisterName.ESP].Value);
			Assert.AreEqual(value, x86emulator.Registers[RegisterName.EBP].Value);
		}
		
		[Test]
		public void MovEaxEaxFourByteValue()
		{
			UInt32 value = 0x31337;
			AbstractValue[] buffer = new AbstractValue[] {new AbstractValue(value)};
			AbstractValue pointer = new AbstractValue(buffer);
			x86emulator.Registers[RegisterName.EAX] = pointer;

			code = new Byte[] {0x8b, 0x00} ;
			x86emulator.Run(code);

			Assert.AreEqual(0x2, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize, x86emulator.StackSize);
			Assert.AreEqual(value, x86emulator.Registers[RegisterName.EAX].Value);
		}
		
		[Test]
		public void MovEaxEaxPointerPointerValue()
		{
			UInt32 value = 0x31337;
			AbstractValue argv = new AbstractValue(value);
			AbstractValue[] argvBuffer = new AbstractValue[] {argv};
			AbstractValue argvPointer = new AbstractValue(argvBuffer);
			x86emulator.Registers[RegisterName.EAX] = argvPointer;

			code = new Byte[] {0x8b, 0x00} ;
			x86emulator.Run(code);

			Assert.AreEqual(0x2, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize, x86emulator.StackSize);
			Assert.AreEqual(value, x86emulator.Registers[RegisterName.EAX].Value);
		}
		
		[Test]
		public void MovEaxEbpPlusTwelve()
		{
			AbstractValue[] buffer = new AbstractValue[16];
			buffer[12] = new AbstractValue(1);
			
			AbstractValue pointer = new AbstractValue(buffer);
			x86emulator.Registers[RegisterName.EBP] = pointer;

			code = new Byte[] {0x8b, 0x45, 0xc} ;
			x86emulator.Run(code);

			Assert.AreEqual(0x3, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize, x86emulator.StackSize);
			Assert.AreEqual(1, x86emulator.ReturnValue.Value);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void EmptyCodeArray()
		{
			code = new Byte[] {};
			x86emulator.Run(code);
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOpcodeException))]
		public void InvalidOpcode()
		{
			x86emulator.Run(new Byte[] {0xfe});
		}
		

		[Test]
		public void Sub()
		{	
			code = new Byte[] {0x83, 0xec, 0x08};
			x86emulator.Registers[RegisterName.ESP] = new AbstractValue(0x0a);
			x86emulator.Run(code);
			Assert.AreEqual(0x3, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize, x86emulator.StackSize);
			Assert.AreEqual(0x02, x86emulator.Registers[RegisterName.ESP].Value);
		}

		[Test]
		public void MovPtrEsp0x10()
		{
			code = new Byte[] {0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};
			x86emulator.Run(code);
			Assert.AreEqual(0x7, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize, x86emulator.StackSize);
			Assert.AreEqual(0x10, x86emulator.TopOfStack.Value);
		}

		[Test]
		public void MovEax0x0()
		{
			code = new Byte[] {0xb8, 0x00, 0x00, 0x00, 0x00};
			x86emulator.Run(code);
			Assert.AreEqual(code.Length, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize, x86emulator.StackSize);
			Assert.AreEqual(0x0, x86emulator.Registers[RegisterName.EAX].Value);
		}

		[Test]
		public void AddEax0x1()
		{
			code = new Byte[] {0x05, 0x01, 0x00, 0x00, 0x00};
			x86emulator.Registers[RegisterName.EAX] = new AbstractValue(1);
			x86emulator.Run(code);
			Assert.AreEqual(code.Length, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize, x86emulator.StackSize);
			Assert.AreEqual(0x2, x86emulator.Registers[RegisterName.EAX].Value);
		}

		[Test]
		public void MovEax0xXXXXXXXX()
		{
			code = new Byte[] {0xb8, 0x37, 0x13, 0x03, 0x00};
			x86emulator.Run(code);
			Assert.AreEqual(code.Length, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize, x86emulator.StackSize);
			Assert.AreEqual(0x00031337, x86emulator.ReturnValue.Value);
		}

		[Test]
		public void MovEsp0xXXXXXXXX()
		{
			code = new Byte[] {0xc7, 0x04, 0x24, 0x37, 0x13, 0x03, 0x00};
			x86emulator.Run(code);
			Assert.AreEqual(0x7, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize, x86emulator.StackSize);
			Assert.AreEqual(0x00031337, x86emulator.TopOfStack.Value);
		}
		
		[Test]
		public void ReturnValueIsAliasedToEax()
		{
			AbstractValue value = new AbstractValue(0x31337);
			x86emulator.ReturnValue = value;
			Assert.AreEqual(value, x86emulator.Registers[RegisterName.EAX]);
		}

		[Test]
		public void Call()
		{
			//TODO: should take AbstractValue rather than UInt32
			//TODO: need to reconcile with esp/ebp handling
			x86emulator.PushOntoStack(16);
			code = new Byte[] {0xe8, 0x14, 0xff, 0xff, 0xff};
			x86emulator.Run(code);
			Assert.AreEqual(0x5, x86emulator.InstructionPointer);
			Assert.AreEqual(oldStackSize, x86emulator.StackSize);
			Assert.AreEqual(16, x86emulator.ReturnValue.PointsTo.Length);
		}
		
		UInt32 reportedInstructionPointer;
		private void onReportOOB(Object sender, NewReportEventArgs e)
		{
			reportedInstructionPointer = e.InstructionPointer;
		}

		[Test]
		public void MovIntoAssignedEaxOutOfBounds()
		{
			Byte value = 0x01, index = 0x10;
			AbstractValue[] buffer = new AbstractValue[index];
			AbstractValue pointer = new AbstractValue(buffer);
			x86emulator.ReturnValue = pointer;
			code = new Byte[] {0xc6, 0x40, index, value};
			x86emulator.NewReport += onReportOOB;
			x86emulator.Run(code);
			Assert.AreEqual(0, reportedInstructionPointer);			
		}

		[Test]
		public void MovIntoAssignedEaxInBounds()
		{
			Byte value = 0x01, index = 0xf;
			AbstractValue[] buffer = new AbstractValue[16];
			AbstractValue pointer = new AbstractValue(buffer);
			x86emulator.ReturnValue = pointer;
			code = new Byte[] {0xc6, 0x40, index, value};
			x86emulator.Run(code);
			Assert.AreEqual(value, x86emulator.ReturnValue.PointsTo[index].Value);
		}
		
		[Test]
		public void MovBytePtrEaxInBounds()
		{ //	mov    BYTE PTR [eax], value
			Byte value = 0x01;
			AbstractValue[] buffer = new AbstractValue[16];
			AbstractValue pointer = new AbstractValue(buffer);
			x86emulator.ReturnValue = pointer;
			code = new Byte[] {0xc6, 0x00, value};
			x86emulator.Run(code);
			Assert.AreEqual(value, x86emulator.ReturnValue.PointsTo[0].Value);
		}
		
		[Test]
		public void MovBytePtrEaxPlus16FromBl()
		{ // mov    BYTE PTR [eax+16],bl
			Byte offset = 0x10;
			x86emulator.Registers[RegisterName.EBX] = new AbstractValue(0x1);
			AbstractValue[] buffer = new AbstractValue[offset+1];
			AbstractValue pointer = new AbstractValue(buffer);
			x86emulator.ReturnValue = pointer;

			code = new Byte[] {0x88, 0x58, offset};
			x86emulator.Run(code);

			Assert.AreEqual(0x1, x86emulator.ReturnValue.PointsTo[offset].Value);
		}

		[Test]
		public void MovDwordPtrEaxPlus16FromEbx()
		{ // mov    BYTE PTR [eax+16],bl
			Byte offset = 0x10;
			x86emulator.Registers[RegisterName.EBX] = new AbstractValue(0x1);
			AbstractValue[] buffer = new AbstractValue[offset+1];
			AbstractValue pointer = new AbstractValue(buffer);
			x86emulator.Registers[RegisterName.EAX] = pointer;

			code = new Byte[] {0x89, 0x58, offset};
			x86emulator.Run(code);

			Assert.AreEqual(0x1, x86emulator.Registers[RegisterName.EAX].PointsTo[offset].Value);
		}

		[Test]
		public void MovIntoOffsetFromAl()
		{ // mov    ds:0x80495e0,al
			UInt32 value = 1;
			UInt32 offset = 0x80495e0;
			code = new Byte[] {0xa2, 0xe0, 0x95, 0x04, 0x08};
			x86emulator.Registers[RegisterName.EAX] = new AbstractValue(value);
			x86emulator.Run(code);
			Assert.AreEqual(value, x86emulator.DataSegment[offset].Value);
		}

		[Test]
		public void MovIntoEdxFromOffset()
		{ // movzx  edx,BYTE PTR ds:0x80495e0
			UInt32 value = 1;
			AbstractValue abstractValue = new AbstractValue(value);
			x86emulator.DataSegment[0x80495e0] = abstractValue;
			code = new Byte[] {0x0f, 0xb6, 0x15, 0xe0, 0x95, 0x04, 0x08};
			x86emulator.Run(code);
			Assert.AreEqual(value, x86emulator.Registers[RegisterName.EDX].Value);
			
			
		}

		[Test]
		public void AddImmediateToEAX()
		{ // add    eax,0xf
			Byte immediate = 0x0f;
			code = new Byte[] {0x83, 0xc0, immediate};
			UInt32 value = 1;
			x86emulator.Registers[RegisterName.EAX] = new AbstractValue(value);
			x86emulator.Run(code);
			Assert.AreEqual(value + immediate, x86emulator.Registers[RegisterName.EAX].Value);	
		}

		[Test]
		public void MovEax0x10()
		{ // mov    DWORD PTR [eax],0x10
		
			AbstractValue[] value = new AbstractValue [1];
			x86emulator.Registers[RegisterName.EAX] = new AbstractValue(value);
			code = new Byte[] {0xc7, 0x00, 0x10, 0x00, 0x00, 0x00};
			x86emulator.Run(code);

			AbstractValue sixteen = x86emulator.Registers[RegisterName.EAX].PointsTo[0];
			Assert.AreEqual(0x10, sixteen.Value);
			                
		}

		[Test]
		public void MovEbpMinus8()
		{ // mov    DWORD PTR [ebp-8],0xf
		
			AbstractValue[] value = new AbstractValue [0x100];
			x86emulator.Registers[RegisterName.EBP] = new AbstractValue(value);
			code = new Byte[] {0xc7, 0x45, 0xf8, 0x0f, 0x00, 0x00, 0x00};
			x86emulator.Run(code);

			AbstractValue fifteen = x86emulator.Registers[RegisterName.EBP].PointsTo[0xf8];
			Assert.AreEqual(0xf, fifteen.Value);
			                
		}
		
		[Test]
		public void MovPtrEspEax()
		{ // mov [esp]. eax
			Byte[] code = new Byte[] {0x89, 0x04, 0x24};
			x86emulator.Registers[RegisterName.EAX] = new AbstractValue(0x10);
			AbstractValue[] values = new AbstractValue[] {new AbstractValue(1)};
			AbstractBuffer buffer = new AbstractBuffer(values);
			x86emulator.Registers[RegisterName.ESP] = new AbstractValue(buffer);
			
			x86emulator.Run(code);
			
			AbstractBuffer espBuffer = x86emulator.Registers[RegisterName.ESP].PointsTo;
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
			x86emulator.Registers[RegisterName.EAX] = new AbstractValue(buffer);
				
			x86emulator.Run(code);
			Assert.AreEqual(one, x86emulator.Registers[RegisterName.EDX].PointsTo[0]);
		}
	
	}
}
