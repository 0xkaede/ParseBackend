﻿using System.Collections;
using System.Collections.Generic;
using CUE4Parse.UE4.Readers;
using CUE4Parse.Utils;

namespace CUE4Parse.UE4.Assets.Objects.Unversioned
{
    public class FUnversionedHeader
    {
        public IReadOnlyList<FFragment> Fragments;
        public BitArray ZeroMask;
        public readonly bool HasNonZeroValues;
        public bool HasValues => HasNonZeroValues | ZeroMask.Length > 0;
        
        public FUnversionedHeader(FArchive Ar)
        {
            var fragments = new List<FFragment>();
            
            FFragment fragment;
            var zeroMaskNum = 0;
            uint unmaskedNum = 0;
            
            do
            {
                fragment = new FFragment(Ar.Read<ushort>());
                fragments.Add(fragment);
                
                if (fragment.HasAnyZeroes)
                    zeroMaskNum += fragment.ValueNum;
                else
                    unmaskedNum += fragment.ValueNum;
            } while (!fragment.IsLast);

            if (zeroMaskNum > 0)
            {
                LoadZeroMaskData(Ar, zeroMaskNum, out ZeroMask);
                HasNonZeroValues = unmaskedNum > 0 || ZeroMask.Contains(false);
            }
            else
            {
                ZeroMask = new BitArray(0);
                HasNonZeroValues = unmaskedNum > 0;
            }
            Fragments = fragments;
        }
        
        private static void LoadZeroMaskData(FArchive reader, int numBits, out BitArray data)
        {
            if (numBits <= 8)
            {
                data = new BitArray(new[] { reader.Read<byte>() });
            }
            else if (numBits <= 16)
            {
                data = new BitArray(new []{ (int) reader.Read<ushort>() });
            }
            else
            {
                var num = numBits.DivideAndRoundUp(32);
                var intData = new int[num];
                for (int idx = 0; idx < num; idx++)
                {
                    intData[idx] = reader.Read<int>();
                }
                data = new BitArray(intData);
            }

            data.Length = numBits;
        }
    }
}