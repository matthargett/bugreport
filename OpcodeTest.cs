﻿// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	[TestFixture]
	public class OpcodeTest
	{
		private Byte[] code;
		private OpcodeEncoding encoding;
		
		[Test]
		public void EvGv()
		{
			code = new Byte[] { 0x89, 0x00 };
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvGv, encoding);
		}
		
		[Test]
		public void XorEvGv()
		{
			//Xor EBP,EBP
			code = new Byte[] { 0x31, 0xed };
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvGv, encoding);
			Assert.AreEqual(OperatorEffect.Xor, Opcode.GetOperatorEffect(code));
			Assert.AreEqual(RegisterName.EBP, Opcode.GetDestinationRegister(code));
			Assert.AreEqual(RegisterName.EBP, Opcode.GetSourceRegister(code));
		}
		

		[Test]
		public void PushIz()
		{
			code = new Byte[] {0x68, 0x10, 0x84, 0x04, 0x08};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.Iz, encoding);
			Assert.AreEqual(StackEffect.Push, Opcode.GetStackEffect(code));
			Assert.IsFalse(Opcode.HasSourceRegister(code));
			Assert.IsFalse(Opcode.HasDestinationRegister(code));
			Assert.IsTrue(Opcode.HasImmediate(code));
			Assert.AreEqual(0x08048410, Opcode.GetImmediate(code));
		}
		
		[Test]
		public void PoprSI()
		{
			//Pop rSI
			code = new Byte[] { 0x5e};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rSI, encoding);
			Assert.AreEqual(StackEffect.Pop, Opcode.GetStackEffect(code));
			Assert.IsFalse(Opcode.HasSourceRegister(code));
			Assert.IsTrue(Opcode.HasDestinationRegister(code));
			Assert.AreEqual(RegisterName.ESI, Opcode.GetDestinationRegister(code));
		}

		[Test]
		public void PushrCX()
		{
			code = new Byte[] {0x51};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rCX, encoding);
			Assert.AreEqual(StackEffect.Push, Opcode.GetStackEffect(code));
			Assert.IsTrue(Opcode.HasSourceRegister(code));
			Assert.IsFalse(Opcode.HasDestinationRegister(code));
			Assert.AreEqual(RegisterName.ECX, Opcode.GetSourceRegister(code));
		}
				
		[Test]
		public void PoprCX()
		{
			code = new Byte[] { 0x59};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rCX, encoding);
			Assert.AreEqual(StackEffect.Pop, Opcode.GetStackEffect(code));
			Assert.IsFalse(Opcode.HasSourceRegister(code));
			Assert.IsTrue(Opcode.HasDestinationRegister(code));
			Assert.AreEqual(RegisterName.ECX, Opcode.GetDestinationRegister(code));
		}

		[Test]
		public void PushrSI()
		{
			//Push rSI
			code = new Byte[] { 0x56};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rSI, encoding);
			Assert.AreEqual(StackEffect.Push, Opcode.GetStackEffect(code));
			Assert.IsTrue(Opcode.HasSourceRegister(code));
			Assert.IsFalse(Opcode.HasDestinationRegister(code));
			Assert.AreEqual(RegisterName.ESI, Opcode.GetSourceRegister(code));
		}
		
		[Test]
		public void PushrAX()
		{
			//Push rAX
			code = new Byte[] { 0x50};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rAX, encoding);
			Assert.AreEqual(StackEffect.Push, Opcode.GetStackEffect(code));
			Assert.IsTrue(Opcode.HasSourceRegister(code));
			Assert.IsFalse(Opcode.HasDestinationRegister(code));
			Assert.AreEqual(RegisterName.EAX, Opcode.GetSourceRegister(code));
		}
		
		[Test]
		public void PushrDX()
		{
			code = new Byte[] {0x52};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rDX, encoding);
			Assert.AreEqual(StackEffect.Push, Opcode.GetStackEffect(code));
			Assert.IsTrue(Opcode.HasSourceRegister(code));
			Assert.IsFalse(Opcode.HasDestinationRegister(code));
			Assert.AreEqual(RegisterName.EDX, Opcode.GetSourceRegister(code));
		}
		
		[Test]
		public void PoprAX()
		{
			//Pop rAX
			code = new Byte[] { 0x58};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rAX, encoding);
			Assert.AreEqual(StackEffect.Pop, Opcode.GetStackEffect(code));
			Assert.IsFalse(Opcode.HasSourceRegister(code));
			Assert.IsTrue(Opcode.HasDestinationRegister(code));
			Assert.AreEqual(RegisterName.EAX, Opcode.GetDestinationRegister(code));
		}
		
		[Test]
		public void PoprDX()
		{
			code = new Byte[] {0x5a};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rDX, encoding);
			Assert.AreEqual(StackEffect.Pop, Opcode.GetStackEffect(code));
			Assert.IsFalse(Opcode.HasSourceRegister(code));
			Assert.IsTrue(Opcode.HasDestinationRegister(code));
			Assert.AreEqual(RegisterName.EDX, Opcode.GetDestinationRegister(code));
		}

		[Test]
		public void PushrSP()
		{
			//Push ESP
			code = new Byte[] { 0x54};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rSP, encoding);
			Assert.AreEqual(StackEffect.Push, Opcode.GetStackEffect(code));
			Assert.IsTrue(Opcode.HasSourceRegister(code));
			Assert.IsFalse(Opcode.HasDestinationRegister(code));
			Assert.AreEqual(RegisterName.ESP, Opcode.GetSourceRegister(code));
			Assert.AreEqual(RegisterName.None, Opcode.GetDestinationRegister(code));
		}
		
		[Test]
		public void PoprSP()
		{
			//Push ESP
			code = new Byte[] { 0x5c};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rSP, encoding);
			Assert.AreEqual(StackEffect.Pop, Opcode.GetStackEffect(code));
			Assert.IsFalse(Opcode.HasSourceRegister(code));
			Assert.IsTrue(Opcode.HasDestinationRegister(code));
			Assert.AreEqual(RegisterName.None, Opcode.GetSourceRegister(code));
			Assert.AreEqual(RegisterName.ESP, Opcode.GetDestinationRegister(code));
		}

		[Test]
		public void GvEv()
		{ // mov eax, [eax]
			code = new Byte[] { 0x8b, 0x00 };
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.GvEv, encoding);
			Assert.AreEqual(RegisterName.EAX, Opcode.GetSourceRegister(code));
			Assert.AreEqual(RegisterName.EAX, Opcode.GetDestinationRegister(code));
		}

		[Test]
		public void GvEvDword()
		{ // mov eax, [0x11223344]
			code = new Byte[] { 0x8b, 0x05, 0x44, 0x33, 0x22, 0x11 };
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.GvEv, encoding);
			Assert.AreEqual(RegisterName.None, Opcode.GetSourceRegister(code));
			Assert.AreEqual(RegisterName.EAX, Opcode.GetDestinationRegister(code));
		}

		[Test]
		public void GvEvSIB()
		{ // mov eax, [base_register+scale_register*scaler
			code = new Byte[] { 0x8b, 0x04, 0x24 };
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.GvEv, encoding);
			Assert.AreEqual(RegisterName.None, Opcode.GetSourceRegister(code));
			Assert.AreEqual(RegisterName.EAX, Opcode.GetDestinationRegister(code));
		}


		[Test]
		[ExpectedException(typeof(InvalidOpcodeException))]
		public void UnknownOpcode()
		{
			code = new Byte[] { 0xf0, 0x00 };
			Opcode.GetEncoding(code);
		}
		
		[Test]
		public void None()
		{
			code = new Byte[] {0x90};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.None, encoding);
			Assert.AreEqual(StackEffect.None, Opcode.GetStackEffect(code));
		}

		[Test]
		public void Halt()
		{
			code = new Byte[] {0xf4};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.None, encoding);
			Assert.AreEqual(StackEffect.None, Opcode.GetStackEffect(code));
		}

		[Test]
		public void RAXIv()
		{
			code = new Byte[] {0xb8, 0x00, 0x00, 0x00, 0x00};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rAxIv, encoding);
			Assert.AreEqual(RegisterName.EAX, Opcode.GetDestinationRegister(code));
		}

		[Test]
		public void EvIz()
		{
			code = new Byte[] {0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvIz, encoding);
			Assert.IsTrue(Opcode.HasDestinationRegister(code));
			Assert.AreEqual(RegisterName.ESP, Opcode.GetDestinationRegister(code));
			Assert.AreEqual(0x10, Opcode.GetImmediate(code));
		}
		
		[Test]
		public void Jz()
		{
			code = new Byte[] {0xe8, 0x14, 0xff, 0xff, 0xff};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.Jz, encoding);	
			Assert.IsTrue(Opcode.HasImmediate(code));
			Assert.AreEqual(0xffffff14, Opcode.GetImmediate(code));
		}
		
		[Test]
		public void PushrBP()
		{
			code = new Byte[] {0x55};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rBP, encoding);
			Assert.AreEqual(StackEffect.Push, Opcode.GetStackEffect(code));
			Assert.AreEqual(RegisterName.None, Opcode.GetDestinationRegister(code));
			Assert.AreEqual(RegisterName.EBP, Opcode.GetSourceRegister(code));
		}
		
		[Test]
		public void PoprBP()
		{
			code = new Byte[] {0x5d};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rBP, encoding);
			Assert.AreEqual(StackEffect.Pop, Opcode.GetStackEffect(code));
			Assert.AreEqual(RegisterName.None, Opcode.GetSourceRegister(code));
			Assert.AreEqual(RegisterName.EBP, Opcode.GetDestinationRegister(code));
		}
		
		[Test]
		public void PushrBX()
		{
			code = new Byte[] {0x53};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rBX, encoding);
			Assert.AreEqual(StackEffect.Push, Opcode.GetStackEffect(code));
			Assert.AreEqual(RegisterName.None, Opcode.GetDestinationRegister(code));
			Assert.AreEqual(RegisterName.EBX, Opcode.GetSourceRegister(code));
		}

		[Test]
		public void PoprBX()
		{
			code = new Byte[] {0x5b};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rBX, encoding);
			Assert.AreEqual(StackEffect.Pop, Opcode.GetStackEffect(code));
			Assert.AreEqual(RegisterName.EBX, Opcode.GetDestinationRegister(code));
			Assert.AreEqual(RegisterName.None, Opcode.GetSourceRegister(code));
		}
		
		[Test]
		public void EbIb()
		{
			Byte immediate = 0;
			code = new Byte[]  {0xc6, 0x40, 0x10, immediate};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EbIb, encoding);
			Assert.AreEqual(immediate, Opcode.GetImmediate(code));
		}
		
		[Test]
		public void GvEbRegisterToRegister()
		{ //  movzx  ebx,BYTE PTR [eax]
			code = new Byte[] {0x0f, 0xb6, 0x18};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.GvEb, encoding);
			
			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
			Assert.AreEqual(RegisterName.EAX, Opcode.GetSourceRegister(code));			
		}
		
		[Test]
		public void EbGb()
		{ // mov    BYTE PTR [eax+16],bl
			code = new Byte[] {0x88, 0x58, 0x10};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EbGb, encoding);

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
			Assert.AreEqual(RegisterName.EBX, Opcode.GetSourceRegister(code));
		}
		
		[Test]
		public void ObAL()
		{ // mov    ds:0x80495e0,al
			code = new Byte[] {0xa2, 0xe0, 0x95, 0x04, 0x08};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.ObAL, encoding);

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
		}
		
		[Test]
		public void GvEbOffsetToRegister()
		{ // movzx  edx,BYTE PTR ds:0x80495e0
			code = new Byte[] {0x0f, 0xb6, 0x15, 0xe0, 0x95, 0x04, 0x08};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.GvEb, encoding);		

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
		}
		
		[Test]
		public void EvIbAdd()
		{ //  83 c0 0f                add    eax,0xf
			code = new Byte[] {0x83, 0xc0, 0x0f};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvIb, encoding);		

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Add, operatorEffect);
		}
		
		[Test]
		public void EvIbSub()
		{	
			code = new Byte[] {0x83, 0xec, 0x08};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvIb, encoding);		

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Sub, operatorEffect);
		}
		
		[Test]
		public void EvIbAnd()
		{	
			code = new Byte[] {0x83, 0xe4, 0xf0};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvIb, encoding);		

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.And, operatorEffect);
		}

		[Test]
		public void EvIbCmp()
		{
			code = new Byte[] {0x83, 0x7d, 0x08, 0x01};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvIb, encoding);		

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Cmp, operatorEffect);
			
		}
		[Test]
		public void EvIbShr()
		{	
			code = new Byte[] {0xc1, 0xe8, 0x04};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvIb, encoding);		

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Shr, operatorEffect);
		}

		[Test]
		public void EvIbShl()
		{	
			code = new Byte[] {0xc1, 0xe0, 0x04};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvIb, encoding);		

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Shl, operatorEffect);
		}

		[Test]
		public void EvGvSub()
		{	
			code = new Byte[] {0x29, 0xc4};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvGv, encoding);		

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Sub, operatorEffect);
		}
		
		[Test]
		public void rAxOv()
		{
			code = new Byte[] {0xa1, 0xe4, 0x84, 0x04, 0x08};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rAxOv, encoding);		

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);			
		}

		[Test]
		public void GvM()
		{
			code = new Byte[] {0x8d, 0x50, 0x10};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.GvM, encoding);		

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);			
		}
		
		[Test]
		public void AnotherGvM()
		{
			code = new Byte[] {0x8d, 0x04, 0x02};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.GvM, encoding);		

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
			Assert.AreEqual(RegisterName.EAX, Opcode.GetDestinationRegister(code));
		}
		
		[Test]
		public void rAxIz()
		{
			code = new Byte[] {0x05, 0x04, 0x01, 0x00, 0x00};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rAxIz, encoding);		

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Add, operatorEffect);			
		}
		
		[Test]
		public void Jb()
		{
			code = new Byte[] {0x75, 0x06};
			encoding = Opcode.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.Jb, encoding);		

			OperatorEffect operatorEffect = Opcode.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Jnz, operatorEffect);						
		}
		
		[Test]
		public void LeaveReturn()
		{
			Assert.AreEqual(OperatorEffect.Leave, Opcode.GetOperatorEffect(new Byte[] {0xc9}));
			Assert.AreEqual(OperatorEffect.Return, Opcode.GetOperatorEffect(new Byte[] {0xc3}));
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void InvalidGetImmediate()
		{
			code = new Byte[] {0x90};
			Assert.IsFalse(Opcode.HasImmediate(code));
			Opcode.GetImmediate(code);
		}
	}
}
