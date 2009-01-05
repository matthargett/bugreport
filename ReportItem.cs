// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;

namespace bugreport
{
    public struct ReportItem
    {
        public UInt32 InstructionPointer;
        public Boolean IsTainted;

        public ReportItem(UInt32 instructionPointer, Boolean isTainted)
        {
            this.InstructionPointer = instructionPointer;
            this.IsTainted = isTainted;
        }

        public override Boolean Equals(object obj)
        {
            ReportItem report = (ReportItem)obj;
            return this.InstructionPointer == report.InstructionPointer &&
                   this.IsTainted == report.IsTainted;
        }

        public override Int32 GetHashCode()
        {
            return this.InstructionPointer.GetHashCode() ^ this.IsTainted.GetHashCode();
        }

        public static Boolean operator== (ReportItem a, ReportItem b)
        {
            return a.Equals(b);
        }

        public static Boolean operator!= (ReportItem a, ReportItem b)
        {
            return !a.Equals(b);
        }
    }
}
