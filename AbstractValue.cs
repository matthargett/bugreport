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
			//TODO: this doesn't do anything with PointsTo
			AbstractValue tainted = new AbstractValue(this);
			tainted.IsTainted = true;
			return tainted;
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
		
		public AbstractValue DoOperation(OperatorEffect _operatorEffect, AbstractValue rhs)
		{
			AbstractValue lhs = this;
			
			switch(_operatorEffect)
			{
                case OperatorEffect.Assignment:
                {
                    AbstractValue newAbstractValue = new AbstractValue(rhs);
                    if (rhs.IsInitialized && rhs.IsOOB)
                        newAbstractValue.IsOOB = true;
                    
                    return newAbstractValue;
                }
					
				case OperatorEffect.Add:
				{
					if (rhs.IsPointer)
						throw new ArgumentException("rhs pointer not supported.");
					
					if (lhs.IsPointer)
					{
						AbstractBuffer newBuffer = AbstractBuffer.Add(lhs.PointsTo, rhs.Value);
						return new AbstractValue(newBuffer);
					}
					
					UInt32 sum = lhs.Value + rhs.Value;
					AbstractValue result = new AbstractValue(sum);
					result.IsTainted = lhs.IsTainted || rhs.IsTainted;
					return result;
				}
					
				case OperatorEffect.Sub:
				{
					if (rhs.IsPointer)
						throw new ArgumentException("rhs pointer not supported.");
					
					if (lhs.IsPointer)
					{
						AbstractBuffer newBuffer = AbstractBuffer.Sub(lhs.PointsTo, rhs.Value);
						return new AbstractValue(newBuffer);
					}
					UInt32 total = lhs.Value - rhs.Value;
					AbstractValue result = new AbstractValue(total);
					result.IsTainted = lhs.IsTainted || rhs.IsTainted;
					return result;
				}
				
				case OperatorEffect.And:
				{
					if (rhs.IsPointer)
						throw new ArgumentException("rhs pointer not supported.");
					
					if (lhs.IsPointer)
					{
						AbstractBuffer newBuffer = AbstractBuffer.And(lhs.PointsTo, rhs.Value);
						return new AbstractValue(newBuffer);
					}
					
					UInt32 total = lhs.Value & rhs.Value;
					AbstractValue result = new AbstractValue(total);
					result.IsTainted = lhs.IsTainted || rhs.IsTainted;
					return result;
				}
				
				case OperatorEffect.Shr:
				{
					UInt32 total = lhs.Value >> (Byte)rhs.Value;
					AbstractValue result = new AbstractValue(total);
					result.IsTainted = lhs.IsTainted || rhs.IsTainted;
					return result;
				}
					
				case OperatorEffect.Shl:
				{
					UInt32 total = lhs.Value << (Byte)rhs.Value;
					AbstractValue result = new AbstractValue(total);
					result.IsTainted = lhs.IsTainted || rhs.IsTainted;
					return result;
				}
					
				default:
					throw new ArgumentException(String.Format("Unsupported OperatorEffect: {0}", _operatorEffect), "_operatorEffect");
			}
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
