// Copyright (c) 2006 Luis Miras
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	[TestFixture]
	public class AbstractBufferTests
	{
		[Test]
		public void PointerAdd()
		{
			AbstractValue one = new AbstractValue(0x1);
			AbstractValue two = new AbstractValue(0x2);
			AbstractValue three = new AbstractValue(0x3);
			AbstractValue four = new AbstractValue(0x4);
			AbstractValue[] avBuffer = new AbstractValue[4];
			
			avBuffer[0] = one;
			avBuffer[1] = two;
			avBuffer[2] = three;
			avBuffer[3] = four;
			
			AbstractBuffer buffer = new AbstractBuffer(avBuffer);
			Assert.AreEqual(one, buffer[0]);
			AbstractBuffer modifiedBuffer = AbstractBuffer.Add(buffer, 2);
			Assert.AreEqual(three, modifiedBuffer[0]);
			
		}

		[Test]
		public void PointerSub()
		{
			AbstractValue one = new AbstractValue(0x1);
			AbstractValue two = new AbstractValue(0x2);
			AbstractValue three = new AbstractValue(0x3);
			AbstractValue four = new AbstractValue(0x4);
			AbstractValue[] avBuffer = new AbstractValue[4];
			
			avBuffer[0] = one;
			avBuffer[1] = two;
			avBuffer[2] = three;
			avBuffer[3] = four;
			
			AbstractBuffer buffer = new AbstractBuffer(avBuffer);
			Assert.AreEqual(one, buffer[0]);
			AbstractBuffer modifiedBuffer = AbstractBuffer.Add(buffer, 2);
			Assert.AreEqual(three, modifiedBuffer[0]);

			AbstractBuffer subbedBuffer = AbstractBuffer.Sub(modifiedBuffer, 2);
			Assert.AreEqual(one, subbedBuffer[0]);
		}
		
		[Test]
		public void PointerAnd()
		{
			AbstractValue one = new AbstractValue(0x1);
			AbstractValue two = new AbstractValue(0x2);
			AbstractValue three = new AbstractValue(0x3);
			AbstractValue four = new AbstractValue(0x4);
			AbstractValue[] avBuffer = new AbstractValue[4];
			
			avBuffer[0] = one;
			avBuffer[1] = two;
			avBuffer[2] = three;
			avBuffer[3] = four;
			
			AbstractBuffer buffer = new AbstractBuffer(avBuffer);
			Assert.AreEqual(one, buffer[0]);
			AbstractBuffer modifiedBuffer = AbstractBuffer.Add(buffer, 3);
			Assert.AreEqual(four, modifiedBuffer[0]);

			AbstractBuffer andedBuffer = AbstractBuffer.And(modifiedBuffer, 0xfffffff0);
			Assert.AreEqual(one, andedBuffer[0]);
		}
	
	}
}
