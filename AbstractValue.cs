// Copyright (c) 2006-2007 Luis Miras, Doug Coker, Todd Nagengast,
// Anthony Lineberry, Dan Moniz, Bryan Siepert, Mike Seery, Cullen Bryan
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;
using System.Text;

namespace bugreport
{
public class AbstractValue
{
    AbstractBuffer pointsTo;
    UInt32 storage;
    Boolean tainted;
    Boolean isOOB;

    public const UInt32 UNKNOWN = 0xb4dc0d3d;
    public const UInt32 MAX_BUFFER_SIZE = 25600000;

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
        isOOB = _copyMe.IsOOB;

        if (_copyMe.PointsTo != null)
        {
            pointsTo = new AbstractBuffer(_copyMe.PointsTo);
        }
    }

    public AbstractValue(UInt32 _value)
    {
        storage = _value;
    }

    public override Boolean Equals(object obj)
    {
        AbstractValue other = obj as AbstractValue;

        if (null == other)
        {
            return false;
        }

        return this.Value == other.Value &&
               this.IsOOB == other.IsOOB &&
               this.IsTainted == other.IsTainted &&
               this.PointsTo == other.PointsTo;
    }

    public override Int32 GetHashCode()
    {
        Int32 hashCode = this.Value.GetHashCode() ^ this.IsOOB.GetHashCode() ^
                         this.IsTainted.GetHashCode();

        if (this.PointsTo != null)
        {
            hashCode ^= this.PointsTo.GetHashCode();
        }

        return hashCode;
    }

    public static AbstractValue[] GetNewBuffer(UInt32 size)
    {
        if (size > MAX_BUFFER_SIZE)
        {
            throw new ArgumentOutOfRangeException("size", "Size specified larger than maximum allowed: " + MAX_BUFFER_SIZE);
        }
        
        AbstractValue[] buffer = new AbstractValue[size];
        for (UInt32 i = 0; i < size; i++)
        {
            buffer[i] = new AbstractValue();
        }
        
        return buffer;
    }

    public AbstractValue TruncateValueToByte()
    {
        UInt32 byteValue = this.Value & 0xff;
        AbstractValue truncatedValue = new AbstractValue(byteValue);
        truncatedValue.IsTainted = this.IsTainted;

        return truncatedValue;
    }

    public AbstractValue AddTaint()
    {
        if (this.PointsTo != null)
        {
            throw new InvalidOperationException("Cannot AddTaint to a pointer");
        }

        AbstractValue taintedValue = new AbstractValue(this);
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

    public AbstractBuffer PointsTo
    {
        get { return pointsTo; }

        private set { pointsTo = value; }
    }

    public Boolean IsTainted
    {
        get { return tainted; }

        private set { tainted = value; }
    }

    public Boolean IsOOB
    {
        get { return isOOB; }
        set { isOOB = value; }
    }

    public Boolean IsInitialized
    {
        get { return storage != UNKNOWN; }
    }

    public UInt32 Value
    {
        get { return storage; }
        private set { storage = value; }
    }

    public Boolean IsPointer
    {
        get { return pointsTo != null; }
    }

    public override String ToString()
    {
        String result = String.Empty;

        if (tainted)
            result += "t";

        UInt32 valueToPrint = this.Value;

        if (pointsTo != null)
        {
            AbstractValue pointer = pointsTo[0];

            StringBuilder newResult = new StringBuilder(result);

            const Byte maximumPointerDepth = 100;
            Int32 count = maximumPointerDepth;
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
