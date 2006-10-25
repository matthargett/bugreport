// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using NUnit.Framework;
using System;

namespace bugreport
{
	[TestFixture]
	public class AbstractValueTests
	{	
		AbstractValue pointer;
		AbstractBuffer buffer;
		
		[Test]
		public void DefaultIsNotInitialized() 
		{
			AbstractValue uninit = new AbstractValue();			
			Assert.IsFalse(uninit.IsInitialized);
			Assert.AreEqual(AbstractValue.UNKNOWN, uninit.Value);
		}
		
		[Test]
		public void InitializedIsInitialized() 
		{
			AbstractValue uninit = new AbstractValue(0xabcdef);
			Assert.IsTrue(uninit.IsInitialized);
			Assert.AreEqual(0xabcdef, uninit.Value);
		}
		
		[Test]
		public void AssignmentAtByteZero()
		{
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(16);
			pointer = new AbstractValue(buffer);
			pointer.PointsTo[0] = new AbstractValue(0x31337);
			Assert.AreEqual(0x31337, pointer.PointsTo[0].Value);
		}

		[Test]
		public void AssignmentAtEnd()
		{
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(16);
			pointer = new AbstractValue(buffer);
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
			one = one.DoOperation(OperatorEffect.Assignment, two);
			
			Assert.AreEqual(2, one.Value);
			Assert.IsTrue(one.IsTainted);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DoOperationForUnknown()
		{
			AbstractValue one = new AbstractValue(1);
			AbstractValue two = new AbstractValue(2).AddTaint();
			one.DoOperation(OperatorEffect.Unknown, two);
		}

		[Test]
		public void DoOperationForAdd()
		{
			AbstractValue one = new AbstractValue(1);
			AbstractValue two = new AbstractValue(2).AddTaint();
			AbstractValue three = one.DoOperation(OperatorEffect.Add, two);
			
			Assert.AreEqual(3, three.Value);
			Assert.IsTrue(three.IsTainted);
		}
		
		[Test]
		public void DoOperationForSub()
		{
			AbstractValue one = new AbstractValue(1);
			AbstractValue three = new AbstractValue(3).AddTaint();
			AbstractValue two = three.DoOperation(OperatorEffect.Sub, one);
			
			Assert.AreEqual(2, two.Value);
			Assert.IsTrue(two.IsTainted);
		}

		[Test]
		public void DoOperationForAnd()
		{
			AbstractValue threeFifty = new AbstractValue(0x350).AddTaint();
			AbstractValue ff = new AbstractValue(0xff);
			AbstractValue fifty = threeFifty.DoOperation(OperatorEffect.And, ff);
			
			Assert.AreEqual(0x50, fifty.Value);
			Assert.IsTrue(fifty.IsTainted);
		}

		[Test]
		public void DoOperationForShr()
		{
			AbstractValue eight = new AbstractValue(0x8).AddTaint();
			AbstractValue threeBits = new AbstractValue(0x3);
			AbstractValue one = eight.DoOperation(OperatorEffect.Shr, threeBits);
			
			Assert.AreEqual(0x1, one.Value);
			Assert.IsTrue(one.IsTainted);
		}

		[Test]
		public void DoOperationForShl()
		{
			AbstractValue one = new AbstractValue(0x1).AddTaint();
			AbstractValue threeBits = new AbstractValue(0x3);
			AbstractValue eight = one.DoOperation(OperatorEffect.Shl, threeBits);
			
			Assert.AreEqual(0x8, eight.Value);
			Assert.IsTrue(eight.IsTainted);
		}

		[Test]
		public void PointerAdd()
		{
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(0x10);	
			AbstractValue one= new AbstractValue(0x1);
			buffer[4] = one;
			pointer = new AbstractValue(buffer);
			
			AbstractValue pointerPlus4 = pointer.DoOperation(OperatorEffect.Add, new AbstractValue(0x4));
			Assert.AreEqual(one, pointerPlus4.PointsTo[0]);
		}
		
		[Test]
		public void DoOperationForPointerAnd()
		{
			AbstractValue[] buffer = AbstractValue.GetNewBuffer(0x10);
			AbstractValue one= new AbstractValue(0x1);
			buffer[4] = one;
			
			pointer = new AbstractValue(buffer);
			
			AbstractValue pointerPlus4 = pointer.DoOperation(OperatorEffect.Add, new AbstractValue(0x4));
			Assert.AreEqual(one, pointerPlus4.PointsTo[0]);
			
			AbstractValue pointerAnd = pointerPlus4.DoOperation(OperatorEffect.And, new AbstractValue(0xfffffff0));
			Assert.AreEqual(one, pointerAnd.PointsTo[4]);		
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void EmptyBuffer()
		{
			new AbstractValue(new AbstractBuffer(new AbstractValue[] {}));
		}
		
		[Test]
		public void NoPointer()
		{
			AbstractValue value = new AbstractValue(2).AddTaint();
			
			Assert.IsNull(value.PointsTo);
			StringAssert.Contains("0x00000002", value.ToString());
			StringAssert.Contains("t", value.ToString());
			
		}
		
		[Test]
		public void Pointer()
		{	
			buffer = new AbstractBuffer(new AbstractValue[] {new AbstractValue(2)});
			pointer = new AbstractValue(buffer);
			Assert.AreEqual(2, pointer.PointsTo[0].Value);
			StringAssert.StartsWith("*", pointer.ToString());
		}

		[Test]
		public void PointerPointer()
		{	
			buffer = new AbstractBuffer(new AbstractValue[] {new AbstractValue(2)});
			pointer = new AbstractValue(buffer);
			AbstractValue pointerPointer = new AbstractValue(new AbstractValue[] {pointer});
			Assert.AreEqual(2, pointerPointer.PointsTo[0].PointsTo[0].Value);
			StringAssert.StartsWith("**", pointerPointer.ToString());
		}

		[Test]
		public void AddPointerPointer()
		{	
			buffer = new AbstractBuffer(new AbstractValue[] {new AbstractValue(1)});
			pointer = new AbstractValue(buffer);
			AbstractBuffer buffer2 = new AbstractBuffer(new AbstractValue[] {new AbstractValue(2)});
			AbstractValue pointer2 = new AbstractValue(buffer2);
			AbstractValue pointerPointer = new AbstractValue(new AbstractValue[] {pointer, pointer2});
			AbstractValue addedPointerPointer = pointerPointer.DoOperation(OperatorEffect.Add, new AbstractValue(1));
			Assert.AreEqual(2, addedPointerPointer.PointsTo[0].PointsTo[0].Value);
			StringAssert.StartsWith("**", pointerPointer.ToString());
		}

		[Test]
		public void PointerPointerPointer()
		{	
			buffer = new AbstractBuffer(new AbstractValue[] {new AbstractValue(2)});
			pointer = new AbstractValue(buffer);
			AbstractValue pointerPointer = new AbstractValue(new AbstractValue[] {pointer});
			AbstractValue pointerPointerPointer = new AbstractValue(new AbstractValue[] {pointerPointer});
			Assert.AreEqual(2, pointerPointerPointer.PointsTo[0].PointsTo[0].PointsTo[0].Value);

			StringAssert.StartsWith("***", pointerPointerPointer.ToString());
		}

    }
}
