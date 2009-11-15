// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace bugreport
{
    public struct OperationResult
    {
        public AbstractValue Value;
        public Boolean ZeroFlag;

        public OperationResult(AbstractValue value, Boolean zeroFlag)
        {
            Value = value;
            ZeroFlag = zeroFlag;
        }

        public static Boolean operator ==(OperationResult a, OperationResult b)
        {
            return a.Equals(b);
        }

        public static Boolean operator !=(OperationResult a, OperationResult b)
        {
            return !a.Equals(b);
        }

        public override Boolean Equals(object obj)
        {
            var operationResult = (OperationResult)obj;
            return Value.Equals(operationResult.Value) && ZeroFlag == operationResult.ZeroFlag;
        }

        public override Int32 GetHashCode()
        {
            return Value.GetHashCode() ^ ZeroFlag.GetHashCode();
        }
    }

    public struct MachineState
    {
        private readonly Dictionary<UInt32, AbstractValue> dataSegment;
        private readonly RegisterCollection registers;
        private UInt32 instructionPointer;
        private Boolean zeroFlag;

        public MachineState(RegisterCollection registers)
        {
            dataSegment = new Dictionary<UInt32, AbstractValue>();
            this.registers = registers;
            instructionPointer = 0x00;
            zeroFlag = false;
        }

        public MachineState(MachineState copyMe)
        {
            instructionPointer = copyMe.instructionPointer;
            registers = new RegisterCollection(copyMe.registers);
            dataSegment = new Dictionary<UInt32, AbstractValue>(copyMe.dataSegment);
            zeroFlag = copyMe.zeroFlag;
        }

        public Boolean ZeroFlag
        {
            get { return zeroFlag; }
            private set { zeroFlag = value; }
        }

        public UInt32 InstructionPointer
        {
            get { return instructionPointer; }
            set { instructionPointer = value; }
        }

        public RegisterCollection Registers
        {
            get { return registers; }
        }

        public Dictionary<UInt32, AbstractValue> DataSegment
        {
            get { return dataSegment; }
        }

        public AbstractValue TopOfStack
        {
            get
            {
                Debug.Assert(registers[RegisterName.ESP] != null);
                Debug.Assert(registers[RegisterName.ESP].PointsTo != null);
                return registers[RegisterName.ESP].PointsTo[0];
            }

            private set
            {
                Debug.Assert(registers[RegisterName.ESP] != null);
                Debug.Assert(registers[RegisterName.ESP].PointsTo != null);
                registers[RegisterName.ESP].PointsTo[0] = value;
            }
        }

        public AbstractValue ReturnValue
        {
            get { return registers[RegisterName.EAX]; }
            set { registers[RegisterName.EAX] = value; }
        }

        public static Boolean operator ==(MachineState a, MachineState b)
        {
            return a.Equals(b);
        }

        public static Boolean operator !=(MachineState a, MachineState b)
        {
            return !a.Equals(b);
        }

        public override Boolean Equals(object obj)
        {
            var other = (MachineState)obj;

            if (!(instructionPointer == other.instructionPointer &&
                  registers.Equals(other.registers) &&
                  zeroFlag == other.zeroFlag))
            {
                return false;
            }

            foreach (var key in dataSegment.Keys)
            {
                if (!other.dataSegment.ContainsKey(key))
                {
                    return false;
                }

                if (!dataSegment[key].Equals(other.dataSegment[key]))
                {
                    return false;
                }
            }

            return true;
        }

        public override Int32 GetHashCode()
        {
            var hashCode = instructionPointer.GetHashCode() ^
                           registers.GetHashCode() ^
                           zeroFlag.GetHashCode();

            foreach (var key in dataSegment.Keys)
            {
                hashCode ^= dataSegment[key].GetHashCode();
            }

            return hashCode;
        }

        public MachineState PushOntoStack(AbstractValue value)
        {
            var newState = DoOperation(RegisterName.ESP, OperatorEffect.Add, new AbstractValue(0x4));
            newState.TopOfStack = new AbstractValue(value);
            return newState;
        }

        public MachineState DoOperation(OperatorEffect operatorEffect, AbstractValue offset)
        {
            if (offset.IsPointer)
            {
                throw new ArgumentException("_offset pointer not supported.");
            }

            var newState = new MachineState(this);
            switch (operatorEffect)
            {
                case OperatorEffect.Jnz:
                {
                    if (!newState.zeroFlag)
                    {
                        newState.instructionPointer += offset.Value;
                    }

                    break;
                }

                default:
                {
                    throw new ArgumentException(
                        String.Format("Unsupported OperatorEffect: {0}", operatorEffect), "operatorEffect");
                }
            }

            return newState;
        }

        public MachineState DoOperation(UInt32 offset, OperatorEffect operatorEffect, AbstractValue rhs)
        {
            var newState = new MachineState(this);

            switch (operatorEffect)
            {
                case OperatorEffect.Assignment:
                {
                    newState.dataSegment[offset] =
                        DoOperation(newState.dataSegment[offset], operatorEffect, rhs).Value;
                    break;
                }
            }

            return newState;
        }

        public MachineState DoOperation(RegisterName lhs, Int32 index, OperatorEffect operatorEffect, AbstractValue rhs)
        {
            var newState = new MachineState(this);

            switch (operatorEffect)
            {
                case OperatorEffect.Assignment:
                case OperatorEffect.Cmp:
                {
                    var result = DoOperation(Registers[lhs].PointsTo[index], operatorEffect, rhs);
                    newState.Registers[lhs].PointsTo[index] = result.Value;
                    newState.ZeroFlag = result.ZeroFlag;
                    break;
                }
            }

            return newState;
        }

        public MachineState DoOperation(RegisterName lhs, OperatorEffect operatorEffect, AbstractValue rhs)
        {
            var newState = new MachineState(this);

            if (Registers[lhs].IsPointer &&
                operatorEffect != OperatorEffect.Assignment)
            {
                var newBuffer = Registers[lhs].PointsTo.DoOperation(operatorEffect, rhs);
                newState.Registers[lhs] = new AbstractValue(newBuffer);
            }
            else
            {
                var result = DoOperation(Registers[lhs], operatorEffect, rhs);
                newState.Registers[lhs] = result.Value;
                newState.ZeroFlag = result.ZeroFlag;
            }

            return newState;
        }

        public MachineState DoOperation(RegisterName lhs, OperatorEffect operatorEffect, RegisterName rhs)
        {
            var newState = new MachineState(this);
            var result = DoOperation(Registers[lhs], operatorEffect, Registers[rhs]);
            newState.Registers[lhs] = result.Value;
            newState.ZeroFlag = result.ZeroFlag;
            return newState;
        }

        private static OperationResult DoOperation(AbstractValue lhs, OperatorEffect operatorEffect, AbstractValue rhs)
        {
            if (rhs.IsPointer &&
                operatorEffect != OperatorEffect.Assignment)
            {
                throw new ArgumentException("rhs pointer only supported for OperatorEffect.Assignment.");
            }

            var result = new OperationResult();
            AbstractValue totalValue;

            if (operatorEffect == OperatorEffect.Assignment)
            {
                var newValue = new AbstractValue(rhs);
                if (rhs.IsInitialized &&
                    rhs.IsOutOfBounds)
                {
                    newValue.IsOutOfBounds = true;
                }

                result.Value = newValue;

                return result;
            }

            if (operatorEffect == OperatorEffect.Cmp)
            {
                result.ZeroFlag = (lhs.Value - rhs.Value) == 0;

                totalValue = lhs;
                result.Value = totalValue;
                return result;
            }

            if (lhs.IsPointer)
            {
                var newBuffer = lhs.PointsTo.DoOperation(operatorEffect, rhs);
                result.Value = new AbstractValue(newBuffer);
                return result;
            }

            var total = CalculateValueFor(lhs.Value, operatorEffect, rhs.Value);
            totalValue = new AbstractValue(total);

            if (lhs.IsTainted ||
                rhs.IsTainted)
            {
                totalValue = totalValue.AddTaint();
            }

            result.Value = totalValue;
            return result;
        }

        private static UInt32 CalculateValueFor(UInt32 lhs, OperatorEffect operatorEffect, UInt32 rhs)
        {
            switch (operatorEffect)
            {
                case OperatorEffect.Add:
                {
                    return lhs + rhs;
                }

                case OperatorEffect.Sub:
                {
                    return lhs - rhs;
                }

                case OperatorEffect.And:
                {
                    return lhs & rhs;
                }

                case OperatorEffect.Xor:
                {
                    return lhs ^ rhs;
                }

                case OperatorEffect.Shr:
                {
                    return lhs >> (Byte)rhs;
                }

                case OperatorEffect.Shl:
                {
                    return lhs << (Byte)rhs;
                }

                default:
                {
                    throw new ArgumentException(
                        String.Format("Unsupported OperatorEffect: {0}", operatorEffect), "operatorEffect");
                }
            }
        }
    }
}