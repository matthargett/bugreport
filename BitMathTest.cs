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
    public class BitMathTest
    {
        [Test]
        public void AllZeroes()
        {
            UInt32 result = BitMath.BytesToDword(new Byte[] {0, 0, 0, 0}, 0);
            Assert.AreEqual(0, result);
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void EmptyBytes()
        {
            BitMath.BytesToDword(new Byte[] {}, 0);
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void NotEnoughBytesToDwordAtNonZeroIndex()
        {
            BitMath.BytesToDword(new Byte[] {0, 1, 2, 3}, 1);
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void NotEnoughBytesToDwordAtZeroIndex()
        {
            BitMath.BytesToDword(new Byte[] {0}, 0);
        }

        [Test]
        public void OneTwoThreeFour()
        {
            UInt32 result = BitMath.BytesToDword(new Byte[] {1, 2, 3, 4}, 0);
            Assert.AreEqual(0x04030201, result);
        }
    }
}