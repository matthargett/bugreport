// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	[TestFixture]
	public class OpcodeFormatterTests
	{
		 
		[Test]
		public void MovPtrEsp10()
		{
			Byte[] code = new Byte[] { 0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00 };
			Assert.AreEqual("mov", OpcodeFormatter.GetInstructionName(code));
			Assert.AreEqual("[esp], 0x10", OpcodeFormatter.GetOperands(code));
			Assert.AreEqual("(EvIz)", OpcodeFormatter.GetEncoding(code));
		}
	

	}
}
