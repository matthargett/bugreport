// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
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
			foreach (RegisterName register in Enum.GetValues(typeof(RegisterName))) 
			{
				Assert.AreNotSame(newRegisters[register], registers[register]);
			}
			
			Assert.AreNotSame(newRegisters[RegisterName.ESP].PointsTo, registers[RegisterName.ESP].PointsTo);
		}
		
		[Test]
		public void DefaultRegistersContainUninitializedValues() 
		{
			foreach (RegisterName register in RegisterName.GetValues(typeof(RegisterName))) 
			{
				Assert.IsFalse(registers[register].IsInitialized);
			}			
		}
		
		[Test]
		public void ToStringOutput()
		{
			StringAssert.StartsWith("EAX=?", registers.ToString());
		}
	}
}
