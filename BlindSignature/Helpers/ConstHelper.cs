namespace BlindSignature.Helpers
{
    public static class ConstHelper
    {
        // Commands
        // Separator — 255 255 255 255;
        // Get open key — 255 255 255 254;
        // Sign message — 255 255 255 253;
        // End connection — 255 255 255 252;
        public static readonly byte[] Separator = { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue };
        public const string CommandGetOpenKey = "GET_OPEN_KEY";
        public const string CommandSignMessage = "SIGN_MESSAGE";
        public const string CommandEndConnection = "END_CONNECTION";

        public const string LocalName = "localhost";
        public const string LocalAddress = "127.0.0.1";
        public const int Port = 13000;
        public const int StreamLength = 256;

        public const int MaxPrimeNumber = 313;
    }
}