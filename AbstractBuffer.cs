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

        private bool IsIndexPastBounds(Int32 index)
        {
            return (((baseIndex + index) >= this.allocatedLength) && ((baseIndex + index) >= this.storage.Length));
        }

        public AbstractValue this[Int32 index]
        {
            get
            {
                // We check this.storage.Length as well so that we aren't calling Extend() when we dont need to.
                if (this.IsIndexPastBounds(index))
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

                if (this.IsIndexPastBounds(index))
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
}
