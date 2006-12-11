// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	public class AbstractValue
	{
		AbstractBuffer pointsTo;
		UInt32 storage;
		Boolean tainted;
        Boolean isOOB;

        public const UInt32 UNKNOWN = 0xb4dc0d3d;
		
        public AbstractValue() {
        	storage = UNKNOWN;        	
        }               
        
		public AbstractValue(AbstractValue[] _willPointTo)
		{
			if (_willPointTo.Length == 0)
				throw new ArgumentException("_willPointTo", "Empty buffer is not allowed");
			storage = 0xdeadbeef;
			pointsTo = new AbstractBuffer(_willPointTo);
		}

		public AbstractValue(AbstractBuffer _willPointTo)
		{
			if (_willPointTo.Length == 0)
				throw new ArgumentException("_willPointTo", "Empty buffer is not allowed");
			storage = 0xdeadbeef;
			pointsTo = new AbstractBuffer(_willPointTo);
		}

		public AbstractValue(AbstractValue _copyMe)
		{
			this.Value = _copyMe.Value;
			this.IsTainted = _copyMe.IsTainted;
            this.IsOOB = _copyMe.IsOOB;
			this.PointsTo = _copyMe.PointsTo;
		}

		public AbstractValue(UInt32 _value)
		{
			storage = _value;
		}
		
		public override bool Equals(object obj)
		{
			AbstractValue other = (AbstractValue)obj;

			return this.Value == other.Value &&
				this.IsOOB == other.IsOOB &&
				this.IsTainted == other.IsTainted &&
				this.PointsTo == other.PointsTo;			
		}
		public static AbstractValue[] GetNewBuffer(uint size) {
			AbstractValue[] buffer = new AbstractValue[size];
			for (uint i = 0; i < size; i++) 
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
				throw new ArgumentException("AddTaint", "Cannot AddTaint to a pointer");
			}
			
			AbstractValue tainted = new AbstractValue(this);
			tainted.IsTainted = true;
			return tainted;
		}
		
		public AbstractValue AddTaintIf(Boolean condition)
		{
			if (condition)
				return AddTaint();
			
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
		
		public override string ToString()
		{
			String result = String.Empty;
			
			if (tainted)
				result += "t";
			
			if (pointsTo == null)
			{
				result += String.Format("0x{0:x8}", storage);
			}
			else
			{
				AbstractValue pointer = pointsTo[0];
				while (pointer != null)
				{
					result += "*";
					
					if (pointer.PointsTo != null)
						pointer = pointer.PointsTo[0];
					else
						pointer = null;
				}
			}
			
			return result;
		}
	}
}
