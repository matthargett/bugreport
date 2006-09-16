// Copyright (c) 2006 Luis Miras
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
	public class AbstractBuffer
	{
		private AbstractValue[] storage;
		private UInt32 baseIndex = 0;
        private Int32 allocatedLength = 0;

		public AbstractBuffer(AbstractValue[] _buffer)
		{
			storage = _buffer;
            allocatedLength = storage.Length;
		}
		
		public AbstractBuffer(AbstractBuffer _copyMe)
		{
			this.storage = _copyMe.storage;
			this.BaseIndex = _copyMe.BaseIndex;
            this.allocatedLength = _copyMe.Length;
		}

        private void Extend(UInt32 _newLength)
        {
            // Account for element [0] of the array
            _newLength = _newLength + 1;
            if (_newLength >= this.Length)
            {
                AbstractValue[] _copyTo = new AbstractValue[_newLength];

                Int32 i;
                for (i = 0; i < this.storage.Length; i++)
                {
                    _copyTo[i] = this.storage[i];
                }
                for (; i < _newLength; i++)
                {
                    _copyTo[i] = new AbstractValue(AbstractValue.UNKNOWN);
                    _copyTo[i].IsOOB = true;
                }               
                this.storage = _copyTo;
            }

            return;
        }

        public static AbstractBuffer Add(AbstractBuffer _buffer, UInt32 _addValue)
		{
			AbstractBuffer result = new AbstractBuffer(_buffer);
            result.baseIndex += _addValue;

            return result;
		}
		public static AbstractBuffer And(AbstractBuffer _buffer, UInt32 _andValue)
		{
			AbstractBuffer result = new AbstractBuffer(_buffer);
			
			if ((result.baseIndex & _andValue) < 0)
				throw new ArgumentException(String.Format("Attempting to set a negative baseindex, baseindex: {0:x4}, _andValue {1:x4}", result.baseIndex, _andValue));
			
			result.baseIndex &= _andValue;
			return result;			
		}
		public static AbstractBuffer Sub(AbstractBuffer _buffer, UInt32 _subValue)
		{
			AbstractBuffer result = new AbstractBuffer(_buffer);
			
			if (result.baseIndex < _subValue)
				throw new ArgumentException(String.Format("Attempting to set a negative baseindex, baseindex: {0:x4}, _subValue {1:x4}", result.baseIndex, _subValue));
			
			result.baseIndex -= _subValue;
			return result;			
		}
		public AbstractValue this[Int32 index]
		{
			get 
            { 
                // We check this.storage.Length as well so that we aren't calling Extend() when we dont need to.
                if(((baseIndex + index) >= this.allocatedLength) && ((baseIndex + index) >= this.storage.Length))
                {
                    this.Extend(baseIndex + (uint)index);
                    return this.storage[baseIndex + index];
                }
                else
                    return storage[baseIndex + index]; 
            }
			
			set 
            {
                if ((baseIndex + index) >= this.allocatedLength)
                    value.IsOOB = true;

                if (((baseIndex + index) >= this.allocatedLength) && ((baseIndex + index) >= this.storage.Length))
                {
                    this.Extend(baseIndex + (uint)index);
                    this.storage[baseIndex + index] = value;
                    
                }
                else
                    this.storage[baseIndex + index] = value; 
            }
		}
		
		public UInt32 BaseIndex
		{
			get { return baseIndex; }
			
			private set { baseIndex = value; }
		}
		
		public Int32 Length
		{
			get { return this.allocatedLength; }
		}
	}
	
	
	public class AbstractValue
	{
		AbstractBuffer pointsTo;
		UInt32 storage;
		Boolean tainted;
        Boolean isOOB;

        public const UInt32 UNKNOWN = 0xb4dc0d3d;
		
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
			AbstractValue tainted = new AbstractValue(this.Value);
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
		
		public UInt32 Value
		{
			get { return storage; }
			
			private set { storage = value; }
		}

		public Boolean IsPointer
		{
			get { return pointsTo != null; }
		}
		
		public static AbstractValue DoOperation(AbstractValue lhs, OperatorEffect _operatorEffect, AbstractValue rhs)
		{
			switch(_operatorEffect)
			{
                case OperatorEffect.Assignment:
                {
                    AbstractValue newAbstractValue = new AbstractValue(rhs);
                    if (lhs != null && lhs.IsOOB)
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
				result += "*";
				AbstractValue pointer = pointsTo[0];
				while (pointer != null)
				{
					result += "*";
					if (pointer.PointsTo != null)
					{
						pointer = pointer.PointsTo[0];
					}
					else
					{
						pointer = null;
					}
				}
			}
			
			return result;
		}
	}
}
