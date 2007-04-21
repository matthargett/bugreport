// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	[TestFixture]
	public class AbstractBufferTests
	{
		[Test]
		public void PointerAssignment()
		{
			AbstractValue[] values = AbstractValue.GetNewBuffer(4);	
			AbstractBuffer buffer = new AbstractBuffer(values);
			AbstractBuffer assignedBuffer = buffer.DoOperation(OperatorEffect.Assignment, null);
			Assert.AreNotSame(buffer, assignedBuffer);
			
		}

		[Test]
		public void Copy()
		{
			AbstractValue[] values = AbstractValue.GetNewBuffer(4);	
			AbstractBuffer buffer = new AbstractBuffer(values);
			AbstractBuffer newBuffer = new AbstractBuffer(buffer);
			Assert.AreNotSame(newBuffer, buffer);
			
			for (Int32 index =0; index < newBuffer.Length; index++)
			{
				Assert.AreSame(newBuffer[index], buffer[index]);
			}
		}
		
		[Test]
		public void PointerAdd()
		{
			AbstractValue one = new AbstractValue(0x1);
			AbstractValue two = new AbstractValue(0x2);
			AbstractValue three = new AbstractValue(0x3);
			AbstractValue four = new AbstractValue(0x4);
			AbstractValue[] avBuffer = AbstractValue.GetNewBuffer(4);
			
			avBuffer[0] = one;
			avBuffer[1] = two;
			avBuffer[2] = three;
			avBuffer[3] = four;
			
			AbstractBuffer buffer = new AbstractBuffer(avBuffer);
			Assert.AreEqual(one, buffer[0]);
			AbstractBuffer modifiedBuffer = buffer.DoOperation(OperatorEffect.Add, new AbstractValue(2));
			Assert.AreEqual(three, modifiedBuffer[0]);
			
		}

		[Test]
		public void PointerSub()
		{
			AbstractValue one = new AbstractValue(0x1);
			AbstractValue two = new AbstractValue(0x2);
			AbstractValue three = new AbstractValue(0x3);
			AbstractValue four = new AbstractValue(0x4);
			AbstractValue[] avBuffer = AbstractValue.GetNewBuffer(4);
			
			avBuffer[0] = one;
			avBuffer[1] = two;
			avBuffer[2] = three;
			avBuffer[3] = four;
			
			AbstractBuffer buffer = new AbstractBuffer(avBuffer);
			Assert.AreEqual(one, buffer[0]);
			AbstractBuffer modifiedBuffer = buffer.DoOperation(OperatorEffect.Add, new AbstractValue(2));
			Assert.AreEqual(three, modifiedBuffer[0]);

			AbstractBuffer subbedBuffer = modifiedBuffer.DoOperation(OperatorEffect.Sub, new AbstractValue(2));
			Assert.AreEqual(one, subbedBuffer[0]);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void PointerSubUnderflow()
		{
			new AbstractBuffer(new AbstractValue[] {}).DoOperation(OperatorEffect.Sub, new AbstractValue(1));
		}
		
		[Test]
		public void PointerAnd()
		{
			AbstractValue one = new AbstractValue(0x1);
			AbstractValue two = new AbstractValue(0x2);
			AbstractValue three = new AbstractValue(0x3);
			AbstractValue four = new AbstractValue(0x4);
			AbstractValue[] avBuffer = AbstractValue.GetNewBuffer(4);
			
			avBuffer[0] = one;
			avBuffer[1] = two;
			avBuffer[2] = three;
			avBuffer[3] = four;
			
			AbstractBuffer buffer = new AbstractBuffer(avBuffer);
			Assert.AreEqual(one, buffer[0]);
			AbstractBuffer modifiedBuffer = buffer.DoOperation(OperatorEffect.Add, new AbstractValue(3));
			Assert.AreEqual(four, modifiedBuffer[0]);
			AbstractBuffer andedBuffer = modifiedBuffer.DoOperation(OperatorEffect.And, new AbstractValue(0xfffffff0));
			Assert.AreEqual(one, andedBuffer[0]);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void InvalidPointerAnd()
		{
			AbstractValue one = new AbstractValue(0x1);
			AbstractBuffer buffer = new AbstractBuffer(new AbstractValue[] {one});
			AbstractBuffer modifiedBuffer = buffer.DoOperation(OperatorEffect.Sub, new AbstractValue(3));
			modifiedBuffer.DoOperation(OperatorEffect.And, new AbstractValue(0xf));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void PointerUnknownOperation()
		{
			new AbstractBuffer(new AbstractValue[] {}).DoOperation(OperatorEffect.Unknown, null);
		}
		

		[Test]
		public void PointerOverflowByOne()
		{
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(16);
			AbstractBuffer pointer = new AbstractBuffer(buffer);

			AbstractValue value = pointer[16];
			Assert.IsTrue(value.IsOOB);
			Assert.IsFalse(value.IsInitialized);
			Assert.AreEqual(AbstractValue.UNKNOWN, value.Value);
		}

		[Test]
		public void PointerOverflowStillRetainsOldValues()
		{
			AbstractValue test1 = new AbstractValue(0x41);
			AbstractValue test2 = new AbstractValue(0x42);
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(2);

			buffer[0] = test1;
			buffer[1] = test2;

			AbstractBuffer pointer = new AbstractBuffer(buffer);

			// Accessing pointer[2] will cause the AbstractBuffer to extend..
			Assert.IsTrue(pointer[2].IsOOB, " value is not out of bounds");
			Assert.IsFalse(pointer[2].IsInitialized);
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
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(16);
			AbstractBuffer pointer = new AbstractBuffer(buffer);

			//Access beyond buffer bounds forcing buffer to expand
			Assert.IsTrue(pointer[17].IsOOB);
			Assert.IsFalse(pointer[17].IsInitialized);
			Assert.AreEqual(AbstractValue.UNKNOWN, pointer[17].Value);

			pointer[17] = new AbstractValue(0x41414141);

			// Access beyond previously expanded bounds to force 2nd expand
			Assert.IsTrue(pointer[64].IsOOB);
			Assert.IsFalse(pointer[64].IsInitialized);
			Assert.AreEqual(AbstractValue.UNKNOWN, pointer[64].Value);
			
			// check that value set outside of bounds is still the same as well
			Assert.IsTrue(pointer[17].IsOOB);
			Assert.AreEqual(0x41414141, pointer[17].Value);
		}

		[Test]
		public void OverflowDoesntLoseIncrement()
		{
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(16);
			AbstractBuffer pointer = new AbstractBuffer(buffer);
			AbstractValue value = new AbstractValue(0x41);
			value = value.AddTaint();

			pointer[0] = value;
			
			pointer = pointer.DoOperation(OperatorEffect.Add, new AbstractValue(1));
			
			pointer[16] = value;
			
			pointer = pointer.DoOperation(OperatorEffect.Sub, new AbstractValue(1));

			Assert.AreEqual(0x41, pointer[0].Value);
			Assert.IsTrue(value.IsTainted);
			Assert.AreEqual(0x41, pointer[17].Value);
			Assert.IsTrue(pointer[17].IsTainted);
		}
		
		[Test]
		public void OverflowZeroSizeBuffer() {
			AbstractBuffer f = new AbstractBuffer(new AbstractValue[] {});
			Assert.IsFalse(f[0].IsInitialized);
		}
	}
}
