// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Text;

namespace bugreport
{
    public class AbstractValue
    {
        public const UInt32 MAX_BUFFER_SIZE = 25600000;
        public const UInt32 UNKNOWN = 0xb4dc0d3d;
        private readonly AbstractBuffer pointsTo;
        private readonly UInt32 storage;
        private Boolean tainted;

        public AbstractValue()
        {
            storage = UNKNOWN;
        }

        public AbstractValue(AbstractValue[] _willPointTo)
        {
            if (_willPointTo.Length == 0)
            {
                throw new ArgumentException("Empty buffer is not allowed", "_willPointTo");
            }

            storage = 0xdeadbeef;
            pointsTo = new AbstractBuffer(_willPointTo);
        }

        public AbstractValue(AbstractBuffer _willPointTo)
        {
            if (_willPointTo.Length == 0)
            {
                throw new ArgumentException("Empty buffer is not allowed", "_willPointTo");
            }

            storage = 0xdeadbeef;
            pointsTo = new AbstractBuffer(_willPointTo);
        }

        public AbstractValue(AbstractValue _copyMe)
        {
            if (_copyMe == null)
            {
                throw new ArgumentNullException("_copyMe");
            }

            storage = _copyMe.Value;
            tainted = _copyMe.IsTainted;
            IsOOB = _copyMe.IsOOB;

            if (_copyMe.PointsTo != null)
            {
                pointsTo = new AbstractBuffer(_copyMe.PointsTo);
            }
        }

        public AbstractValue(UInt32 _value)
        {
            storage = _value;
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

        public Boolean IsOOB { get; set; }

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
                   IsOOB == other.IsOOB &&
                   IsTainted == other.IsTainted &&
                   PointsTo == other.PointsTo;
        }

        public override Int32 GetHashCode()
        {
            Int32 hashCode = Value.GetHashCode() ^ IsOOB.GetHashCode() ^
                             IsTainted.GetHashCode();

            if (PointsTo != null)
            {
                hashCode ^= PointsTo.GetHashCode();
            }

            return hashCode;
        }

        public AbstractValue TruncateValueToByte()
        {
            UInt32 byteValue = Value & 0xff;
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

            var taintedValue = new AbstractValue(this);
            taintedValue.IsTainted = true;
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
            String result = String.Empty;

            if (tainted)
            {
                result += "t";
            }

            UInt32 valueToPrint = Value;

            if (pointsTo != null)
            {
                AbstractValue pointer = pointsTo[0];

                var newResult = new StringBuilder(result);

                const Byte MAXIMUM_DISPLAYED_POINTER_DEPTH = 100;
                Int32 count = MAXIMUM_DISPLAYED_POINTER_DEPTH;
                while ((pointer != null) && (count-- > 0))
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