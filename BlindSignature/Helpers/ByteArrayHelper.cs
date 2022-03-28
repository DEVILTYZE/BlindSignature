using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlindSignature.Helpers
{
    public static class ByteArrayHelper
    {
        public static KeyValuePair<int, int> GetOffsetOfSeparator(byte[] array, bool startsWithEnd = false)
        {
            var startIndex = 0;
            var endIndex = array.Length - ConstHelper.Separator.Length;
            var step = 1;
            var compareFunction = (Func<int, int, bool>)((index, end) => index < end);

            if (startsWithEnd)
            {
                startIndex = array.Length - 1;
                endIndex = ConstHelper.Separator.Length;
                step = -1;
                compareFunction = (index, end) => index >= end;
            }
            
            for (var i = startIndex; compareFunction.Invoke(i, endIndex); i += step)
                if (array[i] == byte.MaxValue && new ArraySegment<byte>(array, i, ConstHelper.Separator.Length)
                        .All(element => element == byte.MaxValue))
                    return new KeyValuePair<int, int>(i, i + ConstHelper.Separator.Length);

            return new KeyValuePair<int, int>(-1, -1);
        }

        public static bool IsEqualsArrays(byte[] original, byte[] modified)
        {
            if (original.Length != modified.Length)
                return false;

            return !original.Where((element, i) => element != modified[i]).Any();
        }

        public static byte[] RemoveEndSeparator(byte[] array)
        {
            var (index, _) = GetOffsetOfSeparator(array, true);

            if (index == -1)
                throw new ArgumentException("Неверные данные!");

            return new ArraySegment<byte>(array, 0, index).ToArray();
        }

        public static bool IsInvalidData(byte[] array)
        {
            if (array[0] != byte.MaxValue) 
                return false;
            
            for (var i = 1; i < array.Length; ++i)
                if (array[i] != byte.MinValue)
                    return false;
            
            return true;
        }

        public static byte[] GetCommandAndDataByteArray(string command, IEnumerable<byte> data = null)
        {
            if (data is null)
                return Encoding.UTF8.GetBytes(command).Concat(ConstHelper.Separator).ToArray();
            
            return Encoding.UTF8.GetBytes(command).Concat(ConstHelper.Separator).Concat(data)
                .Concat(ConstHelper.Separator).ToArray();
        }
    }
}