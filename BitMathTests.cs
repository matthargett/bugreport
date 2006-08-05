// Copyright (c) 2006 Luis Miras
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	
	[TestFixture]
	public class BitMathTests
	{
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void NotEnoughBytesToDwordAtZeroIndex()
		{
			BitMath.BytesToDword(new Byte[] {0}, 0);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void NotEnoughBytesToDwordAtNonZeroIndex()
		{
			BitMath.BytesToDword(new Byte[] {0, 1, 2, 3}, 1);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void EmptyBytes()
		{
			BitMath.BytesToDword(new Byte[] {}, 0);
		}

		[Test]
		public void AllZeroes()
		{
			UInt32 result = BitMath.BytesToDword(new Byte[] {0, 0, 0, 0}, 0);
			Assert.AreEqual(0, result);			
		}

		[Test]
		public void OneTwoThreeFour()
		{
			UInt32 result = BitMath.BytesToDword(new Byte[] {1, 2, 3, 4}, 0);
			Assert.AreEqual(0x04030201, result);			
		}

	}
}
