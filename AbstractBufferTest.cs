// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
    [TestFixture]
    public class AbstractBufferTest
    {
        [Test]
        public void Copy()
        {
            AbstractValue[] values = AbstractValue.GetNewBuffer(4);
            var buffer = new AbstractBuffer(values);
            var newBuffer = new AbstractBuffer(buffer);
            Assert.AreNotSame(newBuffer, buffer);

            for (Int32 index = 0; index < newBuffer.Length; index++)
            {
                Assert.AreSame(newBuffer[index], buffer[index]);
            }
        }

        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void InvalidPointerAnd()
        {
            var one = new AbstractValue(0x1);
            var buffer = new AbstractBuffer(new[] {one});
            AbstractBuffer modifiedBuffer = buffer.DoOperation(OperatorEffect.Sub, new AbstractValue(3));
            modifiedBuffer.DoOperation(OperatorEffect.And, new AbstractValue(0xf));
        }

        [Test]
        public void OverflowDoesntLoseIncrement()
        {
            AbstractValue[] buffer = AbstractValue.GetNewBuffer(16);
            var pointer = new AbstractBuffer(buffer);
            var value = new AbstractValue(0x41);
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
        public void OverflowZeroSizeBuffer()
        {
            var f = new AbstractBuffer(new AbstractValue[] {});
            Assert.IsFalse(f[0].IsInitialized);
        }

        [Test]
        public void PointerAdd()
        {
            var one = new AbstractValue(0x1);
            var two = new AbstractValue(0x2);
            var three = new AbstractValue(0x3);
            var four = new AbstractValue(0x4);
            AbstractValue[] values = AbstractValue.GetNewBuffer(4);

            values[0] = one;
            values[1] = two;
            values[2] = three;
            values[3] = four;

            var buffer = new AbstractBuffer(values);
            Assert.AreEqual(one, buffer[0]);
            AbstractBuffer modifiedBuffer = buffer.DoOperation(OperatorEffect.Add, new AbstractValue(2));
            Assert.AreEqual(three, modifiedBuffer[0]);
        }

        [Test]
        public void PointerAnd()
        {
            var one = new AbstractValue(0x1);
            var two = new AbstractValue(0x2);
            var three = new AbstractValue(0x3);
            var four = new AbstractValue(0x4);
            AbstractValue[] avBuffer = AbstractValue.GetNewBuffer(4);

            avBuffer[0] = one;
            avBuffer[1] = two;
            avBuffer[2] = three;
            avBuffer[3] = four;

            var buffer = new AbstractBuffer(avBuffer);
            Assert.AreEqual(one, buffer[0]);
            AbstractBuffer modifiedBuffer = buffer.DoOperation(OperatorEffect.Add, new AbstractValue(3));
            Assert.AreEqual(four, modifiedBuffer[0]);
            AbstractBuffer andedBuffer = modifiedBuffer.DoOperation(OperatorEffect.And, new AbstractValue(0xfffffff0));
            Assert.AreEqual(one, andedBuffer[0]);
        }

        [Test]
        public void PointerAssignment()
        {
            AbstractValue[] values = AbstractValue.GetNewBuffer(4);
            var buffer = new AbstractBuffer(values);
            AbstractBuffer assignedBuffer = buffer.DoOperation(OperatorEffect.Assignment, null);
            Assert.AreNotSame(buffer, assignedBuffer);
        }

        [Test]
        public void PointerOverflowByOne()
        {
            AbstractValue[] buffer = AbstractValue.GetNewBuffer(16);
            var pointer = new AbstractBuffer(buffer);

            AbstractValue value = pointer[16];
            Assert.IsTrue(value.IsOOB);
            Assert.IsFalse(value.IsInitialized);
            Assert.AreEqual(AbstractValue.UNKNOWN, value.Value);
        }

        [Test]
        public void PointerOverflowStillRetainsOldValues()
        {
            var test1 = new AbstractValue(0x41);
            var test2 = new AbstractValue(0x42);
            AbstractValue[] buffer = AbstractValue.GetNewBuffer(2);

            buffer[0] = test1;
            buffer[1] = test2;

            var pointer = new AbstractBuffer(buffer);

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
            var pointer = new AbstractBuffer(buffer);

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
        public void PointerSub()
        {
            var one = new AbstractValue(0x1);
            var two = new AbstractValue(0x2);
            var three = new AbstractValue(0x3);
            var four = new AbstractValue(0x4);
            AbstractValue[] values = AbstractValue.GetNewBuffer(4);

            values[0] = one;
            values[1] = two;
            values[2] = three;
            values[3] = four;

            var buffer = new AbstractBuffer(values);
            Assert.AreEqual(one, buffer[0]);
            AbstractBuffer modifiedBuffer = buffer.DoOperation(OperatorEffect.Add, new AbstractValue(2));
            Assert.AreEqual(three, modifiedBuffer[0]);

            AbstractBuffer subbedBuffer = modifiedBuffer.DoOperation(OperatorEffect.Sub, new AbstractValue(2));
            Assert.AreEqual(one, subbedBuffer[0]);
        }

        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void PointerSubUnderflow()
        {
            new AbstractBuffer(new AbstractValue[] {}).DoOperation(OperatorEffect.Sub, new AbstractValue(1));
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void PointerUnknownOperation()
        {
            new AbstractBuffer(new AbstractValue[] {}).DoOperation(OperatorEffect.Unknown, null);
        }
    }
}