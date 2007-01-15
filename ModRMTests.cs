// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	[TestFixture]
	public class ModRMTests
	{
		Byte[] code;
		
		[Test]
		public void EaxEaxNoIndex()
		{ //  mov    eax,DWORD PTR [eax]
			code = new Byte[] { 0x8b, 0x00 };
			Assert.AreEqual(RegisterName.EAX, ModRM.GetEv(code));
			Assert.AreEqual(RegisterName.EAX, ModRM.GetGv(code));
			Assert.IsFalse(ModRM.HasIndex(code));
			Assert.IsTrue(ModRM.IsEffectiveAddressDereferenced(code));
		}
		
		[Test]
		public void EaxEbpPlusSByte12()
		{	//  mov    eax,DWORD PTR [ebp+index]
			Int32 index =  0x0c;
			code = new Byte[] { 0x8b, 0x45, (Byte)index};
			Assert.AreEqual(RegisterName.EBP, ModRM.GetEv(code));
			Assert.AreEqual(RegisterName.EAX, ModRM.GetGv(code));
			Assert.AreEqual(index, ModRM.GetIndex(code));
			
		}

		[Test]
		public void EaxEbx4()
		{ //  mov    eax,DWORD PTR [ebx-4]
			code = new Byte[] { 0x8b, 0x43, 0xfc };
			Assert.AreEqual(RegisterName.EBX, ModRM.GetEv(code));
			Assert.AreEqual(RegisterName.EAX, ModRM.GetGv(code));
			Assert.AreEqual(0xfc, ModRM.GetIndex(code));
		}
		
		[Test]
		public void EbxEsp0()
		{ //  mov    ebx,DWORD PTR [esp]
			code = new Byte[] { 0x8b, 0x1c, 0x24 };
		//	Assert.AreEqual(RegisterName.ESP, ModRM.GetEv(code));
			Assert.IsTrue(ModRM.HasSIB(code));
			Assert.AreEqual(RegisterName.EBX, ModRM.GetGv(code));
			Assert.IsFalse(ModRM.HasIndex(code));
		}
		
		[Test]
		public void EbxEbp12()
		{ //  mov    ebx,DWORD PTR [ebp-12]
			code = new Byte[] { 0x8b, 0x5d, 0xf4 };
			Assert.AreEqual(RegisterName.EBP, ModRM.GetEv(code));
			Assert.AreEqual(RegisterName.EBX, ModRM.GetGv(code));
			Assert.IsFalse(ModRM.HasSIB(code));
			Assert.AreEqual(0xf4, ModRM.GetIndex(code));
		}
		
		[Test]
		public void EdiEbp4()
		{ // mov    edi,DWORD PTR [ebp-4]
			code = new Byte[] { 0x8b, 0x7d, 0xfc };
			Assert.AreEqual(RegisterName.EBP, ModRM.GetEv(code));
			Assert.AreEqual(RegisterName.EDI, ModRM.GetGv(code));
			Assert.AreEqual(0xfc, ModRM.GetIndex(code));
		}
		
		[Test]
		public void EsiEbp8()
		{ //  mov    esi,DWORD PTR [ebp-8]
			code = new Byte[] { 0x8b, 0x75, 0xf8 };
			Assert.AreEqual(RegisterName.EBP, ModRM.GetEv(code));
			Assert.AreEqual(RegisterName.ESI, ModRM.GetGv(code));
			Assert.AreEqual(0xf8, ModRM.GetIndex(code));
		}

		[Test]
		[ExpectedException(typeof(NotImplementedException))]
		public void EaxEbpPlusDword12()
		{
			Int32 index =  0x0c;
			code = new Byte[] { 0x8b, 0x85, (Byte)index, 0x00, 0x00, 0x00};
			Assert.IsTrue(ModRM.HasIndex(code));
			Assert.AreEqual(index, ModRM.GetIndex(code));
		}
		
		[Test]
		public void MovEbpEsp()
		{ // mov    ebp,esp
			code = new Byte[] { 0x89, 0xe5} ;
			Assert.AreEqual(RegisterName.EBP, ModRM.GetEv(code));
			Assert.AreEqual(RegisterName.ESP, ModRM.GetGv(code));
			Assert.IsFalse(ModRM.HasIndex(code));
			Assert.IsFalse(ModRM.IsEffectiveAddressDereferenced(code));
		}
		
		[Test]
		public void MovEsp0x10()
		{ // mov DWORD PTR [esp],0x10
			code = new Byte[] {0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};
			Assert.IsTrue(ModRM.HasSIB(code));
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetEVWithHasSIB()
		{
			code = new Byte[] {0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};
			Assert.IsTrue(ModRM.HasSIB(code));
			ModRM.GetEv(code);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetIndexWithNoIndex()
		{
			code = new Byte[] { 0x89, 0xe5} ;
			Assert.IsFalse(ModRM.HasIndex(code));
			ModRM.GetIndex(code);
		}

		[Test]
		public void TwoByteWithModRMNoIndex()
		{ //  movzx  ebx,BYTE PTR [eax]
			code = new Byte[] {0x0f, 0xb6, 0x18};
			Assert.AreEqual(RegisterName.EAX, ModRM.GetEv(code));
			Assert.AreEqual(RegisterName.EBX, ModRM.GetGv(code));
			Assert.IsFalse(ModRM.HasIndex(code));
			Assert.IsTrue(ModRM.IsEffectiveAddressDereferenced(code));
			
		}

		[Test]
		public void TwoByteWithModRMWithIndex()
		{ //  movzx  eax,BYTE PTR [ebp-5]
			code = new Byte[] {0x0f, 0xb6, 0x45, 0xfb};
			Assert.AreEqual(RegisterName.EBP, ModRM.GetEv(code));
			Assert.AreEqual(RegisterName.EAX, ModRM.GetGv(code));
			Assert.AreEqual(0xfb, ModRM.GetIndex(code));
			Assert.IsTrue(ModRM.IsEffectiveAddressDereferenced(code));
			
		}

		[Test]
		public void TwoByteWithModRMWithOnlyOffset()
		{ // movzx  edx,BYTE PTR ds:0x80495e0
			code = new Byte[] {0x0f, 0xb6, 0x15, 0xe0, 0x95, 0x04, 0x08};
			Assert.AreEqual(RegisterName.EDX, ModRM.GetGv(code));
			Assert.IsFalse(ModRM.HasIndex(code));
			Assert.IsTrue(ModRM.IsEffectiveAddressDereferenced(code));
			Assert.IsTrue(ModRM.HasOffset(code));	
		}

		[Test]
		public void GvEbOffsetToRegister()
		{ // movzx  edx,BYTE PTR ds:0x80495e0
			code = new Byte[] {0x0f, 0xb6, 0x15, 0xe0, 0x95, 0x04, 0x08};
			Assert.AreEqual(RegisterName.EDX, ModRM.GetGv(code));
			Assert.IsFalse(ModRM.HasIndex(code));
			Assert.IsTrue(ModRM.IsEffectiveAddressDereferenced(code));
			Assert.IsTrue(ModRM.HasOffset(code));	
		}
	}
}
