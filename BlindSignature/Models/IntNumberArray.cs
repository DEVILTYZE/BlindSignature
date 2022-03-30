using System;
using System.Collections.Generic;

namespace BlindSignature.Models
{
    public class IntNumberArray : ICloneable, IComparable<IntNumberArray>
    {
        private const int ByteArrayLength = 4;
        private readonly int[] _array;

        public int Length => _array.Length;

        public IntNumberArray() => _array = new int[1];

        public IntNumberArray(IReadOnlyList<byte> array)
        {
            _array = new int[array.Count];

            for (var i = 0; i < array.Count; ++i)
                _array[i] = array[i];
        }

        private IntNumberArray(int[] array) => _array = array;

        public int this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public IEnumerable<byte> GetBytes()
        {
            var result = new byte[Length * ByteArrayLength];

            for (var i = 0; i < Length; ++i)
            {
                var intByteArray = BitConverter.GetBytes(_array[i]);

                for (var j = 0; j < ByteArrayLength; ++j)
                    result[i * ByteArrayLength + j] = intByteArray[j];
            }

            return result;
        }

        public static IntNumberArray GetNumber(byte[] array)
        {
            var result = new int[array.Length / ByteArrayLength];

            for (var i = 0; i < result.Length; ++i)
                result[i] = BitConverter.ToInt32(array, i * ByteArrayLength);

            return new IntNumberArray(result);
        }

        public int CompareTo(IntNumberArray other)
        {
            if (Length > other.Length)
                return 1;
            
            if (Length < other.Length)
                return -1;

            for (var i = 0; i < Length; ++i)
            {
                var result = _array[i].CompareTo(other._array[i]);
                
                if (result != 0)
                    return result;
            }

            return 0;
        }

        public override string ToString() => string.Join(" ", _array);

        public object Clone()
        {
            var newArray = new int[_array.Length];
            _array.CopyTo(newArray, 0);
            
            return new IntNumberArray(newArray);
        }
    }
}