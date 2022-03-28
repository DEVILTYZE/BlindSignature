using System;
using BlindSignature.Models;

namespace BlindSignature.Helpers
{
    public static class BlindSignatureGenerator
    {
        private const int MaxPrimeNumber = 113; //313;

        public static (int, int) SignByOpenKey(int message, Key openKey)
        {
            var randomValue = GetRandomValue(openKey);
            var result = (decimal)message * ((long)randomValue ^ openKey.Value) % openKey.Module;

            return (randomValue, (int)result);
        }

        public static int SignByClosedKey(int message, Key closedKey) 
            => (int)((long)message ^ closedKey.Value % closedKey.Module);

        public static int RemoveSignByOpenKey(int signedMessage, Key openKey, int randomValue)
        {
            var negativeRandomValue = GetInverseRandomValue(randomValue, openKey.Module);

            return (int)((decimal)signedMessage * negativeRandomValue % openKey.Module);
        }

        public static Key GenerateRandomKey()
        {
            var random = new Random();
            int module, value;
            
            do
            {
                module = random.Next(MaxPrimeNumber / 2, MaxPrimeNumber);
            } 
            while (IsPrimeNumber(module));
            
            do
            {
                value = random.Next(MaxPrimeNumber / 2, MaxPrimeNumber);
            } 
            while (IsPrimeNumber(value));

            return new Key(module, value);
        }

        private static int GetRandomValue(Key openKey)
        {
            var random = new Random();
            int randomValue;
            
            do
            {
                randomValue = random.Next(10, openKey.Module);
            } 
            while (IsCoPrimeNumbers(openKey.Module, randomValue));

            return randomValue;
        }
        
        private static bool IsCoPrimeNumbers(int value1, int value2)
        {
            if (value1 < value2)
            {
                value1 += value2;
                value2 = value1 - value2;
                value1 -= value2;
            }

            int remainder;

            do
            {
                remainder = value1 % value2;
                value1 = value2;
                value2 = remainder;
            } 
            while (remainder != 0);

            return value2 == 1;
        }

        private static bool IsPrimeNumber(int value)
        {
            for (var i = 2; i < value; ++i)
                if (value % i == 0)
                    return false;

            return true;
        }

        private static int GetInverseRandomValue(int randomValue, int module)
        {
            var s = (1, 0);
            var r = randomValue < module ? (module, randomValue) : (randomValue, module);

            while (r.Item2 != 0)
            {
                var temp = r.Item1 / r.Item2;
                r = (r.Item2, r.Item1 - temp * r.Item2);
                s = (s.Item2, s.Item1 - temp * s.Item2);
            }

            return randomValue < module 
                ? randomValue == 0 ? 0 : (r.Item1 - s.Item1 * module) / randomValue 
                : module == 0 ? 0 : (r.Item1 - s.Item1 * randomValue) / module;
        }
    }
}