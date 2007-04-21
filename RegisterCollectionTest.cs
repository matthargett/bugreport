// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	[TestFixture]
	public class RegisterCollectionTest
	{
		RegisterCollection registers;
		
		[SetUp]
		public void SetUp()
		{
			 registers = new RegisterCollection();
		}
		
		[Test]
		public void Copy()
		{
			registers[RegisterName.ESP] = new AbstractValue(new AbstractBuffer(AbstractValue.GetNewBuffer(10)));
			RegisterCollection newRegisters = new RegisterCollection(registers);
			for (UInt32 i = 0; i < 7; i++) 
			{
				RegisterName register = (RegisterName)i;
				Assert.AreNotSame(newRegisters[register], registers[register]);
			}
			
			Assert.AreNotSame(newRegisters[RegisterName.ESP].PointsTo, registers[RegisterName.ESP].PointsTo);
		}
		
		[Test]
		public void DefaultRegistersContainUninitializedValues() 
		{
			for (UInt32 i = 0; i < 7; i++) 
			{
				RegisterName register = (RegisterName)i;
				Assert.IsFalse(registers[register].IsInitialized);
			}			
		}
		
		[Test]
		public void ToStringOutput()
		{
			StringAssert.StartsWith("EAX=?", registers.ToString());
		}
		
		[Test]
		public void Equals()
		{
			Assert.IsFalse(registers.Equals(null));
		}
	}
}
