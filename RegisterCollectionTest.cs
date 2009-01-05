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
    public class RegisterCollectionTest
    {
        RegisterCollection registers;

        [SetUp]
        public void SetUp()
        {
            registers = new RegisterCollection();
        }

        [Test]
        public void Copy()
        {
            registers[RegisterName.ESP] = new AbstractValue(new AbstractBuffer(AbstractValue.GetNewBuffer(10)));
            RegisterCollection newRegisters = new RegisterCollection(registers);
            for (UInt32 i = 0; i < 7; i++)
            {
                RegisterName register = (RegisterName)i;
                Assert.AreNotSame(newRegisters[register], registers[register]);
            }

            Assert.AreNotSame(newRegisters[RegisterName.ESP].PointsTo, registers[RegisterName.ESP].PointsTo);
        }

        [Test]
        public void DefaultRegistersContainUninitializedValues()
        {
            for (UInt32 i = 0; i < 7; i++)
            {
                RegisterName register = (RegisterName)i;
                Assert.IsFalse(registers[register].IsInitialized);
            }
        }

        [Test]
        public void ToStringOutput()
        {
            StringAssert.StartsWith("EAX=?", registers.ToString());
        }

        [Test]
        public void Equals()
        {
            Assert.IsFalse(registers.Equals(null));
        }
    }
}
