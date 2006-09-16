// Copyright (c) 2006 Luis Miras
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using NUnit.Framework;
using System;

namespace bugreport
{
	[TestFixture]
	public class AbstractValueTests
	{
		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void PointerOverflowByOne()
		{
			AbstractValue[] buffer = new AbstractValue[16];
			AbstractValue pointer = new AbstractValue(buffer);
			pointer.PointsTo[16] = null;
		}

		[Test]
		public void AssignmentAtByteZero()
		{
			AbstractValue[] buffer = new AbstractValue[16];
			AbstractValue pointer = new AbstractValue(buffer);
			pointer.PointsTo[0] = new AbstractValue(0x31337);
			Assert.AreEqual(0x31337, pointer.PointsTo[0].Value);
		}

		[Test]
		public void AssignmentAtEnd()
		{
			AbstractValue[] buffer = new AbstractValue[16];
			AbstractValue pointer = new AbstractValue(buffer);
			pointer.PointsTo[15] = new AbstractValue(0x31337);
			Assert.AreEqual(0x31337, pointer.PointsTo[15].Value);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ZeroSizeBuffer()
		{
			new AbstractValue(new AbstractValue[] {});
		}
		
		[Test]
		public void TruncateValue()
		{
			AbstractValue dwordValue = new AbstractValue(0xdeadbeef);
			AbstractValue byteValue = dwordValue.TruncateValueToByte();
			Assert.AreEqual(0xef, byteValue.Value);
		}
		
		[Test]
		public void AddTaint()
		{
			AbstractValue clean = new AbstractValue(0x31337);
			Assert.IsFalse(clean.IsTainted);
			AbstractValue tainted = clean.AddTaint();
			Assert.IsTrue(tainted.IsTainted);
		}
		
		[Test]
		public void DoOperationForAssignment()
		{
			AbstractValue one = new AbstractValue(1);
			AbstractValue two = new AbstractValue(2).AddTaint();
			one = AbstractValue.DoOperation(one, OperatorEffect.Assignment, two);
			
			Assert.AreEqual(2, one.Value);
			Assert.IsTrue(one.IsTainted);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DoOperationForUnknown()
		{
			AbstractValue one = new AbstractValue(1);
			AbstractValue two = new AbstractValue(2).AddTaint();
			AbstractValue.DoOperation(one, OperatorEffect.Unknown, two);
		}

		[Test]
		public void DoOperationForAdd()
		{
			AbstractValue one = new AbstractValue(1);
			AbstractValue two = new AbstractValue(2).AddTaint();
			AbstractValue three = AbstractValue.DoOperation(one, OperatorEffect.Add, two);
			
			Assert.AreEqual(3, three.Value);
			Assert.IsTrue(three.IsTainted);
		}
		
		[Test]
		public void DoOperationForSub()
		{
			AbstractValue one = new AbstractValue(1);
			AbstractValue three = new AbstractValue(3).AddTaint();
			AbstractValue two = AbstractValue.DoOperation(three, OperatorEffect.Sub, one);
			
			Assert.AreEqual(2, two.Value);
			Assert.IsTrue(two.IsTainted);
		}

		[Test]
		public void DoOperationForAnd()
		{
			AbstractValue threeFifty = new AbstractValue(0x350).AddTaint();
			AbstractValue ff = new AbstractValue(0xff);
			AbstractValue fifty = AbstractValue.DoOperation(threeFifty, OperatorEffect.And, ff);
			
			Assert.AreEqual(0x50, fifty.Value);
			Assert.IsTrue(fifty.IsTainted);
		}

		[Test]
		public void DoOperationForShr()
		{
			AbstractValue eight = new AbstractValue(0x8).AddTaint();
			AbstractValue threeBits = new AbstractValue(0x3);
			AbstractValue one = AbstractValue.DoOperation(eight, OperatorEffect.Shr, threeBits);
			
			Assert.AreEqual(0x1, one.Value);
			Assert.IsTrue(one.IsTainted);
		}

		[Test]
		public void DoOperationForShl()
		{
			AbstractValue one = new AbstractValue(0x1).AddTaint();
			AbstractValue threeBits = new AbstractValue(0x3);
			AbstractValue eight = AbstractValue.DoOperation(one, OperatorEffect.Shl, threeBits);
			
			Assert.AreEqual(0x8, eight.Value);
			Assert.IsTrue(eight.IsTainted);
		}

		[Test]
		public void PointerArith()
		{
			AbstractValue[] buffer = new AbstractValue[0x10];
			AbstractValue one= new AbstractValue(0x1);
			buffer[4] = one;
			AbstractValue pointer = new AbstractValue(buffer);
			
			AbstractValue pointerPlus4 = AbstractValue.DoOperation(pointer, OperatorEffect.Add, new AbstractValue(0x4));
			Assert.AreEqual(one, pointerPlus4.PointsTo[0]);
		}
		
		[Test]
		public void DoOperationForPointerAnd()
		{
			AbstractValue[] buffer = new AbstractValue[0x10];
			AbstractValue one= new AbstractValue(0x1);
			buffer[4] = one;
			
			AbstractValue pointer = new AbstractValue(buffer);
			
			AbstractValue pointerPlus4 = AbstractValue.DoOperation(pointer, OperatorEffect.Add, new AbstractValue(0x4));
			Assert.AreEqual(one, pointerPlus4.PointsTo[0]);
			
			AbstractValue pointerAnd = AbstractValue.DoOperation(pointerPlus4, OperatorEffect.And, new AbstractValue(0xfffffff0));
			Assert.AreEqual(one, pointerAnd.PointsTo[4]);		
		}

        [Test]
        public void ReadBeyondEndGetsUnkownValue()
        {
            AbstractValue[] buffer = new AbstractValue[16];
            AbstractValue pointer = new AbstractValue(buffer);

            Assert.IsNotNull(pointer);
            Assert.IsNotNull(pointer.PointsTo);
            AbstractBuffer newBuffer = pointer.PointsTo.Extend(20);
            pointer = new AbstractValue(newBuffer);
            
            Assert.AreEqual(AbstractValue.UNKNOWN, pointer.PointsTo[16].Value);
            Assert.IsTrue(pointer.PointsTo[16].IsOOB);
        }
    }
}
