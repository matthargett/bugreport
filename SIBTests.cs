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
    public class SIBTests
    {
        private Byte[] code;

        [Test]
        public void EvGvSIBNoIndexToEspFromEax()
        {
            code = new Byte[] {0x89, 0x04, 0x24};
            Assert.IsTrue(ModRM.HasSIB(code));
            Assert.AreEqual(RegisterName.ESP, SIB.GetBaseRegister(code));
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void GetBaseRegisterWhenNoSIBPresent()
        {
            // mov    ebp,esp
            code = new Byte[] {0x89, 0xe5};
            Assert.IsFalse(ModRM.HasSIB(code));
            SIB.GetBaseRegister(code);
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void GetScaledRegisterWhenNoSIBPresent()
        {
            // mov    ebp,esp
            code = new Byte[] {0x89, 0xe5};
            Assert.IsFalse(ModRM.HasSIB(code));
            SIB.GetScaledRegister(code);
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void GetScalerWhenNoSIBPresent()
        {
            // mov    ebp,esp
            code = new Byte[] {0x89, 0xe5};
            Assert.IsFalse(ModRM.HasSIB(code));
            SIB.GetScaler(code);
        }

        [Test]
        public void GvMSIBRegisterIndex()
        {
            code = new Byte[] {0x8d, 0x04, 0x02};
            Assert.AreEqual(RegisterName.EDX, SIB.GetBaseRegister(code));
            Assert.AreEqual(RegisterName.EAX, SIB.GetScaledRegister(code));
            Assert.AreEqual(1, SIB.GetScaler(code));
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void HasSIBWhenNoModRMPresent()
        {
            code = new Byte[] {0x00};
            ModRM.HasSIB(code);
        }
    }
}