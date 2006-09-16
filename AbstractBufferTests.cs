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

        [Test]
        public void PointerOverflowByOne()
        {
            AbstractValue[] buffer = new AbstractValue[16];
            AbstractBuffer pointer = new AbstractBuffer(buffer);

            AbstractValue value = pointer[16];
            Assert.IsTrue(value.IsOOB);
            Assert.AreEqual(AbstractValue.UNKNOWN, value.Value);
        }

        [Test]
        public void PointerOverflowStillRetainsOldValues()
        {
            AbstractValue test1 = new AbstractValue(0x41);
            AbstractValue test2 = new AbstractValue(0x42);
            AbstractValue[] buffer = new AbstractValue[2];

            buffer[0] = test1;
            buffer[1] = test2;

            AbstractBuffer pointer = new AbstractBuffer(buffer);

            // Accessing pointer[2] will cause the AbstractBuffer to extend..
            Assert.IsTrue(pointer[2].IsOOB);
            Assert.AreEqual(AbstractValue.UNKNOWN, pointer[2].Value);

            // And then we make sure the in bounds values stay the same
            Assert.IsFalse(pointer[0].IsOOB);
            Assert.IsFalse(pointer[1].IsOOB);

            Assert.AreEqual(0x41, pointer[0].Value);
            Assert.AreEqual(0x42, pointer[1].Value);
        }

        [Test]
        public void PointerOverflowTwiceStillRetainsOriginalValues()
        {
            AbstractValue[] buffer = new AbstractValue[16];
            AbstractBuffer pointer = new AbstractBuffer(buffer);

            //Access beyond buffer bounds forcing buffer to expand
            Assert.IsTrue(pointer[17].IsOOB);
            Assert.AreEqual(AbstractValue.UNKNOWN, pointer[17].Value);

            pointer[17] = new AbstractValue(0x41414141);

            // Access beyond previously expanded bounds to force 2nd expand
            Assert.IsTrue(pointer[64].IsOOB);
            Assert.AreEqual(AbstractValue.UNKNOWN, pointer[64].Value);
            
            // check that value set outside of bounds is still the same as well 
            Assert.IsTrue(pointer[17].IsOOB);
            Assert.AreEqual(0x41414141, pointer[17].Value);
        }

        [Test]
        public void PointerIncSetValueDecRetainsTaints()
        {
            AbstractValue[] buffer = new AbstractValue[16];
            AbstractBuffer pointer = new AbstractBuffer(buffer);
            AbstractValue value = new AbstractValue(0x41);
            value.AddTaint();

            pointer[0] = value;

            pointer = AbstractBuffer.Add(pointer, 1);
            
            pointer[16] = value;
            
            pointer = AbstractBuffer.Sub(pointer, 1);

            Assert.AreEqual(0x41, pointer[0].Value);
        }
	}
}
