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
		public void PointerPointerPointer()
		{	
			buffer = new AbstractBuffer(new AbstractValue[] {new AbstractValue(2)});
			pointer = new AbstractValue(buffer);
			AbstractValue pointerPointer = new AbstractValue(new AbstractValue[] {pointer});
			AbstractValue pointerPointerPointer = new AbstractValue(new AbstractValue[] {pointerPointer});
			Assert.AreEqual(2, pointerPointerPointer.PointsTo[0].PointsTo[0].PointsTo[0].Value);

			StringAssert.StartsWith("***", pointerPointerPointer.ToString());
		}

		[Test]
		public void CheckOOBAfterCopy()
		{
			AbstractValue src = new AbstractValue(0x31337);
			src.IsOOB = false;
			AbstractValue dest = new AbstractValue(0x1);
			dest.IsOOB = true;
			dest = new AbstractValue(src);
			
			Assert.IsFalse(dest.IsOOB);
		}
		
		[Test]
		public void PreserveIsOOBAfterCopy()
		{
			AbstractValue src = new AbstractValue(0x31337);
			AbstractValue dest = new AbstractValue(0x1);
			src.IsOOB = true;
			dest = new AbstractValue(src);
			
			Assert.IsTrue(dest.IsOOB);
		}
    }
}
