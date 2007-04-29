// Copyright (c) 2006-2007 Luis Miras, Doug Coker, Todd Nagengast,
// Anthony Lineberry, Dan Moniz, Bryan Siepert, Mike Seery, Cullen Bryan
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
[TestFixture]
public class ReportItemTest
{
    [Test]
    public void Equality()
    {
        ReportItem same = new ReportItem(123, false);
        ReportItem same2 = new ReportItem(123, false);
        ReportItem different = new ReportItem(456, false);

        Assert.IsTrue(same.Equals(same2));
        Assert.IsFalse(same.Equals(different));

        Assert.IsTrue(same == same2);
        Assert.IsTrue(same != different);

        Assert.AreEqual(same.GetHashCode(),same2.GetHashCode());
        Assert.AreNotEqual(same.GetHashCode(),different.GetHashCode());
    }
}
}
