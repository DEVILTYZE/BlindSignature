using System;
using System.Numerics;
using BlindSignature.Models;

namespace BlindSignature.Helpers
{
    public static class BlindSignatureGenerator
    {
        public static (int, IntNumberArray) SignByOpenKey(IntNumberArray number, Key openKey)
        {
            var randomValue = GetRandomValue(openKey);
            var bigRandomValue = new BigInteger(randomValue);
            //var bigExponent = new BigInteger(openKey.Exponent);
            var bigModule = new BigInteger(openKey.Module);
            
            for (var i = 0; i < number.Length; ++i)
            {
                var bigNumber = new BigInteger(number[i]);
                number[i] = (int)BigInteger.Remainder(BigInteger.Multiply(bigNumber, BigInteger.Pow(
                    bigRandomValue, openKey.Exponent)), bigModule);
                //number[i] = (int)BigInteger.ModPow(BigInteger.Multiply(bigNumber, bigRandomValue), 
                //    bigValue, bigModule);
                //number[i] = (int)(number[i] * ((long)randomValue ^ openKey.Value) % openKey.Module);
            }

            return (randomValue, number);
        }

        public static IntNumberArray SignByClosedKey(IntNumberArray number, Key closedKey)
        {
            var bigExponent = new BigInteger(closedKey.Exponent);
            var bigModule = new BigInteger(closedKey.Module);
            
            for (var i = 0; i < number.Length; ++i)
            {
                var bigNumber = new BigInteger(number[i]);
                number[i] = (int)BigInteger.ModPow(bigNumber, bigExponent, bigModule);
                //number[i] = (int)(((long)number[i] ^ closedKey.Value) % closedKey.Module);
            }

            return number;
        }

        public static IntNumberArray RemoveSignByOpenKey(IntNumberArray number, Key openKey, int randomValue)
        {
            var inverseRandomValue = GetInverseNumber(randomValue, openKey.Module);
            var bigInverseRandomValue = new BigInteger(inverseRandomValue);
            var bigModule = new BigInteger(openKey.Module);

            for (var i = 0; i < number.Length; ++i)
            {
                var bigNumber = new BigInteger(number[i]);
                number[i] = (int)BigInteger.Remainder(BigInteger.Multiply(bigNumber, 
                    bigInverseRandomValue), bigModule);
                //number[i] = (int)((long)number[i] * inverseRandomValue % openKey.Module);
            }

            return number;
        }

        // BYTE SIGN
        #region ByteSign
        
        public static (int, byte[]) SignByOpenKey(byte[] message, Key openKey)
        {
            var randomValue = GetRandomValue(openKey);
            var resultArray = new byte[message.Length];
            
            for (var i = 0; i < message.Length; ++i)
                resultArray[i] = (byte)(message[i] * ((long)randomValue ^ openKey.Exponent) % openKey.Module);

            return (randomValue, resultArray);
        }

        public static byte[] SignByClosedKey(byte[] message, Key closedKey)
        {
            var resultArray = new byte[message.Length];

            for (var i = 0; i < message.Length; ++i)
                resultArray[i] = (byte)((long)message[i] ^ closedKey.Exponent % closedKey.Module);

            return resultArray;
        }

        public static byte[] RemoveSignByOpenKey(byte[] message, Key openKey, int randomValue)
        {
            var inverseRandomValue = GetInverseNumber(randomValue, openKey.Module);
            var resultArray = new byte[message.Length];

            for (var i = 0; i < message.Length; ++i)
                resultArray[i] = (byte)((long)message[i] * inverseRandomValue % openKey.Module);

            return resultArray;
        }

        #endregion

        public static (Key, Key) GenerateRandomKeys()
        {
            var random = new Random();
            int q, p, e;

            do
            {
                q = random.Next(ConstHelper.MaxPrimeNumber / 4, ConstHelper.MaxPrimeNumber);
            } 
            while (!IsPrimeNumber(q));

            do
            {
                p = random.Next(ConstHelper.MaxPrimeNumber / 4, ConstHelper.MaxPrimeNumber);
            } 
            while (!IsPrimeNumber(p));

            do
            {
                e = random.Next(ConstHelper.MaxPrimeNumber / 4, ConstHelper.MaxPrimeNumber);
            } 
            while (!IsPrimeNumber(e));

            var d = GetInverseNumber(e, (p - 1) * (q - 1));
            var openKey = new Key(p * q, e);
            var closedKey = new Key(p * q, d);

            return (openKey, closedKey);
        }

        private static int GetRandomValue(Key openKey)
        {
            var random = new Random();
            int randomValue;
            
            do
            {
                randomValue = random.Next(5, openKey.Module);
            } 
            while (!IsPrimeNumber(randomValue) || !IsCoPrimeNumbers(openKey.Module, randomValue));

            return randomValue;
        }
        
        private static bool IsCoPrimeNumbers(int value1, int value2)
            => BigInteger.GreatestCommonDivisor(new BigInteger(value1), new BigInteger(value2)) == 1;
        

        private static bool IsPrimeNumber(int value)
        {
            for (var i = 2; i < value; ++i)
                if (value % i == 0)
                    return false;

            return true;
        }

        // private static int GetInverseNumber(int number, int module)
        // {
        //     var (a, b) = module > number ? (module, number) : (number, module);
        //     var r = (a, b);
        //     var s = (1, 0);
        //     var t = (0, 1);
        //
        //     while (r.b != 0)
        //     {
        //         var temp = r.a / r.b;
        //         r = (r.b, r.a - temp * r.b);
        //         s = (s.Item2, s.Item1 - temp * s.Item2);
        //         t = (t.Item2, t.Item1 - temp * t.Item2);
        //     }
        //
        //     return t.Item1;
        // }

        private static int GetInverseNumber(int number, int module)
        {
            var bigNumber = new BigInteger(number);
            var bigModule = new BigInteger(module);
            var bigX = BigInteger.One;
            
            while (BigInteger.Remainder(BigInteger.Multiply(bigNumber, bigX), bigModule) != BigInteger.One)
                bigX = BigInteger.Add(bigX, BigInteger.One);

            return (int)bigX;
        }
    }
}