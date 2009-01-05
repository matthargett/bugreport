// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;

namespace bugreport
{
    public struct OperationResult
    {
        public AbstractValue Value;
        public Boolean ZeroFlag;

        public OperationResult(AbstractValue value, Boolean zeroFlag)
        {
            this.Value = value;
            this.ZeroFlag = zeroFlag;
        }

        public override Boolean Equals(object obj)
        {
            OperationResult opResult = (OperationResult)obj;
            return (this.Value.Equals(opResult.Value)) && (this.ZeroFlag == opResult.ZeroFlag);
        }

        public override Int32 GetHashCode()
        {
            return this.Value.GetHashCode() ^ this.ZeroFlag.GetHashCode();
        }

        public static Boolean operator== (OperationResult a, OperationResult b)
        {
            return a.Equals(b);
        }

        public static Boolean operator!= (OperationResult a, OperationResult b)
        {
            return !a.Equals(b);
        }
    }

    public struct MachineState
    {
        private UInt32 instructionPointer;
        private readonly RegisterCollection registers;
        private readonly Dictionary<UInt32, AbstractValue> dataSegment;
        Boolean zeroFlag;

        public MachineState(RegisterCollection registers)
        {
            dataSegment = new Dictionary<UInt32, AbstractValue>();
            this.registers = registers;
            instructionPointer = 0x00;
            zeroFlag = false;
        }

        public MachineState(MachineState copyMe)
        {
            this.instructionPointer = copyMe.instructionPointer;
            this.registers = new RegisterCollection(copyMe.registers);
            this.dataSegment = new Dictionary<UInt32, AbstractValue>(copyMe.dataSegment);
            this.zeroFlag = copyMe.zeroFlag;
        }

        public override Boolean Equals(object obj)
        {
            MachineState other = (MachineState)obj;

            if (!(this.instructionPointer == other.instructionPointer &&
                    this.registers.Equals(other.registers) &&
                    this.zeroFlag == other.zeroFlag))
            {
                return false;
            }

            foreach (UInt32 key in this.dataSegment.Keys)
            {
                if (!other.dataSegment.ContainsKey(key))
                {
                    return false;
                }

                if (!this.dataSegment[key].Equals(other.dataSegment[key]))
                {
                    return false;
                }
            }

            return true;
        }

        public override Int32 GetHashCode()
        {
            Int32 hashCode = this.instructionPointer.GetHashCode() ^
                             this.registers.GetHashCode() ^
                             this.zeroFlag.GetHashCode();

            foreach (UInt32 key in this.dataSegment.Keys)
            {
                hashCode ^= this.dataSegment[key].GetHashCode();
            }

            return hashCode;
        }

        public static Boolean operator== (MachineState a, MachineState b)
        {
            return a.Equals(b);
        }

        public static Boolean operator!= (MachineState a, MachineState b)
        {
            return !a.Equals(b);
        }

        public Boolean ZeroFlag
        {
            get { return zeroFlag; }
            private set { zeroFlag = value; }
        }

        public UInt32 InstructionPointer
        {
            get
            {
                return instructionPointer;
            }
            set
            {
                instructionPointer = value;
            }
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
                System.Diagnostics.Debug.Assert(registers[RegisterName.ESP] != null);
                System.Diagnostics.Debug.Assert(registers[RegisterName.ESP].PointsTo != null);
                return registers[RegisterName.ESP].PointsTo[0];
            }

            set
            {
                System.Diagnostics.Debug.Assert(registers[RegisterName.ESP] != null);
                System.Diagnostics.Debug.Assert(registers[RegisterName.ESP].PointsTo != null);
                registers[RegisterName.ESP].PointsTo[0] = value;
            }
        }

        public AbstractValue ReturnValue
        {
            get
            {
                return registers[RegisterName.EAX];
            }
            set
            {
                registers[RegisterName.EAX] = value;
            }
        }

        public MachineState PushOntoStack(AbstractValue value)
        {
            MachineState newState = DoOperation(RegisterName.ESP, OperatorEffect.Add, new AbstractValue(0x1));
            newState.TopOfStack = new AbstractValue(value);
            return newState;
        }


