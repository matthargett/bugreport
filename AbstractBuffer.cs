// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;

namespace bugreport
{
    public sealed class AbstractBuffer
    {
        private readonly Int32 allocatedLength;
        private UInt32 baseIndex;
        private AbstractValue[] storage;

        public AbstractBuffer(AbstractValue[] _buffer)
        {
            storage = _buffer;
            allocatedLength = storage.Length;
        }

        public AbstractBuffer(AbstractBuffer _copyMe)
        {
            baseIndex = _copyMe.BaseIndex;
            allocatedLength = _copyMe.allocatedLength;
            storage = new AbstractValue[_copyMe.storage.Length];
            Array.Copy(_copyMe.storage, storage, _copyMe.storage.Length);
        }

        private UInt32 BaseIndex
        {
            get { return baseIndex; }
        }

        public Int32 Length
        {
            get { return allocatedLength; }
        }

        public AbstractValue this[Int32 index]
        {
            get
            {
                // We check this.storage.Length as well so that we aren't calling Extend() when we dont need to.
                if (IsIndexPastBounds(index))
                {
                    extend(baseIndex + (UInt32) index);
                    return storage[baseIndex + index];
                }

                return storage[baseIndex + index];
            }

            set
            {
                if ((baseIndex + index) >= allocatedLength)
                {
                    value.IsOOB = true;
                }

                if (IsIndexPastBounds(index))
                {
                    extend(baseIndex + (UInt32) index);
                    storage[baseIndex + index] = value;
                }
                else
                {
                    storage[baseIndex + index] = value;
                }
            }
        }

        public AbstractBuffer DoOperation(OperatorEffect _operatorEffect, AbstractValue _rhs)
        {
            var lhs = this;

            // TODO: should have a guard for if _rhs isn't a pointer
            switch (_operatorEffect)
            {
                case OperatorEffect.Assignment:
                {
                    var result = new AbstractBuffer(lhs);

                    return result;
                }

                case OperatorEffect.Add:
                {
                    var result = new AbstractBuffer(lhs);
                    result.baseIndex += _rhs.Value;

                    return result;
                }

                case OperatorEffect.Sub:
                {
                    var result = new AbstractBuffer(lhs);

                    if (result.baseIndex < _rhs.Value)
                    {
                        throw new ArgumentOutOfRangeException(
                            String.Format(
                                "Attempting to set a negative baseindex, baseindex: {0:x4}, _subValue {1:x4}",
                                result.baseIndex,
                                _rhs.Value
                                )
                            );
                    }

                    result.baseIndex -= _rhs.Value;
                    return result;
                }

                case OperatorEffect.And:
                {
                    var result = new AbstractBuffer(lhs);

                    result.baseIndex &= _rhs.Value;
                    return result;
                }

                default:
                    throw new ArgumentException(
                        String.Format("Unsupported OperatorEffect: {0}", _operatorEffect), "_operatorEffect");
            }
        }

        private Boolean IsIndexPastBounds(Int32 index)
        {
            return ((baseIndex + index) >= allocatedLength) && ((baseIndex + index) >= storage.Length);
        }

        private void extend(UInt32 _newLength)
        {
            // Account for element [0] of the array
            _newLength = _newLength + 1;
            if (_newLength >= Length)
            {
                var _copyTo = new AbstractValue[_newLength];

                Int32 i;
                for (i = 0; i < storage.Length; i++)
                {
                    _copyTo[i] = storage[i];
                }

                for (; i < _newLength; i++)
                {
                    _copyTo[i] = new AbstractValue(AbstractValue.UNKNOWN) {IsOOB = true};
                }

                storage = _copyTo;
            }

            return;
        }
    }
}