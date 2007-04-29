// Copyright (c) 2006-2007 Luis Miras, Doug Coker, Todd Nagengast,
// Anthony Lineberry, Dan Moniz, Bryan Siepert, Mike Seery, Cullen Bryan
// Licensed under GPLv3 draft 3
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
