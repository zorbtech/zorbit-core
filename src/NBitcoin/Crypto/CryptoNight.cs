using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace NBitcoin.Crypto
{
    public sealed unsafe class CryptoNight : IHashProvider
    {
        [DllImport(@"Zorbit.CryptoNight_x64.dll", EntryPoint = "cn_slow_hash_export", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hash_x64(byte* input, byte* output, uint inputLength);

        [DllImport(@"Zorbit.CryptoNight_x86.dll", EntryPoint = "cn_slow_hash_export", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hash_x86(byte* input, byte* output, uint inputLength);
        
        private static readonly Lazy<CryptoNight> SingletonInstance = new Lazy<CryptoNight>(LazyThreadSafetyMode.PublicationOnly);

        public static CryptoNight Instance => SingletonInstance.Value;

        public static CryptoNight Create()
        {
            return new CryptoNight();
        }

        public uint256 Hash(byte[] data)
        {
            byte[] buffer = new byte[32];

            uint length = (uint)data.Length;

            fixed (byte* input = data)
            {
                fixed (byte* output = buffer)
                {
                    if (Environment.Is64BitProcess)
                    {
                        hash_x64(input, output, length);
                    }
                    else
                    {
                        hash_x86(input, output, length);
                    }
                }
            }

            return new uint256(buffer.Take(32).ToArray());
        }
    }
}
