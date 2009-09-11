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
        private AbstractValue[] storage;

        public AbstractBuffer(AbstractValue[] values)
        {
            storage = values;
            Length = storage.Length;
        }

        public AbstractBuffer(AbstractBuffer other)
        {
            BaseIndex = other.BaseIndex;
            Length = other.Length;
            storage = new AbstractValue[other.storage.Length];
            Array.Copy(other.storage, storage, other.storage.Length);
        }

        private UInt32 BaseIndex { get; set; }

        public int Length { get; private set; }

        public AbstractValue this[Int32 index]
        {
            get
            {
                // We check this.storage.Length as well so that we aren't calling Extend() when we dont need to.
                if (IsIndexPastBounds(index))
                {
                    Extend(BaseIndex + (UInt32) index);
                    return storage[BaseIndex + index];
                }

                return storage[BaseIndex + index];
            }

            set
            {
                if ((BaseIndex + index) >= Length)
                {
                    value.IsOutOfBounds = true;
                }

                if (IsIndexPastBounds(index))
                {
                    Extend(BaseIndex + (UInt32) index);
                    storage[BaseIndex + index] = value;
                }
                else
                {
                    storage[BaseIndex + index] = value;
                }
            }
        }

        public AbstractBuffer DoOperation(OperatorEffect operatorEffect, AbstractValue rhs)
        {
            var lhs = this;

            // TODO: should have a guard for if rhs isn't a pointer
            switch (operatorEffect)
            {
                case OperatorEffect.Assignment:
                {
                    var result = new AbstractBuffer(lhs);

                    return result;
                }

                case OperatorEffect.Add:
                {
                    var result = new AbstractBuffer(lhs);
                    result.BaseIndex += rhs.Value;

                    return result;
                }

                case OperatorEffect.Sub:
                {
                    var result = new AbstractBuffer(lhs);

                    if (result.BaseIndex <
                        rhs.Value)
                    {
                        throw new ArgumentOutOfRangeException(
                            String.Format(
                                "Attempting to set a negative baseindex, baseindex: {0:x4}, _subValue {1:x4}",
                                result.BaseIndex,
                                rhs.Value
                                )
                            );
                    }

                    result.BaseIndex -= rhs.Value;
                    return result;
                }

                case OperatorEffect.And:
                {
                    var result = new AbstractBuffer(lhs);

                    result.BaseIndex &= rhs.Value;
                    return result;
                }

                default:
                    throw new ArgumentException(
                        String.Format("Unsupported OperatorEffect: {0}", operatorEffect), "operatorEffect");
            }
        }

        private Boolean IsIndexPastBounds(Int32 index)
        {
            return ((BaseIndex + index) >= Length) && ((BaseIndex + index) >= storage.Length);
        }

        private void Extend(UInt32 newLength)
        {
            // Account for element [0] of the array
            newLength = newLength + 1;
            if (newLength >= Length)
            {
                var extendedCopy = new AbstractValue[newLength];

                Int32 i;
                for (i = 0; i < storage.Length; i++)
                {
                    extendedCopy[i] = storage[i];
                }

                for (; i < newLength; i++)
                {
                    extendedCopy[i] = new AbstractValue(AbstractValue.UNKNOWN)
                                      {
                                          IsOutOfBounds = true
                                      };
                }

                storage = extendedCopy;
            }

            return;
        }
    }
}