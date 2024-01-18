﻿using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace CUE4Parse_Conversion.Textures.ASTC
{
    public class BitArrayStream
    {
        public BitArray BitsArray;
        public int Position { get; private set; }

        public BitArrayStream(BitArray BitArray)
        {
            BitsArray = BitArray;
            Position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadBits(int Length)
        {
            int RetValue = 0;
            for (int i = Position; i < Position + Length; i++)
            {
                if (BitsArray[i])
                {
                    RetValue |= 1 << (i - Position);
                }
            }
            Position += Length;
            return (short)RetValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadBits(int Start, int End)
        {
            int RetValue = 0;
            for (int i = Start; i <= End; i++)
            {
                if (BitsArray[i])
                {
                    RetValue |= 1 << (i - Start);
                }
            }
            return RetValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadBit(int Index)
        {
            return Convert.ToInt32(BitsArray[Index]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBits(int Value, int Length)
        {
            for (int i = Position; i < Position + Length; i++)
            {
                BitsArray[i] = ((Value >> (i - Position)) & 1) != 0;
            }
            Position += Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ToByteArray()
        {
            byte[] RetArray = new byte[(BitsArray.Length + 7) / 8];
            BitsArray.CopyTo(RetArray, 0);
            return RetArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Replicate(int Value, int NumberBits, int ToBit)
        {
            if (NumberBits == 0) return 0;
            if (ToBit == 0) return 0;

            int TempValue = Value & ((1 << NumberBits) - 1);
            int RetValue = TempValue;
            int ResLength = NumberBits;

            while (ResLength < ToBit)
            {
                int Comp = 0;
                if (NumberBits > ToBit - ResLength)
                {
                    int NewShift = ToBit - ResLength;
                    Comp = NumberBits - NewShift;
                    NumberBits = NewShift;
                }
                RetValue <<= NumberBits;
                RetValue |= TempValue >> Comp;
                ResLength += NumberBits;
            }
            return RetValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PopCnt(int Number)
        {
            int Counter;
            for (Counter = 0; Number != 0; Counter++)
            {
                Number &= Number - 1;
            }
            return Counter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T Temp = lhs;
            lhs = rhs;
            rhs = Temp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BitTransferSigned(ref int a, ref int b)
        {
            b >>= 1;
            b |= a & 0x80;
            a >>= 1;
            a &= 0x3F;
            if ((a & 0x20) != 0) a -= 0x40;
        }
    }
}