        public MachineState DoOperation(OperatorEffect _operatorEffect, AbstractValue _offset)
        {
            if (_offset.IsPointer)
            {
                throw new ArgumentException("_offset pointer not supported.");
            }

            MachineState newState = new MachineState(this);
            switch (_operatorEffect)
            {
                case OperatorEffect.Jnz:
                {
                    if (!newState.zeroFlag)
                    {
                        newState.instructionPointer += _offset.Value;
                    }
        
                    break;
                }
                default:
                {
                    throw new ArgumentException(String.Format("Unsupported OperatorEffect: {0}", _operatorEffect), "_operatorEffect");
                }
            }
            return newState;
        }

        public MachineState DoOperation(UInt32 offset, OperatorEffect _operatorEffect, AbstractValue rhs)
        {
            MachineState newState = new MachineState(this);

            switch (_operatorEffect)
            {
                case OperatorEffect.Assignment:
                {
                    newState.dataSegment[offset] = newState.DoOperation(newState.dataSegment[offset], _operatorEffect, rhs).Value;
                    break;
                }
            }
            return newState;
        }

        public MachineState DoOperation(RegisterName lhs, Int32 index, OperatorEffect _operatorEffect, AbstractValue rhs)
        {
            MachineState newState = new MachineState(this);

            switch (_operatorEffect)
            {
                case OperatorEffect.Assignment:
                case OperatorEffect.Cmp:
                {
                    OperationResult result = newState.DoOperation(Registers[lhs].PointsTo[index], _operatorEffect, rhs);
                    newState.Registers[lhs].PointsTo[index] = result.Value;
                    newState.ZeroFlag = result.ZeroFlag;
                    break;
                }
            }

            return newState;
        }

        public MachineState DoOperation(RegisterName lhs, OperatorEffect _operatorEffect, AbstractValue rhs)
        {
            MachineState newState = new MachineState(this);

            if (Registers[lhs].IsPointer && _operatorEffect != OperatorEffect.Assignment)
            {
                AbstractBuffer newBuffer = Registers[lhs].PointsTo.DoOperation(_operatorEffect, rhs);
                newState.Registers[lhs] = new AbstractValue(newBuffer);
            }
            else
            {
                OperationResult result = newState.DoOperation(this.Registers[lhs], _operatorEffect, rhs);
                newState.Registers[lhs] = result.Value;
                newState.ZeroFlag = result.ZeroFlag;
            }

            return newState;
        }

        public MachineState DoOperation(RegisterName lhs, OperatorEffect _operatorEffect, RegisterName rhs)
        {
            MachineState newState = new MachineState(this);
            OperationResult result = newState.DoOperation(Registers[lhs], _operatorEffect, Registers[rhs]);
            newState.Registers[lhs] = result.Value;
            newState.ZeroFlag = result.ZeroFlag;
            return newState;
        }

        public OperationResult DoOperation(AbstractValue lhs, OperatorEffect _operatorEffect, AbstractValue rhs)
        {
            if (rhs.IsPointer && _operatorEffect != OperatorEffect.Assignment)
            {
                throw new ArgumentException("rhs pointer only supported for OperatorEffect.Assignment.");
            }

            OperationResult result = new OperationResult();
            AbstractValue totalValue;
            UInt32 total;


            if (_operatorEffect == OperatorEffect.Assignment)
            {
                AbstractValue newValue = new AbstractValue(rhs);
                if (rhs.IsInitialized && rhs.IsOOB)
                {
                    newValue.IsOOB = true;
                }

                result.Value = newValue;

                return result;
            }

            if (_operatorEffect == OperatorEffect.Cmp)
            {
                if ((lhs.Value - rhs.Value) == 0)
                {
                    result.ZeroFlag = true;
                }
                else
                {
                    result.ZeroFlag = false;
                }

                totalValue = lhs;
                result.Value = totalValue;
                return result;
            }

            if (lhs.IsPointer)
            {
                AbstractBuffer newBuffer = lhs.PointsTo.DoOperation(_operatorEffect, rhs);
                result.Value = new AbstractValue(newBuffer);
                return result;
            }

            total = getCalculatedValue(lhs.Value, _operatorEffect, rhs.Value);
            totalValue = new AbstractValue(total);

            if (lhs.IsTainted || rhs.IsTainted)
            {
                totalValue = totalValue.AddTaint();
            }

            result.Value = totalValue;
            return result;
        }

        private static UInt32 getCalculatedValue(UInt32 lhs, OperatorEffect operatorEffect, UInt32 rhs)
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
                    throw new ArgumentException(String.Format("Unsupported OperatorEffect: {0}", operatorEffect), "operatorEffect");
                }
            }
        }
    }
}
