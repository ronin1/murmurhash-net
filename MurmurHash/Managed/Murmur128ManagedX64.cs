﻿/// Copyright 2012 Darren Kopp
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///    http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Murmur
{
    internal class Murmur128ManagedX64 : Murmur128
    {
        const ulong c1 = 0x87c37b91114253d5L;
        const ulong c2 = 0x4cf5ad432745937fL;

        private ulong h1;
        private ulong h2;

        internal Murmur128ManagedX64(uint seed = 0)
            : base(seed: seed)
        {
        }

        private int Length { get; set; }

        public override void Initialize()
        {
            // initialize hash values to seed values
            h1 = h2 = Seed;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            // store the length of the hash (for use later)
            Length = cbSize;

            // only compute the hash if we have data to hash
            if (Length > 0)
                Body(array);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Body(byte[] data)
        {
            int remaining = Length;
            int position = 0;
            while (remaining >= 16)
            {
                // read our long values and increment our position by 8 bytes each time
                ulong k1 = BitConverter.ToUInt64(data, position); position += 8;
                ulong k2 = BitConverter.ToUInt64(data, position); position += 8;

                // subtract 16 bytes from our remaining count
                remaining -= 16;

                // run our hashing algorithm
                h1 = h1 ^ ((k1 * c1).RotateLeft(31) * c2);
                h1 = (h1.RotateLeft(27) + h2) * 5 + 0x52dce729;

                h2 = h2 ^ ((k2 * c2).RotateLeft(33) * c1);
                h2 = (h2.RotateLeft(31) + h1) * 5 + 0x38495ab5;
            }

            // if we still have bytes left over, run tail algorithm
            if (remaining > 0)
                Tail(data, position, remaining);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Tail(byte[] tail, int start, int remaining)
        {
            // create our keys and initialize to 0
            ulong k1 = 0, k2 = 0;

            // determine how many bytes we have left to work with based on length
            switch (remaining)
            {
                case 15: k2 ^= (ulong)tail[start + 14] << 48; goto case 14;
                case 14: k2 ^= (ulong)tail[start + 13] << 40; goto case 13;
                case 13: k2 ^= (ulong)tail[start + 12] << 32; goto case 12;
                case 12: k2 ^= (ulong)tail[start + 11] << 24; goto case 11;
                case 11: k2 ^= (ulong)tail[start + 10] << 16; goto case 10;
                case 10: k2 ^= (ulong)tail[start + 9] << 8; goto case 9;
                case 9: k2 ^= (ulong)tail[start + 8] << 0; goto case 8;
                case 8: k1 ^= (ulong)tail[start + 7] << 56; goto case 7;
                case 7: k1 ^= (ulong)tail[start + 6] << 48; goto case 6;
                case 6: k1 ^= (ulong)tail[start + 5] << 40; goto case 5;
                case 5: k1 ^= (ulong)tail[start + 4] << 32; goto case 4;
                case 4: k1 ^= (ulong)tail[start + 3] << 24; goto case 3;
                case 3: k1 ^= (ulong)tail[start + 2] << 16; goto case 2;
                case 2: k1 ^= (ulong)tail[start + 1] << 8; goto case 1;
                case 1: k1 ^= (ulong)tail[start] << 0; break;
            }

            h2 = h2 ^ ((k2 * c2).RotateLeft(33) * c1);
            h1 = h1 ^ ((k1 * c1).RotateLeft(31) * c2);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override byte[] HashFinal()
        {
            ulong len = (ulong)Length;
            h1 ^= len; h2 ^= len;

            h1 += h2;
            h2 += h1;

            h1 = fmix(h1);
            h2 = fmix(h2);

            h1 += h2;
            h2 += h1;

            var result = new byte[16];
            Array.Copy(BitConverter.GetBytes(h1), 0, result, 0, 8);
            Array.Copy(BitConverter.GetBytes(h2), 0, result, 8, 8);

            return result;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong fmix(ulong k)
        {
            k = (k ^ (k >> 33)) * 0xff51afd7ed558ccdL;
            k = (k ^ (k >> 33)) * 0xc4ceb9fe1a85ec53L;
            k ^= k >> 33;

            return k;
        }
    }
}
