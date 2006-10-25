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
		RegisterCollection collection;
		
		[SetUp]
		public void SetUp()
		{
			 collection = new RegisterCollection();
		}
		
		[Test]
		public void DefaultRegistersContainUninitializedValues() 
		{
			foreach (RegisterName register in RegisterName.GetValues(typeof(RegisterName))) {
				Assert.IsFalse(collection[register].IsInitialized);
			}			
		}
		
		[Test]
		public void ToStringOutput()
		{
			StringAssert.StartsWith(" EAX=", collection.ToString());
		}
	}
}
