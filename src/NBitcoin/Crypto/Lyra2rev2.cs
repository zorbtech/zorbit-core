using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace NBitcoin.Crypto
{
    public sealed unsafe class Lyra2rev2 : IHashProvider
    {
        [DllImport(@"Zorbit.MultiHash_x64.dll", EntryPoint = "lyra2rev2_export", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hash_x64(byte* input, byte* output);

        [DllImport(@"Zorbit.MultiHash_x86.dll", EntryPoint = "lyra2rev2_export", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hash_x86(byte* input, byte* output);
        
        private static readonly Lazy<Lyra2rev2> SingletonInstance = new Lazy<Lyra2rev2>(LazyThreadSafetyMode.PublicationOnly);

        public static Lyra2rev2 Instance => SingletonInstance.Value;

        public static Lyra2rev2 Create()
        {
            return new Lyra2rev2();
        }

        public uint256 Hash(byte[] data)
        {
            byte[] buffer = new byte[32];

            fixed (byte* input = data)
            {
                fixed (byte* output = buffer)
                {
                    if (Environment.Is64BitProcess)
                    {
                        hash_x64(input, output);
                    }
                    else
                    {
                        hash_x86(input, output);
                    }
                }
            }

            return new uint256(buffer.Take(32).ToArray());
        }
    }
}