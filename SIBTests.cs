// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	[TestFixture]
	public class SIBTests
	{
		Byte[] code;
		
		[Test]
		public void EvGvSIBNoIndexToEspFromEax()
		{
			code = new Byte[] {0x89, 0x04, 0x24};
			Assert.IsTrue(ModRM.HasSIB(code));
			Assert.AreEqual(RegisterName.ESP, SIB.GetBaseRegister(code));
			
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void HasSIBWhenNoModRMPresent()
		{
			code = new Byte[] { 0x00 };
			ModRM.HasSIB(code);
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetSIBWhenNoSIBPresent()
		{ // mov    ebp,esp
			code = new Byte[] { 0x89, 0xe5} ;
			Assert.IsFalse(ModRM.HasSIB(code));
			SIB.GetBaseRegister(code);
		}
		
	}
}
