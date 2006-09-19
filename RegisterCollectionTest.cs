// Copyright (c) 2006 Luis Miras
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	[TestFixture]
	public class RegisterCollectionTest
	{				
		[Test]
		public void DefaultRegistersContainUninitializedValues() 
		{
			RegisterCollection collection = new RegisterCollection();			
			foreach (RegisterName register in RegisterName.GetValues(typeof(RegisterName))) {
				Assert.IsFalse(collection[register].IsInitialized);
			}			
		}						
	}
}
