// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Text;

namespace bugreport
{
    public sealed class AbstractValue
    {
        public const UInt32 MAX_BUFFER_SIZE = 25600000;
        public const UInt32 UNKNOWN = 0xb4dc0d3d;
        private static readonly AbstractValue zero = new AbstractValue(0);
        private readonly AbstractBuffer pointsTo;
        private readonly UInt32 storage;
        private Boolean tainted;

        public AbstractValue()
        {
            storage = UNKNOWN;
        }

        public AbstractValue(AbstractValue[] willPointTo)
        {
            if (willPointTo.Length == 0)
            {
                throw new ArgumentException("Empty buffer is not allowed", "willPointTo");
            }

            storage = 0xdeadbeef;
            pointsTo = new AbstractBuffer(willPointTo);
        }

        public AbstractValue(AbstractBuffer willPointTo)
        {
            if (willPointTo.Length == 0)
            {
                throw new ArgumentException("Empty buffer is not allowed", "willPointTo");
            }

            storage = 0xdeadbeef;
            pointsTo = new AbstractBuffer(willPointTo);
        }

        public AbstractValue(AbstractValue other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            storage = other.Value;
            tainted = other.IsTainted;
            IsOutOfBounds = other.IsOutOfBounds;

            if (other.PointsTo != null)
            {
                pointsTo = new AbstractBuffer(other.PointsTo);
            }
        }

        public AbstractValue(UInt32 value)
        {
            storage = value;
        }

        public AbstractBuffer PointsTo
        {
            get { return pointsTo; }
        }

        public Boolean IsTainted
        {
            get { return tainted; }

            private set { tainted = value; }
        }

        public Boolean IsOutOfBounds { get; set; }

        public Boolean IsInitialized
        {
            get { return storage != UNKNOWN; }
        }

        public UInt32 Value
        {
            get { return storage; }
        }

        public Boolean IsPointer
        {
            get { return pointsTo != null; }
        }

        public static AbstractValue Zero
        {
            get { return zero; }
        }

        public static AbstractValue[] GetNewBuffer(UInt32 size)
        {
            if (size > MAX_BUFFER_SIZE)
            {
                throw new ArgumentOutOfRangeException(
                    "size", "Size specified larger than maximum allowed: " + MAX_BUFFER_SIZE);
            }

            var buffer = new AbstractValue[size];
            for (UInt32 i = 0; i < size; i++)
            {
                buffer[i] = new AbstractValue();
            }

            return buffer;
        }

        public override Boolean Equals(object obj)
        {
            var other = obj as AbstractValue;

            if (null == other)
            {
                return false;
            }

            return Value == other.Value &&
                   IsOutOfBounds == other.IsOutOfBounds &&
                   IsTainted == other.IsTainted &&
                   PointsTo == other.PointsTo;
        }

        public override Int32 GetHashCode()
        {
            var hashCode = Value.GetHashCode() ^ IsOutOfBounds.GetHashCode() ^
                           IsTainted.GetHashCode();

            if (PointsTo != null)
            {
                hashCode ^= PointsTo.GetHashCode();
            }

            return hashCode;
        }

        public AbstractValue TruncateValueToByte()
        {
            var byteValue = Value & 0xff;
            var truncatedValue = new AbstractValue(byteValue);
            truncatedValue.IsTainted = IsTainted;

            return truncatedValue;
        }

        public AbstractValue AddTaint()
        {
            if (PointsTo != null)
            {
                throw new InvalidOperationException("Cannot AddTaint to a pointer");
            }

            var taintedValue = new AbstractValue(this)
                               {
                                   IsTainted = true
                               };
            return taintedValue;
        }

        public AbstractValue AddTaintIf(Boolean condition)
        {
            if (condition)
            {
                return AddTaint();
            }

            return this;
        }

        public override String ToString()
        {
            var result = String.Empty;

            if (tainted)
            {
                result += "t";
            }

            var valueToPrint = Value;

            if (pointsTo != null)
            {
                var pointer = pointsTo[0];

                var newResult = new StringBuilder(result);

                const Byte MAXIMUM_DISPLAYED_POINTER_DEPTH = 100;
                Int32 count = MAXIMUM_DISPLAYED_POINTER_DEPTH;
                while ((pointer != null) &&
                       (count-- > 0))
                {
                    newResult.Append("*");

                    if (pointer.PointsTo != null)
                    {
                        pointer = pointer.PointsTo[0];
                    }
                    else
                    {
                        valueToPrint = pointer.Value;
                        pointer = null;
                    }
                }

                result = newResult.ToString();
            }

            if (valueToPrint != UNKNOWN)
            {
                result += String.Format("0x{0:x8}", valueToPrint);
            }
            else
            {
                result += "?";
            }

            return result;
        }
    }
}