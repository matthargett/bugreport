// Copyright (c) 2006 Luis Miras
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	[TestFixture]
	public class OpcodeHelperTests
	{
		private Byte[] code;
		private OpcodeEncoding encoding;
		
		[Test]
		public void EvGv()
		{
			code = new Byte[] { 0x89, 0x00 };
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvGv, encoding);
		}

		[Test]
		public void GvEv()
		{
			code = new Byte[] { 0x8b, 0x00 };
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.GvEv, encoding);
		}
	
		[Test]
		[ExpectedException(typeof(InvalidOpcodeException))]
		public void UnknownOpcode()
		{
			code = new Byte[] { 0xf0, 0x00 };
			OpcodeHelper.GetEncoding(code);
		}
		
		[Test]
		public void None()
		{
			code = new Byte[] {0x90};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.None, encoding);			
		}

		[Test]
		public void RAXIv()
		{
			code = new Byte[] {0xb8, 0x00, 0x00, 0x00, 0x00};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rAXIv, encoding);			
		}

		[Test]
		public void EvIz()
		{
			code = new Byte[] {0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvIz, encoding);			
		}
		
		[Test]
		public void Jz()
		{
			code = new Byte[] {0xe8, 0x14, 0xff, 0xff, 0xff};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.Jz, encoding);			
		}
		
		[Test]
		public void PushrBP()
		{
			code = new Byte[] {0x55};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rBP, encoding);
			Assert.AreEqual(StackEffect.Push, OpcodeHelper.GetStackEffect(code));
		}
		
		[Test]
		public void PushrBX()
		{
			code = new Byte[] {0x53};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rBX, encoding);
			Assert.AreEqual(StackEffect.Push, OpcodeHelper.GetStackEffect(code));
		}
		
		[Test]
		public void EbIb()
		{
			code = new Byte[]  {0xc6, 0x40, 0x10, 0x00};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EbIb, encoding);
		}
		
		[Test]
		public void GvEbRegisterToRegister()
		{ //  movzx  ebx,BYTE PTR [eax]
			code = new Byte[] {0x0f, 0xb6, 0x18};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.GvEb, encoding);
			
			OperatorEffect operatorEffect = OpcodeHelper.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
		}
		
		[Test]
		public void EbGb()
		{ // mov    BYTE PTR [eax+16],bl
			code = new Byte[] {0x88, 0x58, 0x10};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EbGb, encoding);

			OperatorEffect operatorEffect = OpcodeHelper.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
		}
		
		[Test]
		public void ObAL()
		{ // mov    ds:0x80495e0,al
			code = new Byte[] {0xa2, 0xe0, 0x95, 0x04, 0x08};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.ObAL, encoding);

			OperatorEffect operatorEffect = OpcodeHelper.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
		}
		
		[Test]
		public void GvEbOffsetToRegister()
		{ // movzx  edx,BYTE PTR ds:0x80495e0
			code = new Byte[] {0x0f, 0xb6, 0x15, 0xe0, 0x95, 0x04, 0x08};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.GvEb, encoding);		

			OperatorEffect operatorEffect = OpcodeHelper.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
		}
		
		[Test]
		public void EvIbAdd()
		{ //  83 c0 0f                add    eax,0xf
			code = new Byte[] {0x83, 0xc0, 0x0f};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvIb, encoding);		

			OperatorEffect operatorEffect = OpcodeHelper.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Add, operatorEffect);
		}
		
		[Test]
		public void EvIbSub()
		{	
			code = new Byte[] {0x83, 0xec, 0x08};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvIb, encoding);		

			OperatorEffect operatorEffect = OpcodeHelper.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Sub, operatorEffect);
		}
		
		[Test]
		public void EvIbAnd()
		{	
			code = new Byte[] {0x83, 0xe4, 0xf0};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvIb, encoding);		

			OperatorEffect operatorEffect = OpcodeHelper.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.And, operatorEffect);
		}

		[Test]
		public void EvIbShr()
		{	
			code = new Byte[] {0xc1, 0xe8, 0x04};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvIb, encoding);		

			OperatorEffect operatorEffect = OpcodeHelper.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Shr, operatorEffect);
		}

		[Test]
		public void EvIbShl()
		{	
			code = new Byte[] {0xc1, 0xe0, 0x04};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvIb, encoding);		

			OperatorEffect operatorEffect = OpcodeHelper.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Shl, operatorEffect);
		}

		[Test]
		public void EvGvSub()
		{	
			code = new Byte[] {0x29, 0xc4};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.EvGv, encoding);		

			OperatorEffect operatorEffect = OpcodeHelper.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Sub, operatorEffect);
		}
		
		[Test]
		public void rAxOv()
		{
			code = new Byte[] {0xa1, 0xe4, 0x84, 0x04, 0x08};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.rAxOv, encoding);		

			OperatorEffect operatorEffect = OpcodeHelper.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
			
		}

		[Test]
		public void GvM()
		{
			code = new Byte[] {0x8d, 0x50, 0x10};
			encoding = OpcodeHelper.GetEncoding(code);
			Assert.AreEqual(OpcodeEncoding.GvM, encoding);		

			OperatorEffect operatorEffect = OpcodeHelper.GetOperatorEffect(code);
			Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
			
		}

	}
}
